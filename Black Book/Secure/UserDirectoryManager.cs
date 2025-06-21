using System.IO;

namespace BlackBook.Storage;

public static class UserDirectoryManager {
    public static string GetUserDirectory (string userName) {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users", userName);
        Directory.CreateDirectory(path);
        return path;
    }

    public static string GetEncryptedDataPath (string userName) =>
        Path.Combine(GetUserDirectory(userName), "file");

    public static string GetUserCertPath (string userName) =>
        Path.Combine(GetUserDirectory(userName), "file.file");

    public static string GetPublicKeyPath (string userName) =>
        Path.Combine(GetUserDirectory(userName), "ecdh.pub");

    public static string GetPrivateKeyPath (string userName) =>
        Path.Combine(GetUserDirectory(userName), "trojan.exe");
}
