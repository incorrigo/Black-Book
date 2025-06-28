// CompanyEntryWindow.xaml.cs
using BlackBook.Models;
using BlackBook.Security;
using BlackBook.Storage;
using System;
using System.Threading;
using System.Windows;

namespace BlackBook.Views {
    public partial class CompanyEntryWindow : Window {
        private readonly BlackBookContainer data;
        public CompanyEntryWindow () {
            InitializeComponent();
            data = SessionManager.Data!;
        }

        private void Cancel_Click (object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private async void Save_Click (object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(NameBox.Text)) {
                MessageBox.Show("Company name is required.", "Incomplete",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Create company object
            var company = new Company {
                Name = NameBox.Text.Trim(),
                Address = AddrBox.Text.Trim()
            };
            data.Companies.Add(company);
            try {
                await SecureProfileManager.SaveProfileAsync(
                    SessionManager.CurrentUserName,
                    SessionManager.CurrentPassword,
                    data,
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
                    CancellationToken.None
                );
                MessageBox.Show("Company added successfully.", "Saved",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to save data:\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
