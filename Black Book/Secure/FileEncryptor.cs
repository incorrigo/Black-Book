// Filename: FileEncryptor.cs
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BlackBook.Security {
    public static class FileEncryptor {
        // Derive a 256-bit key from "Name␣␣Password" via SHA-512
        public static byte[] DeriveKey (string name, string password) {
            var composite = $"{name}  {password}";
            using var sha = SHA512.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(composite));
            var key = new byte[32];
            Array.Copy(hash, key, 32);
            return key;
        }

        // Encrypt plainData → outputPath using ChaCha20-Poly1305
        public static void EncryptToFile (byte[] plainData, string outputPath, byte[] key) {
            using var rng = RandomNumberGenerator.Create();
            byte[] nonce = new byte[12];
            rng.GetBytes(nonce);

            byte[] ciphertext = new byte[plainData.Length];
            byte[] tag = new byte[16];

            using var alg = new ChaCha20Poly1305(key);
            alg.Encrypt(nonce, plainData, ciphertext, tag, null);

            using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            fs.Write(nonce, 0, nonce.Length);
            fs.Write(tag, 0, tag.Length);
            fs.Write(ciphertext, 0, ciphertext.Length);
        }

        // Read outputPath, decrypt with key → returns plaintext
        public static byte[] DecryptFromFile (string inputPath, byte[] key) {
            byte[] nonce = new byte[12], tag = new byte[16];
            byte[] cipher;

            using (var fs = new FileStream(inputPath, FileMode.Open, FileAccess.Read)) {
                if (fs.Length < nonce.Length + tag.Length)
                    throw new InvalidDataException("Encrypted file is too short or corrupted.");

                fs.Read(nonce, 0, nonce.Length);
                fs.Read(tag, 0, tag.Length);
                cipher = new byte[fs.Length - nonce.Length - tag.Length];
                fs.Read(cipher, 0, cipher.Length);
            }

            var plain = new byte[cipher.Length];
            using var alg = new ChaCha20Poly1305(key);
            alg.Decrypt(nonce, cipher, tag, plain, null);
            return plain;
        }
    }
}
