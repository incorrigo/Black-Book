using BlackBook.Security;
using BlackBook.Storage;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace BlackBook.Views;

public partial class CertificateSetupWindow : Window {
    public CertificateSetupWindow () {
        InitializeComponent();
    }

    private async void CreateKey_Click (object sender, RoutedEventArgs e) {
        var name = UserNameBox.Text.Trim();
        var password = PasswordBox.Password;
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password)) {
            MessageBox.Show("Name and password can't be empty.");
            return;
        }

        // Create *everything* properly wrapped:
        try {
            await SecureProfileManager.CreateProfileAsync(
                userName: name,
                password: password,
                initialData: new BlackBookContainer(),
                usersRootDirectory: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
                ct: CancellationToken.None
            );
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to initialize profile data:\n{ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        MessageBox.Show("Profile created successfully.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
        Close();
    }

    private void Cancel_Click (object sender, RoutedEventArgs e) {
        Close();
    }

    private void UserNameBox_TextChanged (object sender, System.Windows.Controls.TextChangedEventArgs e) {
        var initials = new string(UserNameBox.Text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word[0])
            .ToArray())
            .ToUpper();

        InitialsMarker.Text = initials;
    }

    private void PasswordBox_PasswordChanged (object sender, RoutedEventArgs e) {
        var password = PasswordBox.Password;
        var strength = EvaluatePasswordStrength(password);

        PasswordStrengthIndicator.Fill = strength switch {
            PasswordStrength.Weak => new SolidColorBrush(Color.FromRgb(120, 40, 40)),
            PasswordStrength.Medium => new SolidColorBrush(Color.FromRgb(128, 0, 128)),
            PasswordStrength.Strong => new SolidColorBrush(Color.FromRgb(255, 20, 147)),
            _ => new SolidColorBrush(Color.FromRgb(51, 51, 51))
        };
    }

    private enum PasswordStrength { None, Weak, Medium, Strong }

    private PasswordStrength EvaluatePasswordStrength (string password) {
        if (password.Length >= 12 &&
            password.Any(char.IsUpper) &&
            password.Any(char.IsDigit) &&
            password.Any(ch => !char.IsLetterOrDigit(ch))) {
            return PasswordStrength.Strong;
        }

        if (password.Length >= 8 &&
            (password.Any(char.IsUpper) || password.Any(char.IsDigit))) {
            return PasswordStrength.Medium;
        }

        if (password.Length >= 4) {
            return PasswordStrength.Weak;
        }

        return PasswordStrength.None;
    }
}
