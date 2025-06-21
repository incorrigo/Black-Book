using System.Collections.Generic;
using System.IO;

namespace BlackBook.Storage;

public static class ProfileSelector {
    public static List<string> GetAvailableProfiles () {
        var usersRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users");
        if (!Directory.Exists(usersRoot))
            return new List<string>();

        var profiles = new List<string>();
        foreach (var dir in Directory.GetDirectories(usersRoot)) {
            var name = Path.GetFileName(dir);
            if (File.Exists(Path.Combine(dir, "cert.pfx")) && File.Exists(Path.Combine(dir, "file")))
                profiles.Add(name);
        }
        return profiles;
    }
}