// PersonEntryWindow.xaml.cs
using BlackBook.Models;
using BlackBook.Security;
using BlackBook.Storage;
using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace BlackBook.Views {
    public partial class PersonEntryWindow : Window {
        private readonly BlackBookContainer data;

        public PersonEntryWindow () {
            InitializeComponent();
            data = SessionManager.Data!;      // the profile data is already loaded after login
            DataContext = data;               // bind to allow CompanyBox ItemsSource to work
        }

        private void Cancel_Click (object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private async void Save_Click (object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(NameBox.Text)) {
                MessageBox.Show("Name is required.", "Incomplete",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Resolve or create associated Company if provided
            Company? company = null;
            var compName = CompanyBox.Text?.Trim();
            if (!string.IsNullOrEmpty(compName)) {
                company = data.Companies.FirstOrDefault(c => c.Name.Equals(compName, StringComparison.OrdinalIgnoreCase))
                          ?? new Company { Name = compName };
                if (!data.Companies.Contains(company)) {
                    data.Companies.Add(company);
                }
            }

            // Create and add the new Person
            var person = new Person {
                Name = NameBox.Text.Trim(),
                Email = EmailBox.Text.Trim(),
                PhoneNumber = PhoneBox.Text.Trim(),
                CompanyId = company?.Id,
                Relationship = (RelationshipType)RelationBox.SelectedIndex
            };
            data.People.Add(person);

            // Persist the profile with the new person (ensuring encryption is maintained)
            try {
                await SecureProfileManager.SaveProfileAsync(
                    SessionManager.CurrentUserName,
                    SessionManager.CurrentPassword,
                    data,
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
                    CancellationToken.None
                );
                // Indicate success and close
                MessageBox.Show("Person added successfully.", "Saved",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to save data:\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // If save fails, we might consider removing the added person to keep data consistent, 
                // but since it didn't persist to disk, the in-memory data still has it. 
                // For now, just leave it and inform the user.
            }
        }
    }
}
