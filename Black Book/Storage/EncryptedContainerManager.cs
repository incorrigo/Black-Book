using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using BlackBook.Security;

namespace BlackBook.Storage;

public static class EncryptedContainerManager {

    public static void SaveEncrypted (BlackBookContainer container, string userName) {
        using var ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
        var otherPublic = ECDHKeyManager.LoadPublicKey(userName);
        byte[] sharedSecret = ecdh.DeriveKeyMaterial(otherPublic);

        using var aes = Aes.Create();
        aes.Key = sharedSecret;
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

        var filePath = UserDirectoryManager.GetEncryptedDataPath(userName);
        using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);

        byte[] ephemeralPubKey = ecdh.PublicKey.ToByteArray();
        bw.Write(ephemeralPubKey.Length);
        bw.Write(ephemeralPubKey);
        bw.Write(aes.IV.Length);
        bw.Write(aes.IV);
        bw.Write(encryptedData);
    }

    public static BlackBookContainer LoadEncrypted (string userName) {
        var filePath = UserDirectoryManager.GetEncryptedDataPath(userName);
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);

        int keyLen = br.ReadInt32();
        byte[] pubKeyBytes = br.ReadBytes(keyLen);

        int ivLen = br.ReadInt32();
        byte[] iv = br.ReadBytes(ivLen);

        byte[] encrypted = br.ReadBytes((int)(fs.Length - fs.Position));

        using var myECDH = ECDHKeyManager.LoadPrivateKey(userName);
        var otherKey = ECDiffieHellmanCngPublicKey.FromByteArray(pubKeyBytes, CngKeyBlobFormat.EccPublicBlob);
        byte[] sharedSecret = myECDH.DeriveKeyMaterial(otherKey);

        using var aes = Aes.Create();
        aes.Key = sharedSecret;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(encrypted);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        var json = sr.ReadToEnd();

        return JsonSerializer.Deserialize<BlackBookContainer>(json);
    }
}
