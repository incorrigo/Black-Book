using System;
using System.Collections.Generic;
using System.IO;

namespace BlackBook.Storage;

/// <summary>Returns a simple list of profile names (folder names below “Users\”).</summary>
public static class ProfileSelector {
    public static IReadOnlyList<string> GetAvailableProfiles () {
        var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users");
        if (!Directory.Exists(root)) return Array.Empty<string>();

        var list = new List<string>();
        foreach (var dir in Directory.GetDirectories(root)) {
            var n = Path.GetFileName(dir);
            if (File.Exists(Path.Combine(dir, "file")) &&
                File.Exists(Path.Combine(dir, "file.file")))
                list.Add(n);
        }
        return list;
    }
}
