using System.IO;
using System.Threading.Tasks;
using BlackBook.Security;
using BlackBook.Storage;

namespace BlackBook;
public static class SessionManager {
    public static string CurrentUserName { get; set; } = "";
    public static string CurrentPassword { get; set; } = "";
    public static BlackBookContainer? Data { get; set; }

    /// <summary>
    /// Returns true if password was correct and data loaded.
    /// Throws ProfileDecryptionException if the AEAD unwrap fails.
    /// </summary>
    public static async Task<bool> LoadSessionAsync (string user, string pw) {
        try {
            Data = await SecureProfileManager.LoadAsync(
                       user, pw, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"));
            CurrentUserName = user;
            CurrentPassword = pw;
            return true;
        }
        catch (ProfileAuthenticationException) { return false; }
    }

}
