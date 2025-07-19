using BlackBook.Storage;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace BlackBook.Views;

public partial class InitialLoginWindow : Window {
    public ObservableCollection<ProfileItem> Profiles { get; set; } = new();
    private CollectionViewSource profileViewSource;

    public InitialLoginWindow () {
        InitializeComponent();
        LoadProfiles();
        ProfileList.ItemsSource = Profiles;
    }

    private void LoadProfiles () {
        Profiles.Clear();
        var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users");
        if (!Directory.Exists(root)) return;

        foreach (var dir in Directory.GetDirectories(root)) {
            var profileName = Path.GetFileName(dir);
            var hasData = File.Exists(Path.Combine(dir, "file"));
            var hasKey = File.Exists(Path.Combine(dir, "file.file"));

            Profiles.Add(new ProfileItem {
                Name = profileName,
                HasData = hasData,
                HasKey = hasKey,
                DotColor = (hasData && hasKey) ? Brushes.Blue : Brushes.Gray
            });
        }

        // Explicitly reset ItemsSource to refresh ListView
        ProfileList.ItemsSource = null;
        ProfileList.ItemsSource = Profiles;
    }

    private void CreateProfile_Click (object sender, RoutedEventArgs e) {
        var setup = new CertificateSetupWindow(onProfileCreated: LoadProfiles) { Owner = this };
        if (setup.ShowDialog() == true) {
            if (Profiles.Count > 0) {
                ProfileList.SelectedIndex = Profiles.Count - 1;
                PasswordInput.Clear();
                PasswordInput.Focus();
            }
        }
    }



    private void Cancel_Click (object sender, RoutedEventArgs e) => Close();

    private void ProfileList_SelectionChanged (object sender, SelectionChangedEventArgs e) {
        PasswordInput.Clear();

        // Dispatcher guarantees UI focus update
        Dispatcher.BeginInvoke(new Action(() => {
            PasswordInput.Focus();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }



    private async void Unlock_Click (object sender, RoutedEventArgs e) {
        if (ProfileList.SelectedItem is not ProfileItem selectedProfile) {
            MessageBox.Show("Please select a profile first.", "Incomplete",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!selectedProfile.HasKey || !selectedProfile.HasData) {
            MessageBox.Show("Selected profile is missing essential files.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var password = PasswordInput.Password;
        if (string.IsNullOrWhiteSpace(password)) {
            MessageBox.Show("Enter a password.", "Incomplete",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        bool ok;
        try {
            ok = await SessionManager.LoadSessionAsync(selectedProfile.Name, password);
        }
        catch (Exception ex) {
            MessageBox.Show($"Could not open profile: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!ok) {
            MessageBox.Show("Incorrect password.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        new MainWindow().Show();
        Close();
    }

    
}

public class ProfileItem {
    public string Name { get; set; } = string.Empty;
    public bool HasData { get; set; }
    public bool HasKey { get; set; }
    public Brush DotColor { get; set; } = Brushes.Gray;
}
