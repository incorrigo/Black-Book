// CertificateSetupWindow.xaml.cs
using BlackBook.Security;
using BlackBook.Storage;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace BlackBook.Views {
    public partial class CertificateSetupWindow : Window {
        private bool passwordsMatch = false;
        private readonly Action? _onProfileCreated;

        public CertificateSetupWindow (Action? onProfileCreated = null) {
            InitializeComponent();
            CreateButton.IsEnabled = false;
            _onProfileCreated = onProfileCreated;
        }

        private async void CreateKey_Click (object sender, RoutedEventArgs e) {
            var name = UserNameBox.Text.Trim();
            var password = PasswordBox.Password;
            var confirm = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password)) {
                MessageBox.Show("Name and password cannot be empty.", "Incomplete",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!passwordsMatch || password != confirm) {
                MessageBox.Show("Passwords do not match. Please confirm the password.", "Incomplete",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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

            MessageBox.Show("Profile created successfully.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);

            // Explicitly invoke callback to reload profiles after creation
            _onProfileCreated?.Invoke();

            DialogResult = true;
            Close();
        }


        private void Cancel_Click (object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void UserNameBox_TextChanged (object sender, System.Windows.Controls.TextChangedEventArgs e) {
            // Update initials marker (first letters of each word in name, uppercase)
            var initials = new string(UserNameBox.Text
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => word[0])
                .ToArray()).ToUpper();
            InitialsMarker.Text = initials;
            ValidateForm();
        }

        private void PasswordBox_PasswordChanged (object sender, RoutedEventArgs e) {
            // Evaluate strength of the new password
            var password = PasswordBox.Password;
            var strength = EvaluatePasswordStrength(password);
            PasswordStrengthIndicator.Fill = strength switch {
                PasswordStrength.Weak => new SolidColorBrush(Color.FromRgb(120, 40, 40)),   // dark red
                PasswordStrength.Medium => new SolidColorBrush(Color.FromRgb(128, 0, 128)),   // purple
                PasswordStrength.Strong => new SolidColorBrush(Color.FromRgb(255, 20, 147)), // deep pink
                _ => new SolidColorBrush(Color.FromRgb(128, 128, 128)) // gray for none
            };
            ValidateForm();
        }

        private void ConfirmPasswordBox_PasswordChanged (object sender, RoutedEventArgs e) {
            // Check if confirmation matches the original password
            passwordsMatch = ConfirmPasswordBox.Password == PasswordBox.Password && PasswordBox.Password.Length > 0;
            // Visually indicate match (green border if match, default gray if not)
            ConfirmPasswordBox.BorderBrush = passwordsMatch
                ? new SolidColorBrush(Color.FromRgb(0, 200, 0))       // green when matching
                : new SolidColorBrush(Color.FromRgb(171, 173, 179));  // light gray default
            ValidateForm();
        }

        // Enable Create button only if all inputs are valid
        private void ValidateForm () {
            bool valid = !string.IsNullOrWhiteSpace(UserNameBox.Text)
                         && !string.IsNullOrWhiteSpace(PasswordBox.Password)
                         && passwordsMatch;
            CreateButton.IsEnabled = valid;
        }

        // Simple password strength checker (same logic as before)
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
}
