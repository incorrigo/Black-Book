using System.Windows;
using BlackBook.Storage;

namespace BlackBook.Views;

public partial class InitialLoginWindow : Window {
    public InitialLoginWindow () {
        InitializeComponent();
        ProfileList.ItemsSource = ProfileSelector.GetAvailableProfiles();
        if (ProfileList.Items.Count > 0)
            ProfileList.SelectedIndex = 0;
    }

    private void Unlock_Click (object sender, RoutedEventArgs e) {
        var name = ProfileList.SelectedItem as string;
        if (name == null) return;

        var password = PasswordInput.Password;
        if (SessionManager.LoadSession(name, password)) {
            new MainWindow().Show();
            Close();
        }
        else {
            MessageBox.Show("Unable to unlock profile. Check your password.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Cancel_Click (object sender, RoutedEventArgs e) {
        Close();
    }
}
