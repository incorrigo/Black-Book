using System.IO;
using System.Threading.Tasks;
using BlackBook.Security;

namespace BlackBook;

public static class SessionManager {
    public static string CurrentUserName { get; private set; } = string.Empty;
    public static string CurrentPassword { get; set; } = string.Empty;
    public static BlackBook.Storage.BlackBookContainer? Data { get; private set; }

    /// <summary>True = password OK and data loaded.<br/>
    /// False = wrong password.<br/>
    /// Throws = tampering.</summary>
    public static async Task<bool> LoadSessionAsync (string user, string pw) {
        try {
            Data = await SecureProfileManager.LoadAsync(
                       user, pw,
                       Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"));

            CurrentUserName = user;
            CurrentPassword = pw;
            return true;
        }
        catch (ProfileAuthenticationException) { return false; }
    }

    public static async Task SaveAndClearAsync () {
        if (Data is null) return;           // nothing to do

        await SecureProfileManager.SaveProfileAsync(
            CurrentUserName, CurrentPassword, Data,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"));

        // ---- purge managed memory ----
        Data.Clear();
        Data = null;
        CurrentUserName = string.Empty;
        CurrentPassword = string.Empty;
    }
}
