/////
/// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
/// h t t p s : / / i n c o r r i g o . i o /
////
/// Password Management Implementation


using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using BlackBook.Security;

namespace BlackBook.Views;

public partial class ChangePasswordWindow : Window {
    private bool passwordsMatch = false;
    public ChangePasswordWindow () {
        InitializeComponent();
        ChangeButton.IsEnabled = false;
    }

    private void NewPasswordBox_PasswordChanged (object sender, RoutedEventArgs e) {
        string pw = NewPasswordBox.Password;
        var strength = EvaluatePasswordStrength(pw);
        PasswordStrengthIndicator.Fill = strength switch {
            PasswordStrength.Weak => new SolidColorBrush(Color.FromRgb(120, 40, 40)),   // dark red
            PasswordStrength.Medium => new SolidColorBrush(Color.FromRgb(128, 0, 128)),   // purple
            PasswordStrength.Strong => new SolidColorBrush(Color.FromRgb(0, 128, 0)),     // green
            _ => new SolidColorBrush(Color.FromRgb(128, 128, 128))  // gray
        };
        ValidateForm();
    }

    private void ConfirmPasswordBox_PasswordChanged (object sender, RoutedEventArgs e) {
        passwordsMatch = ConfirmPasswordBox.Password == NewPasswordBox.Password && NewPasswordBox.Password.Length > 0;
        ConfirmPasswordBox.BorderBrush = passwordsMatch
            ? new SolidColorBrush(Color.FromRgb(0, 200, 0))       // green border if match
            : new SolidColorBrush(Color.FromRgb(171, 173, 179));  // default light gray border
        ValidateForm();
    }

    private void ValidateForm () {
        bool valid = !string.IsNullOrWhiteSpace(CurrentPasswordBox.Password)
                     && !string.IsNullOrWhiteSpace(NewPasswordBox.Password)
                     && passwordsMatch;
        ChangeButton.IsEnabled = valid;
    }

    private async void Change_Click (object sender, RoutedEventArgs e) {
        string currentPw = CurrentPasswordBox.Password;
        string newPw = NewPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(currentPw) || string.IsNullOrWhiteSpace(newPw)) {
            MessageBox.Show("You need to enter all the details", "Change Password",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (currentPw != SessionManager.CurrentPassword) {
            MessageBox.Show("The current password you entered is incorrect", "Change Password",
                            MessageBoxButton.OK, MessageBoxImage.None);
            return;
        }
        if (!passwordsMatch) {
            MessageBox.Show("Both of the new passwords must match", "Confirm Password",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try {
            string user = SessionManager.CurrentUserName;
            string rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users");
            string bundlePath = Path.Combine(rootPath, user, "file.file");

            // Read existing salt (64 bytes explicitly)
            byte[] bundleBlob = await File.ReadAllBytesAsync(bundlePath);
            byte[] salt = bundleBlob[..64];
            byte[] encryptedBundle = bundleBlob[64..];

            // Explicitly extract 32-byte keys for ChaCha20-Poly1305
            byte[] oldFullKey = Crypto.DeriveAeadKey(user, currentPw, salt);
            byte[] oldKey = oldFullKey.Take(32).ToArray();
            byte[] bundlePlain = Crypto.AeadDecrypt(encryptedBundle, oldKey);

            // Generate new salt explicitly for the new password
            byte[] newSalt = Crypto.GenerateSalt();
            byte[] newFullKey = Crypto.DeriveAeadKey(user, newPw, newSalt);
            byte[] newKey = newFullKey.Take(32).ToArray();
            byte[] newWrapped = Crypto.AeadEncrypt(bundlePlain, newKey);

            // Store new salt explicitly alongside new encrypted bundle
            byte[] finalBlob = newSalt.Concat(newWrapped).ToArray();
            await File.WriteAllBytesAsync(bundlePath, finalBlob);

            SessionManager.CurrentPassword = newPw;
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to change password:\n{ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        MessageBox.Show("Your password has now been changed", "Black Book",
                        MessageBoxButton.OK, MessageBoxImage.Information);
        DialogResult = true;
        Close();
    }


    private void Cancel_Click (object sender, RoutedEventArgs e) {
        this.Close();
    }

    private enum PasswordStrength { None, Weak, Medium, Strong }

    private PasswordStrength EvaluatePasswordStrength (string password) {
        if (password.Length >= 12
            && password.Any(char.IsUpper)
            && password.Any(char.IsDigit)
            && password.Any(ch => !char.IsLetterOrDigit(ch))) {
            return PasswordStrength.Strong;
        }
        if (password.Length >= 8
            && (password.Any(char.IsUpper) || password.Any(char.IsDigit))) {
            return PasswordStrength.Medium;
        }
        if (password.Length >= 4) {
            return PasswordStrength.Weak;
        }
        return PasswordStrength.None;
    }
}
