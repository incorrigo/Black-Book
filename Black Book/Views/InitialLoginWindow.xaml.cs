using BlackBook.Storage;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows;

namespace BlackBook.Views {
    public partial class InitialLoginWindow : Window {
        public InitialLoginWindow () {
            InitializeComponent();
            LoadProfiles(); // Call the method to load the profiles
        }

        // Method to load profiles into the ProfileList ComboBox
        private void LoadProfiles () {
            string usersDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users");
            if (Directory.Exists(usersDirectory)) {
                var profiles = Directory.GetDirectories(usersDirectory)
                    .Select(Path.GetFileName) // Get the name of the directory (the profile)
                    .ToList();

                // Bind the profiles to the ComboBox
                ProfileList.ItemsSource = profiles;
            }
            else {
                MessageBox.Show("User directory not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Event handler for Cancel button click
        private void Cancel_Click (object sender, RoutedEventArgs e) {
            this.Close();
        }

        // Load the file, if it exists. Return what is loaded
        public static BlackBookContainer LoadEncrypted (string userName, X509Certificate2 cert) {
            string path = UserDirectoryManager.GetEncryptedDataPath(userName);

            // Check if the file exists
            if (!File.Exists(path)) {
                // Inform the user that the file does not exist
                MessageBox.Show("File not found", "Black Book", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            // Proceed with reading the file
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                using (var br = new BinaryReader(fs)) {
                    // Decrypt and load the container
                    byte[] encryptedData = br.ReadBytes((int)fs.Length);
                    // Additional decryption logic here...
                }
            }

            return new BlackBookContainer(); // Return the loaded data (or empty if not yet created)
        }


        // Event handler for Unlock button click
        private void Unlock_Click (object sender, RoutedEventArgs e) {
            var selectedProfile = ProfileList.SelectedItem as string;
            if (selectedProfile == null) {
                MessageBox.Show("Please select a profile.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string password = PasswordInput.Password;

            // Attempt to load the session using the selected profile and password
            var success = SessionManager.LoadSession(selectedProfile, password);

            if (success) {
                MessageBox.Show("Login successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // Proceed to the next window or part of the app
            }
            else {
                MessageBox.Show("Incorrect password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // "Create New Profile clicked"
        private void CreateProfile_Click (object sender, RoutedEventArgs e) {
            // Transition directly to the CertificateSetupWindow
            var certSetupWindow = new CertificateSetupWindow();
            certSetupWindow.Show();
            this.Close(); // Close the current window (InitialLoginWindow)
        }




    }
}
