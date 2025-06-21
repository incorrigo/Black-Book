using BlackBook.Models;
using BlackBook.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace BlackBook.Views;

public partial class CorrespondenceEntryWindow : Window {
    private List<Person> people;
    private List<Company> companies;
    private List<Situation> situations;
    private List<Interaction> interactions;

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
        var matchName = PersonTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(matchName)) {
            HistoryList.ItemsSource = null;
            return;
        }

        var relevant = interactions
            .Where(i => {
                var p = people.FirstOrDefault(p => p.Id == i.PersonId);
                return p != null && p.Name.Equals(matchName, StringComparison.OrdinalIgnoreCase);
            })
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
        if (HistoryList.SelectedItem is not Interaction selected) return;

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

    private void Cancel_Click (object sender, RoutedEventArgs e) => Close();

    private void Save_Click (object sender, RoutedEventArgs e) {
        if (string.IsNullOrWhiteSpace(PersonTextBox.Text) ||
            string.IsNullOrWhiteSpace(CompanyTextBox.Text) ||
            string.IsNullOrWhiteSpace(NotesTextBox.Text)) {
            MessageBox.Show("Please fill out all required fields.", "Incomplete",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var person = people.FirstOrDefault(p => p.Name.Equals(PersonTextBox.Text, StringComparison.OrdinalIgnoreCase))
                  ?? new Person { Name = PersonTextBox.Text };
        person.Relationship = (RelationshipType)RelationshipComboBox.SelectedIndex;

        var company = companies.FirstOrDefault(c => c.Name.Equals(CompanyTextBox.Text, StringComparison.OrdinalIgnoreCase))
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

        // Save encrypted container
        var path = UserDirectoryManager.GetEncryptedDataPath(SessionManager.CurrentUserName);
        EncryptedContainerManager.SaveEncrypted(SessionManager.Data!, SessionManager.CurrentUserName);



        MessageBox.Show("Correspondence saved successfully.", "Saved",
                        MessageBoxButton.OK, MessageBoxImage.Information);
        Close();
    }

    private void ReplyToSelected_Click (object sender, RoutedEventArgs e) {
        if (HistoryList.SelectedItem is not Interaction selected)
            return;

        var person = people.FirstOrDefault(p => p.Id == selected.PersonId);
        var company = companies.FirstOrDefault(c => c.Id == selected.CompanyId);
        var situation = situations.FirstOrDefault(s => s.Id == selected.SituationId);

        if (person == null || company == null)
            return;

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
