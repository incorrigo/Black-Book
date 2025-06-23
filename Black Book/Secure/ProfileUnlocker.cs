using System;
using System.Security.Cryptography.X509Certificates;

namespace BlackBook.Security;

public static class ProfileUnlocker {
    public static X509Certificate2? TryUnlockCertificate (string name, string password) {
        var certPath = Storage.UserDirectoryManager.GetUserCertPath(name);
        var realPassword = SecurityManager.CreatePfxPassword(name, password);
        try {
            return new X509Certificate2(certPath, realPassword,
                X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
        }
        catch {
            return null;
        }
    }
}