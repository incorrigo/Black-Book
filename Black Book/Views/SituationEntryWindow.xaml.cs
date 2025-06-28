// SituationEntryWindow.xaml.cs
using BlackBook.Models;
using BlackBook.Security;
using BlackBook.Storage;
using System;
using System.Threading;
using System.Windows;

namespace BlackBook.Views {
    public partial class SituationEntryWindow : Window {
        private readonly BlackBookContainer data;
        public SituationEntryWindow () {
            InitializeComponent();
            data = SessionManager.Data!;
        }

        private void Cancel_Click (object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private async void Save_Click (object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(TitleBox.Text)) {
                MessageBox.Show("Situation title is required.", "Incomplete",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Determine status from selection (assuming 0=Active, 1=Closed for now)
            var status = (SituationStatus)StatusBox.SelectedIndex;
            // Create situation
            var situation = new Situation {
                Title = TitleBox.Text.Trim(),
                Status = status
            };
            data.Situations.Add(situation);
            try {
                await SecureProfileManager.SaveProfileAsync(
                    SessionManager.CurrentUserName,
                    SessionManager.CurrentPassword,
                    data,
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
                    CancellationToken.None
                );
                MessageBox.Show("Situation added successfully.", "Saved",
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
