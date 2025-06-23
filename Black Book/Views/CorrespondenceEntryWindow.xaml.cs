using BlackBook;
using BlackBook.Models;
using BlackBook.Security;
using BlackBook.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BlackBook.Views;
public partial class CorrespondenceEntryWindow : Window {
    private readonly List<Person> people;
    private readonly List<Company> companies;
    private readonly List<Situation> situations;
    private readonly List<Interaction> interactions;

    public CorrespondenceEntryWindow () {
        InitializeComponent();
        var data = SessionManager.Data!;
        people = data.People;
        companies = data.Companies;
        situations = data.Situations;
        interactions = data.Interactions;

        TimestampText.Text = DateTime.Now.ToString("yyyy-MM-dd [dddd] - HH:mm");
        PersonTextBox.TextChanged += (_, _) => LoadInteractionHistory();
        HistoryList.SelectionChanged += HistoryList_SelectionChanged;
        LoadInteractionHistory();
    }

    private void LoadInteractionHistory () {
        var name = PersonTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) {
            HistoryList.ItemsSource = null;
            return;
        }

        var relevant = interactions
            .Where(i => people.Any(p => p.Id == i.PersonId
                                      && p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(i => i.Timestamp)
            .ToList();

        HistoryList.ItemsSource = relevant;
    }

    private void SetFormReadonly (bool isReadonly) {
        PersonTextBox.IsReadOnly = isReadonly;
        CompanyTextBox.IsReadOnly = isReadonly;
        NotesTextBox.IsReadOnly = isReadonly;
        SituationComboBox.IsEnabled = !isReadonly;
        DirectionComboBox.IsEnabled = !isReadonly;
        InteractionTypeComboBox.IsEnabled = !isReadonly;
        RelationshipComboBox.IsEnabled = !isReadonly;
        SaveButton.IsEnabled = !isReadonly;
    }

    private void HistoryList_SelectionChanged (object sender, SelectionChangedEventArgs e) {
        if (HistoryList.SelectedItem is not Interaction selected) {
            return;
        }

        var person = people.FirstOrDefault(p => p.Id == selected.PersonId);
        var company = companies.FirstOrDefault(c => c.Id == selected.CompanyId);
        var situation = situations.FirstOrDefault(s => s.Id == selected.SituationId);

        PersonTextBox.Text = person?.Name ?? "";
        CompanyTextBox.Text = company?.Name ?? "";
        NotesTextBox.Text = selected.Notes ?? "";
        DirectionComboBox.SelectedIndex = (int)selected.Direction;
        InteractionTypeComboBox.SelectedIndex = (int)selected.Type;
        RelationshipComboBox.SelectedIndex = person is not null
                                              ? (int)person.Relationship
                                              : 0;
        SituationComboBox.Text = situation?.Title ?? "";

        SetFormReadonly(true);
    }

    private void Cancel_Click (object sender, RoutedEventArgs e) {
        Close();
    }

    private async void Save_Click (object sender, RoutedEventArgs e) {
        if (string.IsNullOrWhiteSpace(PersonTextBox.Text)
         || string.IsNullOrWhiteSpace(CompanyTextBox.Text)
         || string.IsNullOrWhiteSpace(NotesTextBox.Text)) {
            MessageBox.Show("Please fill out all required fields.", "Incomplete",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var person = people
            .FirstOrDefault(p => p.Name.Equals(PersonTextBox.Text, StringComparison.OrdinalIgnoreCase))
                  ?? new Person { Name = PersonTextBox.Text };
        person.Relationship = (RelationshipType)RelationshipComboBox.SelectedIndex;

        var company = companies
            .FirstOrDefault(c => c.Name.Equals(CompanyTextBox.Text, StringComparison.OrdinalIgnoreCase))
                     ?? new Company { Name = CompanyTextBox.Text };

        if (!people.Contains(person)) people.Add(person);
        if (!companies.Contains(company)) companies.Add(company);

        var interaction = new Interaction {
            PersonId = person.Id,
            CompanyId = company.Id,
            Direction = (InteractionDirection)DirectionComboBox.SelectedIndex,
            Type = (InteractionType)InteractionTypeComboBox.SelectedIndex,
            Notes = NotesTextBox.Text,
            Timestamp = DateTime.UtcNow
        };
        interactions.Add(interaction);

        // Persist both key-bundle and JSON container
        try {
            await SecureProfileManager.SaveProfileAsync(
                SessionManager.CurrentUserName,
                SessionManager.CurrentPassword,
                SessionManager.Data!,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
                CancellationToken.None
            );
            MessageBox.Show("Correspondence saved successfully.", "Saved",
                            MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to save data: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ReplyToSelected_Click (object sender, RoutedEventArgs e) {
        if (HistoryList.SelectedItem is not Interaction selected) {
            return;
        }

        var person = people.FirstOrDefault(p => p.Id == selected.PersonId);
        var company = companies.FirstOrDefault(c => c.Id == selected.CompanyId);
        var situation = situations.FirstOrDefault(s => s.Id == selected.SituationId);

        if (person == null || company == null) {
            return;
        }

        PersonTextBox.Text = person.Name;
        CompanyTextBox.Text = company.Name;
        RelationshipComboBox.SelectedIndex = (int)person.Relationship;
        DirectionComboBox.SelectedIndex = selected.Direction == InteractionDirection.Incoming
                                              ? (int)InteractionDirection.Outgoing
                                              : (int)InteractionDirection.Incoming;
        InteractionTypeComboBox.SelectedIndex = (int)selected.Type;
        SituationComboBox.Text = situation?.Title ?? "";

        NotesTextBox.Text = "";
        SetFormReadonly(false);
    }
}

