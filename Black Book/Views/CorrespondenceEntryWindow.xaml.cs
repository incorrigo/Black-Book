using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlackBook.Models;
using BlackBook.Storage;

namespace BlackBook.Views;

public partial class CorrespondenceEntryWindow : Window {

    private List<Person> people;
    private List<Company> companies;
    private List<Situation> situations;
    private List<Interaction> interactions;


    public CorrespondenceEntryWindow () {
        InitializeComponent();
        LoadData();
        TimestampText.Text = DateTime.Now.ToString("yyyy-MM-dd [dddd] - HH:mm");
        UpdatePlaceholders();

        PersonTextBox.TextChanged += (_, _) => LoadInteractionHistory();
        HistoryList.SelectionChanged += HistoryList_SelectionChanged;


        var people = DataStore.Load<Person>("people.json");
        var allInteractions = DataStore.Load<Interaction>("interactions.json");
        var matchName = PersonTextBox.Text.Trim();

        var relevant = allInteractions
            .Where(i => {
                var p = people.FirstOrDefault(p => p.Id == i.PersonId);
                return p != null && p.Name.Equals(matchName, StringComparison.OrdinalIgnoreCase);
            })
            .OrderByDescending(i => i.Timestamp)
            .ToList();

        HistoryList.ItemsSource = relevant;

    }

    private void LoadData () {
        people = DataStore.Load<Person>("people.json");
        companies = DataStore.Load<Company>("companies.json");
        situations = DataStore.Load<Situation>("situations.json");
        interactions = DataStore.Load<Interaction>("interactions.json");
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

        var person = new Person {
            Name = PersonTextBox.Text,
            Relationship = (RelationshipType)RelationshipComboBox.SelectedIndex
        };

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

    private void ReplyToSelected_Click (object sender, RoutedEventArgs e) {
        if (HistoryList.SelectedItem is not Interaction selected)
            return;

        var person = people.FirstOrDefault(p => p.Id == selected.PersonId);
        var company = companies.FirstOrDefault(c => c.Id == selected.CompanyId);
        var situation = situations.FirstOrDefault(s => s.Id == selected.SituationId);

        if (person == null || company == null)
            return;

        // Autofill fields
        PersonTextBox.Text = person.Name;
        CompanyTextBox.Text = company.Name;
        RelationshipComboBox.SelectedIndex = (int)person.Relationship;
        DirectionComboBox.SelectedIndex = selected.Direction == InteractionDirection.Incoming
            ? (int)InteractionDirection.Outgoing
            : (int)InteractionDirection.Incoming;
        InteractionTypeComboBox.SelectedIndex = (int)selected.Type;
        SituationComboBox.Text = situation?.Title ?? "";

        // Clear notes for new entry
        NotesTextBox.Text = "";
        SetFormReadonly(false);
    }

    private void UpdatePlaceholder (TextBox box, TextBlock placeholder) {
        placeholder.Visibility = string.IsNullOrWhiteSpace(box.Text)
            ? Visibility.Visible
            : Visibility.Hidden;
    }

    private void PersonTextBox_TextChanged (object sender, TextChangedEventArgs e) {
        UpdatePlaceholder(PersonTextBox, PersonPlaceholder);
        LoadInteractionHistory();
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
