// InitialLoginWindow.xaml.cs
using BlackBook.Security;
using BlackBook.Storage;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;

namespace BlackBook.Views {
    public partial class InitialLoginWindow : Window {
        public InitialLoginWindow () {
            InitializeComponent();
            LoadProfiles();
        }

        // Load available profiles into the ComboBox:
        private void LoadProfiles () {
            ProfileList.ItemsSource = Storage.ProfileSelector.GetAvailableProfiles();
        }

        private void Cancel_Click (object sender, RoutedEventArgs e) {
            this.Close();
        }

        private async void Unlock_Click (object sender, RoutedEventArgs e) {
            var name = ProfileList.SelectedItem as string;
            var password = PasswordInput.Password;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password)) {
                MessageBox.Show("Select a profile and enter the password.", "Incomplete",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool ok;
            try {
                ok = await SessionManager.LoadSessionAsync(name, password);
            }
            catch (ProfileDecryptionException) {
                MessageBox.Show("Data appears tampered or corrupted.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!ok) {
                MessageBox.Show("Incorrect password. Please try again.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Open the main application window upon successful login
            new MainWindow().Show();
            this.Close();
        }

        private void CreateProfile_Click (object sender, RoutedEventArgs e) {
            // Open the profile creation dialog modally
            var setup = new CertificateSetupWindow();
            setup.Owner = this;
            setup.ShowDialog();
            // After creating a new profile, refresh the profile list in case a new one was added
            LoadProfiles();
        }
    }
}
