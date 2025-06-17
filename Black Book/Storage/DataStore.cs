using System.Text.Json;
using BlackBook.Models;
using System.Collections.Generic;
using System.IO;

namespace BlackBook.Storage;

public static class DataStore {
    public static void Save<T> (string fileName, List<T> data) {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(fileName, json);
    }

    public static List<T> Load<T> (string fileName) {
        if (!File.Exists(fileName))
            return new List<T>();

        var json = File.ReadAllText(fileName);
        return JsonSerializer.Deserialize<List<T>>(json);
    }
}
