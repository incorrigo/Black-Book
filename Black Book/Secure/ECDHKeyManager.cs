using System;
using System.IO;
using System.Security.Cryptography;

namespace BlackBook.Security;

public static class ECDHKeyManager {
    public static void GenerateKeyPair (string userName) {
        using var ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);

        byte[] publicKey = ecdh.PublicKey.ToByteArray();
        byte[] privateKey = ecdh.ExportPkcs8PrivateKey();

        File.WriteAllBytes(Storage.UserDirectoryManager.GetPublicKeyPath(userName), publicKey);
        File.WriteAllBytes(Storage.UserDirectoryManager.GetPrivateKeyPath(userName), privateKey);
    }

    public static ECDiffieHellman LoadPrivateKey (string userName) {
        var privateKeyPath = Storage.UserDirectoryManager.GetPrivateKeyPath(userName);
        byte[] keyBytes = File.ReadAllBytes(privateKeyPath);
        var ecdh = ECDiffieHellman.Create();
        ecdh.ImportPkcs8PrivateKey(keyBytes, out _);
        return ecdh;
    }

    public static ECDiffieHellmanPublicKey LoadPublicKey (string userName) {
        var publicKeyPath = Storage.UserDirectoryManager.GetPublicKeyPath(userName);
        byte[] keyBytes = File.ReadAllBytes(publicKeyPath);
        return ECDiffieHellmanCngPublicKey.FromByteArray(keyBytes, CngKeyBlobFormat.EccPublicBlob);
    }
}
