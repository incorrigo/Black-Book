/////
/// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
/// h t t p s : / / i n c o r r i g o . i o /
////
/// Secure Profile Manager


using BlackBook.Storage;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace BlackBook.Security;

public static class SecureProfileManager {
    private static string Dir (string root, string user) => Path.Combine(root, user);
    private static string DataFile (string root, string u) => Path.Combine(Dir(root, u), "file");
    private static string BundleFile (string root, string u) => Path.Combine(Dir(root, u), "file.file");

    public static async Task CreateProfileAsync (string userName, string password,
                                                BlackBookContainer initialData,
                                                string usersRootDirectory,
                                                CancellationToken ct = default) {
        Directory.CreateDirectory(Dir(usersRootDirectory, userName));

        var salt = Crypto.GenerateSalt();

        // ChaCha20-Poly1305
        var fullPwKey = Crypto.DeriveAeadKey(userName, password, salt);
        var pwKey = fullPwKey.Take(32).ToArray();


        using var ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
        var bundle = KeyBundle.FromEcdh(ecdh);
        var bundleJson = JsonSerializer.SerializeToUtf8Bytes(bundle);

        var wrappedBundle = Crypto.AeadEncrypt(bundleJson, pwKey);

        var finalBlob = salt.Concat(wrappedBundle).ToArray();
        await File.WriteAllBytesAsync(BundleFile(usersRootDirectory, userName), finalBlob, ct);

        var dataKey = DeriveDataKey(ecdh);
        var initJson = JsonSerializer.SerializeToUtf8Bytes(initialData);
        var wrapped = Crypto.AeadEncrypt(initJson, dataKey);
        await File.WriteAllBytesAsync(DataFile(usersRootDirectory, userName), wrapped, ct);
    }

    public static async Task<BlackBookContainer> LoadAsync (string user, string pw,
                                                           string root, CancellationToken ct = default) {
        var combinedBlob = await File.ReadAllBytesAsync(BundleFile(root, user), ct);
        var salt = combinedBlob[..64]; // Explicitly 64-byte salt
        var encryptedBlob = combinedBlob[64..];

        // 32-byte key for ChaCha20-Poly1305
        var fullPwKey = Crypto.DeriveAeadKey(user, pw, salt);
        var pwKey = fullPwKey.Take(32).ToArray();


        KeyBundle bundle;

        try {
            var plain = Crypto.AeadDecrypt(encryptedBlob, pwKey);
            bundle = JsonSerializer.Deserialize<KeyBundle>(plain)
                     ?? throw new InvalidDataException("Bundle JSON empty");
        }
        catch (CryptographicException ex) {
            throw new ProfileAuthenticationException("Wrong password.", ex);
        }

        using var ecdh = bundle.ToEcdh();
        var dataKey = DeriveDataKey(ecdh);
        var wrapped = await File.ReadAllBytesAsync(DataFile(root, user), ct);

        byte[] json;
        try {
            json = Crypto.AeadDecrypt(wrapped, dataKey);
        }
        catch (CryptographicException ex) {
            throw new ProfileDecryptionException("Profile data tampered.", ex);
        }

        return JsonSerializer.Deserialize<BlackBookContainer>(json)
               ?? new BlackBookContainer();
    }

    public static async Task SaveProfileAsync (string user, string pw, BlackBookContainer data,
                                          string root, CancellationToken ct = default) {
        var bundleBlob = await File.ReadAllBytesAsync(BundleFile(root, user), ct);
        var salt = bundleBlob[..64]; // Salt the fucking earth

        // ChaCha20-Poly1305
        var fullPwKey = Crypto.DeriveAeadKey(user, pw, salt);
        var pwKey = fullPwKey.Take(32).ToArray();

        var bundle = JsonSerializer.Deserialize<KeyBundle>(
                        Crypto.AeadDecrypt(bundleBlob[64..], pwKey));

        using var ecdh = bundle!.ToEcdh();
        var dataKey = DeriveDataKey(ecdh);
        var json = JsonSerializer.SerializeToUtf8Bytes(data);
        var wrapped = Crypto.AeadEncrypt(json, dataKey);

        await File.WriteAllBytesAsync(DataFile(root, user), wrapped, ct);
    }


    private static byte[] DeriveDataKey (ECDiffieHellman ecdh) {
        var secret = ecdh.DeriveKeyMaterial(ecdh.PublicKey);
        using var sha = SHA512.Create();
        var full = sha.ComputeHash(secret);
        var key = new byte[32];
        Buffer.BlockCopy(full, 0, key, 0, 32);
        CryptographicOperations.ZeroMemory(full);
        CryptographicOperations.ZeroMemory(secret);
        return key;
    }

    internal record KeyBundle (string Qx, string Qy, string D) {
        public static KeyBundle FromEcdh (ECDiffieHellman ecdh) {
            var p = ecdh.ExportParameters(true);
            return new(
                Convert.ToBase64String(p.Q.X),
                Convert.ToBase64String(p.Q.Y),
                Convert.ToBase64String(p.D));
        }

        public ECDiffieHellman ToEcdh () {
            var p = new ECParameters {
                Curve = ECCurve.NamedCurves.nistP521,
                Q = new ECPoint {
                    X = Convert.FromBase64String(Qx),
                    Y = Convert.FromBase64String(Qy)
                },
                D = Convert.FromBase64String(D)
            };
            var ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
            ecdh.ImportParameters(p);
            return ecdh;
        }
    }
}
