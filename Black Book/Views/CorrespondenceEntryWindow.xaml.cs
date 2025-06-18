using System;
using System.Windows;
using System.Windows.Controls;
using BlackBook.Models;
using BlackBook.Storage;

namespace BlackBook.Views;

public partial class CorrespondenceEntryWindow : Window {
    public CorrespondenceEntryWindow () {
        InitializeComponent();
        TimestampText.Text = DateTime.Now.ToString("yyyy-MM-dd [dddd] - HH:mm");
        UpdatePlaceholders();
    }

    private void Cancel_Click (object sender, RoutedEventArgs e) {
        Close();
    }

    private void Save_Click (object sender, RoutedEventArgs e) {
        if (string.IsNullOrWhiteSpace(PersonTextBox.Text) ||
            string.IsNullOrWhiteSpace(CompanyTextBox.Text) ||
            string.IsNullOrWhiteSpace(NotesTextBox.Text)) {
            MessageBox.Show("Please fill out all required fields.", "Incomplete",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var person = new Person { Name = PersonTextBox.Text };
        var company = new Company { Name = CompanyTextBox.Text };
        var interaction = new Interaction {
            PersonId = person.Id,
            CompanyId = company.Id,
            Direction = (InteractionDirection)DirectionComboBox.SelectedIndex,
            Type = (InteractionType)InteractionTypeComboBox.SelectedIndex,
            Notes = NotesTextBox.Text,
            Timestamp = DateTime.UtcNow
        };

        var people = DataStore.Load<Person>("people.json");
        var companies = DataStore.Load<Company>("companies.json");
        var interactions = DataStore.Load<Interaction>("interactions.json");

        if (!people.Exists(p => p.Name.Equals(person.Name, StringComparison.OrdinalIgnoreCase)))
            people.Add(person);

        if (!companies.Exists(c => c.Name.Equals(company.Name, StringComparison.OrdinalIgnoreCase)))
            companies.Add(company);

        interactions.Add(interaction);

        DataStore.Save("people.json", people);
        DataStore.Save("companies.json", companies);
        DataStore.Save("interactions.json", interactions);

        MessageBox.Show("Correspondence saved successfully.", "Saved",
                        MessageBoxButton.OK, MessageBoxImage.Information);
        Close();
    }

    private void UpdatePlaceholder (TextBox box, TextBlock placeholder) {
        placeholder.Visibility = string.IsNullOrWhiteSpace(box.Text)
            ? Visibility.Visible
            : Visibility.Hidden;
    }

    private void PersonTextBox_TextChanged (object sender, TextChangedEventArgs e) {
        UpdatePlaceholder(PersonTextBox, PersonPlaceholder);
    }

    private void CompanyTextBox_TextChanged (object sender, TextChangedEventArgs e) {
        UpdatePlaceholder(CompanyTextBox, CompanyPlaceholder);
    }

    private void NotesTextBox_TextChanged (object sender, TextChangedEventArgs e) {
        UpdatePlaceholder(NotesTextBox, NotesPlaceholder);
    }

    private void UpdatePlaceholders () {
        UpdatePlaceholder(PersonTextBox, PersonPlaceholder);
        UpdatePlaceholder(CompanyTextBox, CompanyPlaceholder);
        UpdatePlaceholder(NotesTextBox, NotesPlaceholder);
    }
}
