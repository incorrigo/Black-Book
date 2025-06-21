using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;

namespace BlackBook.Storage;

public static class EncryptedContainerManager {

    public static void SaveEncrypted (BlackBookContainer container, X509Certificate2 cert, string path) {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        aes.GenerateIV();

        var json = JsonSerializer.Serialize(container);
        byte[] encryptedData;
        using (var encryptor = aes.CreateEncryptor())
        using (var ms = new MemoryStream())
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        using (var sw = new StreamWriter(cs)) {
            sw.Write(json);
            sw.Close();
            encryptedData = ms.ToArray();
        }

        byte[] encryptedKey = cert.GetRSAPublicKey()?.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1)
            ?? throw new Exception("Cert does not support RSA encryption.");

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);
        bw.Write(encryptedKey.Length);
        bw.Write(encryptedKey);
        bw.Write(aes.IV.Length);
        bw.Write(aes.IV);
        bw.Write(encryptedData);
    }

    public static BlackBookContainer LoadEncrypted (string path, X509Certificate2 cert) {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);

        int keyLen = br.ReadInt32();
        byte[] encryptedKey = br.ReadBytes(keyLen);

        int ivLen = br.ReadInt32();
        byte[] iv = br.ReadBytes(ivLen);

        byte[] encryptedData = br.ReadBytes((int)(fs.Length - fs.Position));

        var aesKey = cert.GetRSAPrivateKey()?.Decrypt(encryptedKey, RSAEncryptionPadding.Pkcs1)
            ?? throw new Exception("Cert does not support RSA decryption.");

        using var aes = Aes.Create();
        aes.Key = aesKey;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(encryptedData);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        var json = sr.ReadToEnd();

        return JsonSerializer.Deserialize<BlackBookContainer>(json);
    }
}