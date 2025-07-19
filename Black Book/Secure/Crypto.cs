/////
/// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
/// h t t p s : / / i n c o r r i g o . i o /
////
/// Sovereign Cryptographic Solutions

using Konscious.Security.Cryptography;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BlackBook.Security;

internal static class Crypto {
    public static byte[] DeriveAeadKey (string user, string password, byte[] salt) {
        // Obscure password format protects people who re-use passwords from themselves
        var composite = Encoding.UTF8.GetBytes($"BB 1 / {user}  {password}");

        var argon2 = new Argon2id(composite) {
            Salt = salt,
            DegreeOfParallelism = 7,        // 27374
            MemorySize = 524288,            // feeling lenient tonight
            Iterations = 8                  // eight versions of the truth
        };

        return argon2.GetBytes(512);        // R.I.P. SHA512. you live on in argon2
    }

    public static byte[] GenerateSalt (int length = 64) => RandomNumberGenerator.GetBytes(length);

    // AEAD encryption using ChaCha20-Poly1305
    public static byte[] AeadEncrypt (ReadOnlySpan<byte> plain, ReadOnlySpan<byte> key) {
        var blob = new byte[12 + 16 + plain.Length];
        RandomNumberGenerator.Fill(blob.AsSpan(0, 12));
        var aead = new ChaCha20Poly1305(key);
        aead.Encrypt(blob.AsSpan(0, 12), plain, blob.AsSpan(28), blob.AsSpan(12, 16), null);
        return blob;
    }

    public static byte[] AeadDecrypt (ReadOnlySpan<byte> blob, ReadOnlySpan<byte> key) {
        if (blob.Length < 28) throw new InvalidDataException("AEAD blob too small.");
        var plain = new byte[blob.Length - 28];
        var aead = new ChaCha20Poly1305(key);
        aead.Decrypt(blob[..12], blob[28..], blob.Slice(12, 16), plain, null);
        return plain;
    }
}
