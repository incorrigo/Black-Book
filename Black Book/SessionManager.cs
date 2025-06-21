using System.Security.Cryptography.X509Certificates;
using BlackBook.Storage;

namespace BlackBook;

public static class SessionManager {
    public static string CurrentUserName { get; private set; } = "";
    public static X509Certificate2? Certificate { get; private set; }
    public static BlackBookContainer? Data { get; private set; }

    public static bool LoadSession (string name, string password) {
        var cert = Security.ProfileUnlocker.TryUnlockCertificate(name, password);
        if (cert == null) return false;

        var filePath = Storage.UserDirectoryManager.GetEncryptedDataPath(name);
        var container = EncryptedContainerManager.LoadEncrypted(filePath, cert);
        CurrentUserName = name;
        Certificate = cert;
        Data = container;
        return true;
    }
}