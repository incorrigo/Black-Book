using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BlackBook.Security;

public static class ProfileEraser {
    public static void NukeProfile(string userName) {
        var dir = Storage.UserDirectoryManager.GetUserDirectory(userName);
        var filePath = Path.Combine(dir, "file");
        var certPath = Path.Combine(dir, "cert.pfx");

        if (!File.Exists(filePath)) return;

        var rng = RandomNumberGenerator.Create();
        long length = new FileInfo(filePath).Length;
        int tail = 500 + RandomInt(0, 1548);
        int passes = RandomInt(3, 7);
        int longestTrail = 0;

        for (int i = 0; i < passes; i++) {
            int totalLength = (int)(length + 500 + RandomInt(0, 1548));
            if (totalLength > longestTrail) longestTrail = totalLength;

            byte[] buffer = new byte[totalLength];
            rng.GetBytes(buffer);
            File.WriteAllBytes(filePath, buffer);
        }

        // Simulate AES encryption with ephemeral key (but discard result)
        using var ecdh = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP521);
        byte[] dummyData = new byte[longestTrail];
        rng.GetBytes(dummyData);

        using var aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();
        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        cs.Write(dummyData, 0, dummyData.Length);
        cs.FlushFinalBlock();
        // Discard: we never save the result

        // Corrupt cert
        if (File.Exists(certPath)) {
            byte[] noise = new byte[1024];
            rng.GetBytes(noise);
            File.WriteAllBytes(certPath, noise);
            File.Delete(certPath);
        }
    }

    private static int RandomInt(int min, int max) {
        var buffer = new byte[4];
        RandomNumberGenerator.Fill(buffer);
        return new Random(BitConverter.ToInt32(buffer, 0)).Next(min, max);
    }
}