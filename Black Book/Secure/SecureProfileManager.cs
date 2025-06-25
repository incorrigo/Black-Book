using BlackBook.Storage;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using System.Text.Json;

namespace BlackBook.Security;

public static class SecureProfileManager {
    private static string Dir (string root, string user) => Path.Combine(root, user);
    private static string DataFile (string root, string u) => Path.Combine(Dir(root, u), "file");
    private static string BundleFile (string root, string u) => Path.Combine(Dir(root, u), "file.file");

    /* ----------  Create  ---------- */

    /// <summary>Create a new profile with the given name/password and initial data.</summary>
    public static async Task CreateProfileAsync (
        string userName,
        string password,
        BlackBookContainer initialData,
        string usersRootDirectory,
        CancellationToken ct = default
    ) {
        // just rename the parameters––body stays exactly the same,
        // substituting `userName` for `user` and `password` for `pw`, etc.
        Directory.CreateDirectory(Path.Combine(usersRootDirectory, userName));

        using var ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
        var bundle = KeyBundle.FromEcdh(ecdh);
        var bundleJson = JsonSerializer.SerializeToUtf8Bytes(bundle);

        var pwKey = Crypto.DeriveAeadKey(userName, password);
        var wrappedBundle = Crypto.AeadEncrypt(bundleJson, pwKey);
        await File.WriteAllBytesAsync(
            Path.Combine(usersRootDirectory, userName, "file.file"),
            wrappedBundle,
            ct
        );

        var dataKey = DeriveDataKey(ecdh);
        var initJson = JsonSerializer.SerializeToUtf8Bytes(initialData);
        var wrapped = Crypto.AeadEncrypt(initJson, dataKey);
        await File.WriteAllBytesAsync(
            Path.Combine(usersRootDirectory, userName, "file"),
            wrapped,
            ct
        );
    }


    /* ----------  Load  ---------- */

    public static async Task<BlackBookContainer> LoadAsync (
            string user, string pw, string root, CancellationToken ct = default) {
        var pwKey = Crypto.DeriveAeadKey(user, pw);
        var blob = await File.ReadAllBytesAsync(BundleFile(root, user), ct);

        KeyBundle bundle;
        try {
            var plain = Crypto.AeadDecrypt(blob, pwKey);
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

    /* ----------  Save  ---------- */

    public static async Task SaveProfileAsync (string user, string pw, BlackBookContainer data,
                                       string root, CancellationToken ct = default) {
        var pwKey = Crypto.DeriveAeadKey(user, pw);
        var bundle = JsonSerializer.Deserialize<KeyBundle>(
                         Crypto.AeadDecrypt(
                             await File.ReadAllBytesAsync(BundleFile(root, user), ct),
                             pwKey));

        using var ecdh = bundle!.ToEcdh();
        var dataKey = DeriveDataKey(ecdh);

        var json = JsonSerializer.SerializeToUtf8Bytes(data);
        var wrapped = Crypto.AeadEncrypt(json, dataKey);
        await File.WriteAllBytesAsync(DataFile(root, user), wrapped, ct);
    }

    /* ----------  helper  ---------- */

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
                Convert.ToBase64String(p.D)
            );
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
