// ProfileDeleteConfirmWindow.xaml.cs
using System.Windows;

namespace BlackBook.Views;

public partial class ProfileDeleteConfirmWindow : Window {
    public string ProfileName => ProfileNameBox.Text.Trim();
    public string Password => PasswordBox.Password;
    public string ConfirmDelete => ConfirmDeleteBox.Text.Trim();

    public ProfileDeleteConfirmWindow () {
        InitializeComponent();
    }

    private void Cancel_Click (object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    private void Delete_Click (object sender, RoutedEventArgs e) {
        if (string.IsNullOrEmpty(ProfileName) || string.IsNullOrEmpty(Password) || ConfirmDelete != "DELETE") {
            MessageBox.Show("You must type your profile name, password, and DELETE", "Delete Confirmation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        DialogResult = true;
    }
}
