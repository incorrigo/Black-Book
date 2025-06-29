// PersonEntryWindow.xaml.cs
using System.Linq;
using System.Threading;
using System.Windows;
using BlackBook.Models;
using BlackBook.Security;
using BlackBook.Storage;

namespace BlackBook.Views;

public partial class PersonEntryWindow : Window {
    private readonly BlackBook.Storage.BlackBookContainer data;
    private Person? editingPerson;

    public PersonEntryWindow (Person? personToEdit = null) {
        InitializeComponent();
        data = SessionManager.Data!;
        DataContext = data;
        if (personToEdit != null) {
            Title = "Edit Person";
            NameBox.Text = personToEdit.Name;
            EmailBox.Text = personToEdit.Email;
            PhoneBox.Text = personToEdit.PhoneNumber;
            // Pre-fill company name if applicable
            var comp = data.Companies.FirstOrDefault(c => c.Id == personToEdit.CompanyId);
            CompanyBox.Text = comp?.Name ?? string.Empty;
            RelationBox.SelectedIndex = (int)personToEdit.Relationship;
            PositionBox.Text = personToEdit.Position;
            NotesBox.Text = personToEdit.Notes;
            editingPerson = personToEdit;
        }
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
            company = data.Companies.FirstOrDefault(c => c.Name.Equals(compName, System.StringComparison.OrdinalIgnoreCase));
            if (company == null) {
                company = new Company { Name = compName };
                data.Companies.Add(company);
            }
        }
        if (editingPerson != null) {
            // Update existing person
            editingPerson.Name = NameBox.Text.Trim();
            editingPerson.Email = EmailBox.Text.Trim();
            editingPerson.PhoneNumber = PhoneBox.Text.Trim();
            editingPerson.CompanyId = company?.Id;  // can be null if no company
            editingPerson.Relationship = (RelationshipType)RelationBox.SelectedIndex;
            editingPerson.Position = PositionBox.Text.Trim();
            editingPerson.Notes = NotesBox.Text.Trim();
        }
        else {
            // Create new person
            var person = new Person {
                Name = NameBox.Text.Trim(),
                Email = EmailBox.Text.Trim(),
                PhoneNumber = PhoneBox.Text.Trim(),
                CompanyId = company?.Id,
                Relationship = (RelationshipType)RelationBox.SelectedIndex,
                Position = PositionBox.Text.Trim(),
                Notes = NotesBox.Text.Trim()
            };
            data.People.Add(person);
        }
        try {
            await SecureProfileManager.SaveProfileAsync(SessionManager.CurrentUserName, SessionManager.CurrentPassword,
                                                        data,
                                                        System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Users"),
                                                        CancellationToken.None);
        }
        catch (System.Exception ex) {
            MessageBox.Show($"Failed to save data:\n{ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        MessageBox.Show(editingPerson != null ? "Person details updated successfully."
                                              : "Person added successfully.",
                        "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        DialogResult = true;
        Close();
    }

    /// <summary>Pre-fill the Company field for a new person.</summary>
    public void PrefillCompany (string companyName) {
        CompanyBox.Text = companyName;
    }
}
