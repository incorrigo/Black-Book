using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using BlackBook.Security;
using BlackBook.Storage;

namespace BlackBook;
public static class SessionManager {
    public static string CurrentUserName { get; set; } = "";
    public static string CurrentPassword { get; set; } = "";
    public static X509Certificate2? Certificate { get; set; }
    public static BlackBookContainer? Data { get; set; }

    public static async Task<bool> LoadSessionAsync (string name, string password) {
        // ① Unlock the PFX and stash it
        var cert = ProfileUnlocker.TryUnlockCertificate(name, password);
        if (cert == null) {
            return false;
        }
        CurrentUserName = name;
        CurrentPassword = password;
        Certificate = cert;

        // ② Load your JSON container
        Data = await SecureProfileManager.LoadProfileAsync(
            userName: name,
            password: password,
            usersRootDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users")
        );

        // ③ Update metadata and re-persist
        Data.LastOpened = DateTime.UtcNow;
        Data.AccessCount++;
        await SecureProfileManager.SaveProfileAsync(
            name, password, Data, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users")
        );

        return true;
    }
}
