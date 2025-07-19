/////
/// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
/// h t t p s : / / i n c o r r i g o . i o /
////
/// Sessions At the Core of What We Do

using BlackBook.Security;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

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
        catch (FileNotFoundException ex) when (ex.FileName.EndsWith("file.file")) {
            Data = null;
            CurrentUserName = string.Empty;
            CurrentPassword = string.Empty;

            // Clearly indicate the absence of the key file
            MessageBox.Show("Can't unlock this profile without the key! " +
                            "Replace file.file in the profile folder",
                            "Black Book | Key Missing",
                            MessageBoxButton.OK, MessageBoxImage.Warning);

            return false;
        }
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
