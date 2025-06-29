// ChangePasswordWindow.xaml.cs (new)
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
            MessageBox.Show("All fields are required.", "Incomplete",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (currentPw != SessionManager.CurrentPassword) {
            MessageBox.Show("Current password is incorrect.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        if (!passwordsMatch) {
            MessageBox.Show("New passwords do not match.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        try {
            string user = SessionManager.CurrentUserName;
            string rootPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users");
            // Re-encrypt key bundle with new password
            byte[] oldKey = Crypto.DeriveAeadKey(user, currentPw);
            string bundlePath = Path.Combine(rootPath, user, "file.file");
            byte[] bundleBlob = await File.ReadAllBytesAsync(bundlePath);
            byte[] bundlePlain = Crypto.AeadDecrypt(bundleBlob, oldKey);
            byte[] newKey = Crypto.DeriveAeadKey(user, newPw);
            byte[] newWrapped = Crypto.AeadEncrypt(bundlePlain, newKey);
            await File.WriteAllBytesAsync(bundlePath, newWrapped);
            SessionManager.CurrentPassword = newPw;
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to change password:\n{ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        MessageBox.Show("Password changed successfully.", "Success",
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
