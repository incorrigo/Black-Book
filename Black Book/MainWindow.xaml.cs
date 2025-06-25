/// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
// 2025-06-25 [Wednesday]
// [BlackBook/MainWindow.xaml.cs]
using System;
using System.IO;
using System.Threading;
using System.Windows;
using BlackBook.Views;
using BlackBook.Models;
using BlackBook.Security;

namespace BlackBook;
public partial class MainWindow : Window {
    public MainWindow () {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded (object sender, RoutedEventArgs e) {
        Title = $"Black Book – {SessionManager.CurrentUserName}";

        // Populate data tabs
        PeopleListBox.ItemsSource = SessionManager.Data?.People;
        CompaniesListBox.ItemsSource = SessionManager.Data?.Companies;
        SituationsListBox.ItemsSource = SessionManager.Data?.Situations;
        InteractionsListBox.ItemsSource = SessionManager.Data?.Interactions;
    }

    private void CreateKey_Click (object sender, RoutedEventArgs e) {
        var setup = new CertificateSetupWindow();
        setup.Owner = this;
        setup.ShowDialog();
    }

    private void OpenBook_Click (object sender, RoutedEventArgs e) {
        var entry = new CorrespondenceEntryWindow();
        entry.Owner = this;
        entry.ShowDialog();
    }

    private void ManagePeople_Click (object sender, RoutedEventArgs e) {
        var pm = new PeopleManager();
        pm.Owner = this;
        pm.ShowDialog();

        // Refresh after potential changes
        PeopleListBox.Items.Refresh();
    }

    private async void AddSampleData_Click (object sender, RoutedEventArgs e) {
        var data = SessionManager.Data!;
        var person = new Person { Name = "John Doe", Email = "john.doe@example.com", PhoneNumber = "01234 567890" };
        data.People.Add(person);

        var company = new Company { Name = "Acme Corp", Address = "1 Main Street", Description = "Sample company" };
        data.Companies.Add(company);

        var situation = new Situation { Title = "Onboarding", Description = "Initial sample situation" };
        data.Situations.Add(situation);

        var interaction = new Interaction {
            PersonId = person.Id,
            CompanyId = company.Id,
            SituationId = situation.Id,
            Direction = InteractionDirection.Incoming,
            Type = InteractionType.Email,
            Notes = "Hello world sample note",
            Timestamp = DateTime.UtcNow
        };
        data.Interactions.Add(interaction);

        try {
            var usersRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users");
            await SecureProfileManager.SaveProfileAsync(
                SessionManager.CurrentUserName,
                SessionManager.CurrentPassword,
                data,
                usersRoot,
                CancellationToken.None
            );
            MessageBox.Show("Sample data added successfully.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);

            // Refresh all lists
            PeopleListBox.Items.Refresh();
            CompaniesListBox.Items.Refresh();
            SituationsListBox.Items.Refresh();
            InteractionsListBox.Items.Refresh();
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to save sample data:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Logout_Click (object sender, RoutedEventArgs e) {
        SessionManager.CurrentUserName = string.Empty;
        SessionManager.CurrentPassword = string.Empty;
        SessionManager.Data = null;

        var login = new InitialLoginWindow();
        login.Show();
        Close();
    }

    private void Quit_Click (object sender, RoutedEventArgs e) {
        Close();
    }
}
