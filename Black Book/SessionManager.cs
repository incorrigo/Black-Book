using System.Security.Cryptography.X509Certificates;
using BlackBook.Storage;

namespace BlackBook;
    public static class SessionManager {
    public static string CurrentUserName { get; set; } = "";
    public static string CurrentPassword { get; set; } = "";
    public static X509Certificate2? Certificate { get; set; }
    public static BlackBookContainer? Data { get; set; }

    public static bool LoadSession (string name, string password) {
            var cert = Security.ProfileUnlocker.TryUnlockCertificate(name, password);
            if (cert == null) return false;

            CurrentUserName = name;
            CurrentPassword = password;
            Certificate = cert;

            Data = EncryptedContainerManager.LoadEncrypted(name, password);
            Data.LastOpened = DateTime.UtcNow;
            Data.AccessCount++;
            EncryptedContainerManager.SaveEncrypted(Data, name, password);

            return true;
        }
    }
