using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace BlackBook.Storage;

public static class EncryptedContainerManager {
    public static void SaveEncrypted (BlackBookContainer container, string userName, X509Certificate2 cert) {
        using var remotePublic = cert.GetECDiffieHellmanPublicKey()!;
        using var ephemeral = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
        byte[] sharedSecret = new byte[32]; // AES-256 key size (32 bytes)
        using (var rng = new RNGCryptoServiceProvider()) {
            rng.GetBytes(sharedSecret); // Generate random bytes for the secret key
        }

        using var aes = Aes.Create();
        aes.Key = sharedSecret; // Assign the generated secret key to AES
        aes.GenerateIV(); // Generate the initialization vector (IV) as usual


        string json = JsonSerializer.Serialize(container);
        byte[] encrypted;
        using (var encryptor = aes.CreateEncryptor())
        using (var ms = new MemoryStream())
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs)) {
            sw.Write(json);
            sw.Flush();
            cs.FlushFinalBlock();
            encrypted = ms.ToArray();
        }

        string path = UserDirectoryManager.GetEncryptedDataPath(userName);
        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);

        byte[] ephPublic = ephemeral.PublicKey.ToByteArray();
        bw.Write(ephPublic.Length);
        bw.Write(ephPublic);
        bw.Write(aes.IV.Length);
        bw.Write(aes.IV);
        bw.Write(encrypted);
    }

    public static BlackBookContainer LoadEncrypted (string userName, X509Certificate2 cert) {
        string path = UserDirectoryManager.GetEncryptedDataPath(userName);
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);

        int keyLen = br.ReadInt32();
        byte[] ephPub = br.ReadBytes(keyLen);
        int ivLen = br.ReadInt32();
        byte[] iv = br.ReadBytes(ivLen);
        byte[] cipher = br.ReadBytes((int)(fs.Length - fs.Position));

        using var priv = cert.GetECDiffieHellmanPrivateKey()!;
        var other = ECDiffieHellmanCngPublicKey.FromByteArray(ephPub, CngKeyBlobFormat.EccPublicBlob);
        byte[] sharedSecret = priv.DeriveKeyMaterial(other);

        using var aes = Aes.Create();
        aes.Key = sharedSecret;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(cipher);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        string json = sr.ReadToEnd();

        return JsonSerializer.Deserialize<BlackBookContainer>(json)!;
    }
}
