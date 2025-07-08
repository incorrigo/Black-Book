// CompanyEntryWindow.xaml.cs
using System;
using System.Threading;
using System.Windows;
using BlackBook.Models;
using BlackBook.Security;
using BlackBook.Storage;

namespace BlackBook.Views;

public partial class CompanyEntryWindow : Window {
    private readonly BlackBook.Storage.BlackBookContainer data;
    private Company? editingCompany;

    public CompanyEntryWindow (Company? companyToEdit = null) {
        InitializeComponent();
        data = SessionManager.Data!;
        if (companyToEdit != null) {
            Title = "Edit Company";
            NameBox.Text = companyToEdit.Name;
            AddrBox.Text = companyToEdit.Address;
            PhoneBox.Text = companyToEdit.PhoneNumber;
            DescBox.Text = companyToEdit.Description;
            editingCompany = companyToEdit;
        }
    }

    private void Cancel_Click (object sender, RoutedEventArgs e) {
        DialogResult = false;
        Close();
    }

    private async void Save_Click (object sender, RoutedEventArgs e) {
        if (string.IsNullOrWhiteSpace(NameBox.Text)) {
            MessageBox.Show("A company needs to have a name", "New Company",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (editingCompany != null) {
            // Edit company
            editingCompany.Name = NameBox.Text.Trim();
            editingCompany.Address = AddrBox.Text.Trim();
            editingCompany.PhoneNumber = PhoneBox.Text.Trim();
            editingCompany.Website = WebsiteBox.Text.Trim();
            editingCompany.Description = DescBox.Text.Trim();
        }
        else {
            // Create new company
            var company = new Company {
                Name = NameBox.Text.Trim(),
                Address = AddrBox.Text.Trim(),
                PhoneNumber = PhoneBox.Text.Trim(),
                Website = WebsiteBox.Text.Trim(),
                Description = DescBox.Text.Trim()
            };
            data.Companies.Add(company);
        }
        try {
            await SecureProfileManager.SaveProfileAsync(SessionManager.CurrentUserName, SessionManager.CurrentPassword,
                                                        data,
                                                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
                                                        CancellationToken.None);
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to save data:\n{ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        MessageBox.Show(editingCompany != null ? "The updated company details have been filed"
                                               : "Your new company has been filed",
                        "Black Book", MessageBoxButton.OK, MessageBoxImage.None);
        DialogResult = true;
        Close();
    }
}
