using System;
using System.Collections.Generic;
using System.IO;

namespace BlackBook.Storage;

public static class ProfileSelector {
    public static List<string> GetAvailableProfiles () {
        var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users");
        var profiles = new List<string>();

        if (!Directory.Exists(root)) return profiles;

        foreach (var dir in Directory.GetDirectories(root)) {
            var name = Path.GetFileName(dir);
            var certPath = Path.Combine(dir, "file.file");
            var dataPath = Path.Combine(dir, "file");

            if (File.Exists(certPath) && File.Exists(dataPath)) {
                profiles.Add(name);
            }
        }

        return profiles;
    }
}
