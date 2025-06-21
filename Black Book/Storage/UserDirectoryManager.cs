using System;
using System.IO;

namespace BlackBook.Storage;

public static class UserDirectoryManager {
    public static string GetUserDirectory (string name) {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users", name);
        Directory.CreateDirectory(path);
        return path;
    }

    public static string GetEncryptedDataPath (string name) {
        return Path.Combine(GetUserDirectory(name), "file");
    }

    public static string GetUserCertPath (string name) {
        return Path.Combine(GetUserDirectory(name), "file.file");
    }
}
