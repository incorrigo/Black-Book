// Filename: EncryptedContainerManager.cs
using System.Text;
using System.Text.Json;
using BlackBook.Models;
using BlackBook.Security;

namespace BlackBook.Storage {
    public static class EncryptedContainerManager {

        public static void SaveEncrypted (BlackBookContainer container, string userName, string password) {
            // 1) JSON-serialize
            var json = JsonSerializer.Serialize(container);
            var plain = Encoding.UTF8.GetBytes(json);

            // 2) Symmetric key from name+pw
            var key = FileEncryptor.DeriveKey(userName, password);

            // 3) Write to disk
            var path = UserDirectoryManager.GetEncryptedDataPath(userName);
            FileEncryptor.EncryptToFile(plain, path, key);
        }

        public static BlackBookContainer LoadEncrypted (string userName, string password) {
            // 1) Derive same key
            var key = FileEncryptor.DeriveKey(userName, password);

            // 2) Read & decrypt
            var plain = FileEncryptor.DecryptFromFile(
                UserDirectoryManager.GetEncryptedDataPath(userName), key);

            // 3) Deserialize JSON
            var json = Encoding.UTF8.GetString(plain);
            return JsonSerializer.Deserialize<BlackBookContainer>(json)
                   ?? new BlackBookContainer();
        }
    }
}
