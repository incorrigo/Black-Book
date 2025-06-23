using BlackBook.Security;
using BlackBook.Storage;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
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


        private void Unlock_Click (object sender, RoutedEventArgs e) {
            var name = ProfileList.SelectedItem as string;
            var password = PasswordInput.Password;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password)) {
                MessageBox.Show("Select profile and enter password.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1️⃣ Load cert via literal composite
            var pfxPwd = SecurityManager.CreatePfxPassword(name, password);
            X509Certificate2 cert;
            try {
                cert = new X509Certificate2(
                    UserDirectoryManager.GetUserCertPath(name),
                    pfxPwd,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet
                );
            }
            catch {
                MessageBox.Show("Wrong certificate password.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 2️⃣ Load & decrypt JSON container
            BlackBookContainer data;
            try {
                data = EncryptedContainerManager.LoadEncrypted(name, password);
            }
            catch {
                MessageBox.Show("Failed to decrypt data file.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 3️⃣ Success → stash in SessionManager & open MainWindow
            SessionManager.CurrentUserName = name;
            SessionManager.Certificate = cert;
            SessionManager.Data = data;
            SessionManager.Data.LastOpened = DateTime.UtcNow;
            SessionManager.Data.AccessCount++;
            EncryptedContainerManager.SaveEncrypted(data, name, password);

            var main = new MainWindow();
            main.Show();
            this.Close();
        }

        private void CreateProfile_Click (object sender, RoutedEventArgs e) {
            var setup = new CertificateSetupWindow();
            setup.Owner = this;
            setup.ShowDialog();

            // After creation, reload the profile dropdown
            LoadProfiles();
        }





    }
}
