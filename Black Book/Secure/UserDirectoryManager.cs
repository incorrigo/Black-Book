using System.IO;

namespace BlackBook.Storage;

public static class UserDirectoryManager {
    public static string GetUserDirectory (string userName) {
        var safeName = userName.Replace(" ", "_");
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users", safeName);
        Directory.CreateDirectory(path);
        return path;
    }

    public static string GetUserCertPath (string userName) =>
        Path.Combine(GetUserDirectory(userName), "cert.pfx");

    public static string GetEncryptedDataPath (string userName) =>
        Path.Combine(GetUserDirectory(userName), "file");
}