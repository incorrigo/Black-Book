using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BlackBook.Security;

internal static class Crypto {
    public static byte[] DeriveAeadKey (string user, string pw) {
        var composite = Encoding.UTF8.GetBytes($"{user}  {pw}");
        using var sha = SHA512.Create();
        var full = sha.ComputeHash(composite);
        var key = new byte[32];
        Buffer.BlockCopy(full, 0, key, 0, 32);
        CryptographicOperations.ZeroMemory(full);
        return key;
    }

    public static byte[] AeadEncrypt (ReadOnlySpan<byte> plain, ReadOnlySpan<byte> key) {
        var blob = new byte[12 + 16 + plain.Length];
        RandomNumberGenerator.Fill(blob.AsSpan(0, 12));          // nonce
        var aead = new ChaCha20Poly1305(key);
        aead.Encrypt(
            nonce: blob.AsSpan(0, 12),
            plaintext: plain,
            ciphertext: blob.AsSpan(28),
            tag: blob.AsSpan(12, 16),
            associatedData: null);
        return blob;
    }

    public static byte[] AeadDecrypt (ReadOnlySpan<byte> blob, ReadOnlySpan<byte> key) {
        if (blob.Length < 28) throw new InvalidDataException("AEAD blob too small.");
        var plain = new byte[blob.Length - 28];
        var aead = new ChaCha20Poly1305(key);
        aead.Decrypt(
            nonce: blob[..12],
            ciphertext: blob[28..],
            tag: blob.Slice(12, 16),
            plaintext: plain,
            associatedData: null);
        return plain;
    }
}
