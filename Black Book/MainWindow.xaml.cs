// MainWindow.xaml.cs
using System.Windows;
using System.IO;
using BlackBook.Models;

namespace BlackBook.Views;

public partial class MainWindow : Window {
    public MainWindow () {
        InitializeComponent();
        DataContext = SessionManager.Data;
        this.MinWidth = 600;
        this.MinHeight = 400;
    }

    private void CloseProfile_Click (object sender, RoutedEventArgs e) {
        new InitialLoginWindow().Show();
        this.Close();
    }

    private void Exit_Click (object sender, RoutedEventArgs e) {
        Application.Current.Shutdown();
    }

    private async void NewPerson_Click (object sender, RoutedEventArgs e) {
        var personWindow = new PersonEntryWindow();
        personWindow.Owner = this;
        bool? result = personWindow.ShowDialog();
        if (result == true) {
            var people = SessionManager.Data!.People;
            if (people.Count > 0) {
                PeopleManagerView.SelectPerson(people[^1]);
            }
        }
    }

    private async void DeleteProfile_Click (object sender, RoutedEventArgs e) {
        var confirmWindow = new ProfileDeleteConfirmWindow();
        confirmWindow.Owner = this;
        if (confirmWindow.ShowDialog() != true)
            return;

        var progressWindow = new ProfileDeleteProgressWindow();
        progressWindow.Owner = this;
        progressWindow.Show();

        var progress = new Progress<double>(val => progressWindow.SetProgress(val));

        try {
            await Security.ForensicProfileDeletion.DeleteProfileAsync(
                confirmWindow.ProfileName,
                confirmWindow.Password,
                confirmWindow.ConfirmDelete,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
                progress);

            progressWindow.Close();

            MessageBox.Show(
                "When a profile is deleted Black Book must be restarted\r\nYour profile has been deleted. Have a nice day!",
                "Forensic Delete", MessageBoxButton.OK, MessageBoxImage.Information);

            Application.Current.Shutdown();
        }
        catch (Exception ex) {
            progressWindow.Close();
            MessageBox.Show($"Profile deletion failed:\n{ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    private async void NewCompany_Click (object sender, RoutedEventArgs e) {
        var companyWindow = new CompanyEntryWindow();
        companyWindow.Owner = this;
        bool? result = companyWindow.ShowDialog();
        if (result == true) {
            var companies = SessionManager.Data!.Companies;
            if (companies.Count > 0) {
                CompanyManagerView.SelectCompany(companies[^1]);
            }
            MainTabControl.SelectedIndex = 2;  // switch to Companies tab
        }
    }

    private async void NewSituation_Click (object sender, RoutedEventArgs e) {
        var situationWindow = new SituationEntryWindow();
        situationWindow.Owner = this;
        bool? result = situationWindow.ShowDialog();
        if (result == true) {
            var situations = SessionManager.Data!.Situations;
            if (situations.Count > 0) {
                SituationManagerView.SelectSituation(situations[^1]);
            }
            MainTabControl.SelectedIndex = 1;  // switch to Situations tab
        }
    }

    private async void NewInteraction_Click (object sender, RoutedEventArgs e) {
        var interactionWindow = new CorrespondenceEntryWindow();
        interactionWindow.Owner = this;
        interactionWindow.ShowDialog();
        // Data bindings will update automatically if new correspondence was saved
    }

    private void ChangePassword_Click (object sender, RoutedEventArgs e) {
        var cpWindow = new ChangePasswordWindow();
        cpWindow.Owner = this;
        cpWindow.ShowDialog();
    }

    private void About_Click (object sender, RoutedEventArgs e) {
        new About { Owner = this }.ShowDialog();
    }

}
