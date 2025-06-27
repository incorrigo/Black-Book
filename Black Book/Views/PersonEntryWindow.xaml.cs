using BlackBook.Models;
using BlackBook.Security;
using BlackBook.Storage;
using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace BlackBook.Views;

public partial class PersonEntryWindow : Window {
    private readonly BlackBookContainer data;

    public PersonEntryWindow () {
        InitializeComponent();
        data = SessionManager.Data!;            // never null after login
        DataContext = data;                     // for CompanyBox binding
    }

    /* ----------  buttons  ---------- */

    private void Cancel_Click (object sender, RoutedEventArgs e) => Close();

    private async void Save_Click (object sender, RoutedEventArgs e) {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) {
            MessageBox.Show("Name is required.", "Incomplete",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        /* --- resolve / create company ------------------------------------ */
        Company company = null!;
        var compName = CompanyBox.Text?.Trim();
        if (!string.IsNullOrEmpty(compName)) {
            company = data.Companies.FirstOrDefault(
                          c => c.Name.Equals(compName,
                                             StringComparison.OrdinalIgnoreCase))
                      ?? new Company { Name = compName };

            if (!data.Companies.Contains(company))
                data.Companies.Add(company);
        }

        /* --- create person ----------------------------------------------- */
        var person = new Person {
            Name = NameBox.Text.Trim(),
            Email = EmailBox.Text.Trim(),
            PhoneNumber = PhoneBox.Text.Trim(),
            CompanyId = company?.Id,
            Relationship = (RelationshipType)RelationBox.SelectedIndex
        };
        data.People.Add(person);

        /* --- persist profile --------------------------------------------- */
        try {
            await SecureProfileManager.SaveProfileAsync(
            SessionManager.CurrentUserName,
            SessionManager.CurrentPassword,
            data,
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
            CancellationToken.None);

            DialogResult = true;   // signals success
            Close();
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to save data:\n{ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
