using BlackBook.Models;
using BlackBook.Security;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace BlackBook.Views;

public partial class CertificateSetupWindow : Window {
    public CertificateSetupWindow () {
        InitializeComponent();
    }

    private void CreateKey_Click (object sender, RoutedEventArgs e) {
        if (string.IsNullOrWhiteSpace(UserNameBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password)) {
            MessageBox.Show("Name and password can't be empty.");
            return;
        }

        var certPassword = SecurityManager.CreateCertPassword(UserNameBox.Text, PasswordBox.Password);

        var cert = SecurityManager.GenerateCertificate(
            commonName: UserNameBox.Text,
            organization: "",
            organizationalUnit: "",
            country: "",
            state: "",
            locality: "",
            password: certPassword
        );

        SecurityManager.ExportCertificate(cert, "usercert.pfx", certPassword);

        MessageBox.Show("Certificate created successfully.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
        Close();
    }

    private void Cancel_Click (object sender, RoutedEventArgs e) {
        Close();
    }

    private void UserNameBox_TextChanged (object sender, System.Windows.Controls.TextChangedEventArgs e) {
        var initials = new string(UserNameBox.Text
            .Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word[0])
            .ToArray())
            .ToUpper();

        InitialsMarker.Text = initials;
    }

    private void PasswordBox_PasswordChanged (object sender, RoutedEventArgs e) {
        var password = PasswordBox.Password;
        var strength = EvaluatePasswordStrength(password);

        PasswordStrengthIndicator.Fill = strength switch {
            PasswordStrength.Weak => new SolidColorBrush(Color.FromRgb(120, 40, 40)),    // mild red
            PasswordStrength.Medium => new SolidColorBrush(Color.FromRgb(128, 0, 128)),  // purple
            PasswordStrength.Strong => new SolidColorBrush(Color.FromRgb(255, 20, 147)), // electric pink
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
