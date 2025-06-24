using BlackBook.Storage;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BlackBook.Security;
    /// <summary>
    /// Handles creation, encryption, decryption, and atomic storage 
    /// of profile key‐bundle (file.file) and data container (file).
    /// </summary>
    public static class SecureProfileManager {
    // === Public API ===

    public static async Task CreateProfileAsync (
     string userName,
     string password,
     BlackBookContainer initialData,
     string usersRootDirectory,
     CancellationToken ct = default) {
        var composite = BuildCompositePassword(userName, password);
        var chachaKey = DeriveChaChaKey(composite);

        // 1) Generate & wrap your PFX
        using var cert = GenerateSelfSignedEcdhCertificate(userName, composite);
        var pfxBytes = cert.Export(X509ContentType.Pkcs12, composite);
        try {
            var wrappedPfx = ChaChaWrap(pfxBytes, chachaKey);
            await AtomicWriteAsync(Path.Combine(usersRootDirectory, userName, "file.file"), wrappedPfx, ct);
        }
        finally { CryptographicOperations.ZeroMemory(pfxBytes); }

        // 2) Derive data key and wrap initial JSON
        var dataKey = DeriveDataKeyFromEcdh(cert);
        try {
            var json = JsonSerializer.Serialize(initialData);
            var plain = Encoding.UTF8.GetBytes(json);
            var wrappedData = ChaChaWrap(plain, dataKey);
            await AtomicWriteAsync(Path.Combine(usersRootDirectory, userName, "file"), wrappedData, ct);
        }
        finally { CryptographicOperations.ZeroMemory(dataKey); }
    }



   

        /// <summary>Load an existing profile, returning the container.</summary>
        public static async Task<BlackBook.Storage.BlackBookContainer> LoadProfileAsync (
            string userName,
            string password,
            string usersRootDirectory,
            CancellationToken ct = default) {
            var composite = BuildCompositePassword(userName, password);
            var chachaKey = DeriveChaChaKey(composite);
            string dir = Path.Combine(usersRootDirectory, userName);

            // --- Unwrap file.file ---
            byte[] wrappedPfx = await File.ReadAllBytesAsync(Path.Combine(dir, "file.file"), ct);
            byte[] pfxBytes;
            try {
                pfxBytes = ChaChaUnwrap(wrappedPfx, chachaKey);
            }
            catch (CryptographicException ex) {
                throw new ProfileDecryptionException("Failed to authenticate profile key bundle.", ex);
            }

            // --- Load X509 cert & extract ECDH private key ---
            X509Certificate2 cert;
            try {
                cert = new X509Certificate2(pfxBytes, composite,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
            }
            catch (CryptographicException ex) {
                throw new ProfileAuthenticationException("Wrong profile password.", ex);
            }
            finally {
                CryptographicOperations.ZeroMemory(pfxBytes);
            }

            // --- Derive data key ---
            byte[] dataKey = DeriveDataKeyFromEcdh(cert);

            // --- Unwrap file (JSON container) ---
            byte[] wrappedData = await File.ReadAllBytesAsync(Path.Combine(dir, "file"), ct);
            byte[] plainData;
            try {
                plainData = ChaChaUnwrap(wrappedData, dataKey);
            }
            catch (CryptographicException ex) {
                throw new ProfileDecryptionException("Failed to authenticate profile data.", ex);
            }
            finally {
                CryptographicOperations.ZeroMemory(dataKey);
            }

            // --- Deserialize & return ---
            var json = Encoding.UTF8.GetString(plainData);
            CryptographicOperations.ZeroMemory(plainData);

            return JsonSerializer.Deserialize<BlackBook.Storage.BlackBookContainer>(json)
                   ?? throw new InvalidDataException("Profile JSON was empty or corrupted.");
        }

        // === High-Level Helpers ===

        private static string BuildCompositePassword (string userName, string password)
            => $"{userName}  {password}";

        private static byte[] DeriveChaChaKey (string composite) {
            using var sha = SHA512.Create();
            var full = sha.ComputeHash(Encoding.UTF8.GetBytes(composite));
            var key = new byte[32];
            Array.Copy(full, 0, key, 0, key.Length);
            CryptographicOperations.ZeroMemory(full);
            return key;
        }

        private static byte[] ChaChaWrap (byte[] plaintext, byte[] key) {
            // nonce (12) + tag (16) + ciphertext
            var result = new byte[12 + 16 + plaintext.Length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(result.AsSpan(0, 12));

            var aead = new ChaCha20Poly1305(key);
            aead.Encrypt(
                nonce: result.AsSpan(0, 12),
                plaintext: plaintext,
                ciphertext: result.AsSpan(12 + 16),
                tag: result.AsSpan(12, 16),
                associatedData: null
            );

            return result;
        }

    /// <summary>
    /// Atomically updates both the encrypted key‐bundle (file.file)
    /// and the JSON container (file).
    /// </summary>
    public static async Task SaveProfileAsync (
        string userName,
        string password,
        BlackBookContainer data,
        string usersRootDirectory,
        CancellationToken ct = default) {
        // 1) Read & unwrap the existing key‐bundle (file.file)
        string profileDir = Path.Combine(usersRootDirectory, userName);
        string keyBundle = Path.Combine(profileDir, "file.file");
        byte[] wrappedPfx = await File.ReadAllBytesAsync(keyBundle, ct).ConfigureAwait(false);
        byte[] compositeKey = DeriveChaChaKey(BuildCompositePassword(userName, password));
        byte[] pfxBytes;
        try {
            pfxBytes = ChaChaUnwrap(wrappedPfx, compositeKey);
        }
        catch (CryptographicException ex) {
            throw new InvalidDataException("Failed to authenticate key bundle.", ex);
        }

        try {
            // 2) Re-wrap the PFX with a fresh nonce/tag
            byte[] newWrappedPfx = ChaChaWrap(pfxBytes, compositeKey);
            await AtomicWriteAsync(keyBundle, newWrappedPfx, ct).ConfigureAwait(false);

            // 3) Load the cert and derive the data key
            using var cert = new X509Certificate2(
                pfxBytes,
                BuildCompositePassword(userName, password),
                X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable
            );
            byte[] dataKey = DeriveDataKeyFromEcdh(cert);

            try {
                // 4) Serialize & wrap your JSON container
                string json = JsonSerializer.Serialize(data);
                byte[] plain = Encoding.UTF8.GetBytes(json);
                byte[] wrapped = ChaChaWrap(plain, dataKey);

                string dataFile = Path.Combine(profileDir, "file");
                await AtomicWriteAsync(dataFile, wrapped, ct).ConfigureAwait(false);
            }
            finally {
                CryptographicOperations.ZeroMemory(dataKey);
            }
        }
        finally {
            CryptographicOperations.ZeroMemory(pfxBytes);
        }
    }

    private static byte[] ChaChaUnwrap (byte[] wrapped, byte[] key) {
            if (wrapped.Length < 12 + 16)
                throw new InvalidDataException("Wrapped blob is too short.");

            var nonce = wrapped.AsSpan(0, 12);
            var tag = wrapped.AsSpan(12, 16);
            var ciphertext = wrapped.AsSpan(12 + 16);
            var plain = new byte[ciphertext.Length];

            var aead = new ChaCha20Poly1305(key);
            aead.Decrypt(
                nonce: nonce,
                ciphertext: ciphertext,
                tag: tag,
                plaintext: plain,
                associatedData: null
            );

            return plain;
        }

        private static X509Certificate2 GenerateSelfSignedEcdhCertificate (string userName, string composite) {
            // Build DN
            var dn = new StringBuilder($"CN=BlackBook:{userName}");
            using var ecdh = ECDsa.Create(ECCurve.NamedCurves.nistP521);
            var req = new CertificateRequest(dn.ToString(), ecdh, HashAlgorithmName.SHA512);

            // Key usages
            req.CertificateExtensions.Add(
                new X509KeyUsageExtension(
                    X509KeyUsageFlags.KeyAgreement |
                    X509KeyUsageFlags.DigitalSignature, false));
            // Self-sign
            var cert = req.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddYears(10));

            // Export+re-import to get a Pkcs12 container
            var pfx = new X509Certificate2(
                cert.Export(X509ContentType.Pkcs12, composite),
                composite,
                X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
            return pfx;
        }





/// <summary>
/// Extracts the EC private key from the cert, imports it into an ECDH instance,
/// and does a self-key-agreement to derive a symmetric key.
/// </summary>
private static byte[] DeriveDataKeyFromEcdh (X509Certificate2 cert) {
        // 1) Grab the EC parameters (including private key) from the cert’s ECDSA object
        using var ecdsa = cert.GetECDsaPrivateKey()
                         ?? throw new InvalidOperationException("Cert has no ECDSA key.");
        var ecParams = ecdsa.ExportParameters(includePrivateParameters: true);

        // 2) Import into ECDiffieHellman
        using var ecdh = ECDiffieHellman.Create();
        ecdh.ImportParameters(ecParams);

        // 3) Self-agreement: derive a key by agreeing with our own public key
        //    (ECDiffieHellmanPublicKey exposes the public half of 'ecdh')
        byte[] derived = ecdh.DeriveKeyFromHash(
            ecdh.PublicKey,
            HashAlgorithmName.SHA256   // or SHA512 if you prefer
        );

        // zero-out private EC params if you want (optional)
        // CryptographicOperations.ZeroMemory(ecParams.D);
            return derived;
}




    private static async Task AtomicWriteAsync (string path, byte[] data, CancellationToken ct) {
            string dir = Path.GetDirectoryName(path)!;
            string tempFile = Path.Combine(dir, Path.GetRandomFileName());

            Directory.CreateDirectory(dir);
            await File.WriteAllBytesAsync(tempFile, data, ct)
                      .ConfigureAwait(false);

            // Replace or move
            File.Move(tempFile, path, overwrite: true);
        }
    }

    // === Custom exceptions ===

    public class ProfileAuthenticationException : Exception {
        public ProfileAuthenticationException (string msg, Exception? inner = null)
            : base(msg, inner) { }
    }

    public class ProfileDecryptionException : Exception {
        public ProfileDecryptionException (string msg, Exception? inner = null)
            : base(msg, inner) { }
    }
