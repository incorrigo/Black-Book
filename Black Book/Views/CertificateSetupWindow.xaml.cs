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

    private void CreateKey_Click (object sender, RoutedEventArgs e) {
        var name = UserNameBox.Text.Trim();
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password)) {
            MessageBox.Show("Name and password can't be empty.");
            return;
        }

        var certPassword = SecurityManager.CreatePfxPassword(name, password);
        var cert = SecurityManager.GenerateCertificate(
            commonName: "Black Book: " + name,
            organization: "Incorrigo Syx", organizationalUnit: "Digital Cryptographic Systems",
            country: "GB", state: "ENGLAND", locality: "Lancashire",
            password: certPassword
        );

        var certPath = UserDirectoryManager.GetUserCertPath(name);
        SecurityManager.ExportCertificate(cert, certPath, certPassword);

        MessageBox.Show("Certificate created successfully.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);

        // Now Initialize the profile by creating a file
        InitializeUserProfile(name, password);

        Close();
    }


    public static void InitializeUserProfile (string userName, string password) {
        // Determine the file path
        string filePath = UserDirectoryManager.GetEncryptedDataPath(userName);

        // Check if the file already exists
        if (!File.Exists(filePath)) {
            // If the file does not exist, create it
            using (FileStream fs = new FileStream(filePath, FileMode.Create)) {
                // Optionally, write some initial data to the file, such as an empty container
                var emptyData = new byte[] { }; // Placeholder for data or an empty container
                fs.Write(emptyData, 0, emptyData.Length);
            }

            // Inform the user that the profile has been created
            MessageBox.Show("Profile created successfully. You can now add data to your profile.", "Profile Created", MessageBoxButton.OK, MessageBoxImage.Information);
        }
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
