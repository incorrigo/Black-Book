// CorrespondenceEntryWindow.xaml.cs
using BlackBook;
using BlackBook.Models;
using BlackBook.Security;
using BlackBook.Storage;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BlackBook.Views;

public partial class CorrespondenceEntryWindow : Window {
    private readonly BlackBookContainer data;
    private readonly ObservableCollection<Person> people;
    private readonly ObservableCollection<Company> companies;
    private readonly ObservableCollection<Situation> situations;
    private readonly ObservableCollection<Interaction> interactions;

    public CorrespondenceEntryWindow () {
        InitializeComponent();
        data = SessionManager.Data!;
        DataContext = data;
        people = data.People;
        companies = data.Companies;
        situations = data.Situations;
        interactions = data.Interactions;
        TimestampText.Text = DateTime.Now.ToString("yyyy-MM-dd [dddd] - HH:mm");
        PersonComboBox.SelectionChanged += PersonComboBox_SelectionChanged;
        PersonComboBox.LostFocus += PersonComboBox_LostFocus;
        HistoryList.SelectionChanged += HistoryList_SelectionChanged;
        // Pre-load history if person field already has content
        LoadInteractionHistory();
    }

    private void LoadInteractionHistory () {
        var name = PersonComboBox.Text.Trim();
        if (PersonComboBox.SelectedItem is Person selectedPerson) {
            // Show interactions involving this person (by Id)
            var relevant = interactions.Where(i => i.PersonId == selectedPerson.Id)
                                       .OrderByDescending(i => i.Timestamp)
                                       .ToList();
            HistoryList.ItemsSource = relevant;
        }
        else if (!string.IsNullOrWhiteSpace(name)) {
            // If a name is typed (not selected from list), find matching person
            var relevant = interactions.Where(i => {
                var p = people.FirstOrDefault(p => p.Id == i.PersonId);
                return p != null && p.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
            })
                .OrderByDescending(i => i.Timestamp)
                .ToList();
            HistoryList.ItemsSource = relevant;
        }
        else {
            HistoryList.ItemsSource = null;
        }
    }

    private void PersonComboBox_SelectionChanged (object sender, SelectionChangedEventArgs e) {
        if (PersonComboBox.SelectedItem is Person person) {
            // When an existing person is selected, fill related fields
            CompanyComboBox.Text = companies.FirstOrDefault(c => c.Id == person.CompanyId)?.Name ?? string.Empty;
            RelationshipComboBox.SelectedIndex = (int)person.Relationship;
            LoadInteractionHistory();
        }
    }

    private void PersonComboBox_LostFocus (object sender, RoutedEventArgs e) {
        if (!string.IsNullOrWhiteSpace(PersonComboBox.Text) && PersonComboBox.SelectedItem == null) {
            // If user typed a name exactly matching an existing person, auto-select their relationship/company
            var person = people.FirstOrDefault(p => p.Name.Equals(PersonComboBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
            if (person != null) {
                RelationshipComboBox.SelectedIndex = (int)person.Relationship;
                CompanyComboBox.Text = companies.FirstOrDefault(c => c.Id == person.CompanyId)?.Name ?? CompanyComboBox.Text;
            }
            else {
                RelationshipComboBox.SelectedIndex = 0;
            }
        }
        LoadInteractionHistory();
    }

    private void HistoryList_SelectionChanged (object sender, SelectionChangedEventArgs e) {
        if (HistoryList.SelectedItem is not Interaction selected) {
            return;
        }
        // Populate form fields with the selected past interaction's data for viewing
        var person = people.FirstOrDefault(p => p.Id == selected.PersonId);
        var company = companies.FirstOrDefault(c => c.Id == selected.CompanyId);
        var situation = situations.FirstOrDefault(s => s.Id == selected.SituationId);

        PersonComboBox.Text = person?.Name ?? "";
        CompanyComboBox.Text = company?.Name ?? "";
        NotesTextBox.Text = selected.Notes ?? "";
        DirectionComboBox.SelectedIndex = (int)selected.Direction;
        InteractionTypeComboBox.SelectedIndex = (int)selected.Type;
        RelationshipComboBox.SelectedIndex = person is not null ? (int)person.Relationship : 0;
        SituationComboBox.Text = situation?.Title ?? "";

        // Make form read-only to avoid editing historical entry
        SetFormReadonly(true);
    }

    private void Cancel_Click (object sender, RoutedEventArgs e) {
        Close();
    }

    private async void Save_Click (object sender, RoutedEventArgs e) {
        if (string.IsNullOrWhiteSpace(PersonComboBox.Text)
         || string.IsNullOrWhiteSpace(CompanyComboBox.Text)
         || string.IsNullOrWhiteSpace(NotesTextBox.Text)) {
            MessageBox.Show("Please fill out all required fields (Person, Company, and Notes).", "Incomplete",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        // Resolve or create Person
        var personName = PersonComboBox.Text.Trim();
        var person = people.FirstOrDefault(p => p.Name.Equals(personName, StringComparison.OrdinalIgnoreCase));
        if (person == null) {
            person = new Person { Name = personName };
            person.Relationship = (RelationshipType)RelationshipComboBox.SelectedIndex;
            people.Add(person);
        }
        else {
            // Update relationship if changed
            person.Relationship = (RelationshipType)RelationshipComboBox.SelectedIndex;
        }

        // Resolve or create Company
        var companyName = CompanyComboBox.Text.Trim();
        var company = companies.FirstOrDefault(c => c.Name.Equals(companyName, StringComparison.OrdinalIgnoreCase));
        if (company == null) {
            company = new Company { Name = companyName };
            companies.Add(company);
        }

        // Ensure person's CompanyId is set
        person.CompanyId = company.Id;

        // Resolve or create Situation
        Situation situation;
        var situationTitle = SituationComboBox.Text.Trim();
        if (!string.IsNullOrEmpty(situationTitle)) {
            situation = situations.FirstOrDefault(s => s.Title.Equals(situationTitle, StringComparison.OrdinalIgnoreCase));
            if (situation == null) {
                situation = new Situation { Title = situationTitle, Status = SituationStatus.Ongoing };
                situations.Add(situation);
            }
        }
        else {
            situation = situations.FirstOrDefault(s => s.Title.Equals("AdHoc", StringComparison.OrdinalIgnoreCase));
            if (situation == null) {
                situation = new Situation { Title = "AdHoc", Status = SituationStatus.Ongoing };
                situations.Add(situation);
            }
        }

        // Create the Interaction object
        var interaction = new Interaction {
            PersonId = person.Id,
            CompanyId = company.Id,
            SituationId = situation.Id,
            Direction = (InteractionDirection)DirectionComboBox.SelectedIndex,
            Type = (InteractionType)InteractionTypeComboBox.SelectedIndex,
            Notes = NotesTextBox.Text.Trim(),
            Timestamp = DateTime.UtcNow
        };
        interactions.Add(interaction);

        try {
            await SecureProfileManager.SaveProfileAsync(SessionManager.CurrentUserName, SessionManager.CurrentPassword,
                                                        SessionManager.Data!,
                                                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
                                                        CancellationToken.None);
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to save data:\n{ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        MessageBox.Show("Correspondence saved successfully.", "Saved",
                        MessageBoxButton.OK, MessageBoxImage.Information);
        DialogResult = true;
        Close();
    }

    private void ReplyToSelected_Click (object sender, RoutedEventArgs e) {
        if (HistoryList.SelectedItem is not Interaction selected) {
            return;
        }
        // Prepare form to enter a response to the selected interaction
        var person = people.FirstOrDefault(p => p.Id == selected.PersonId);
        var company = companies.FirstOrDefault(c => c.Id == selected.CompanyId);
        var situation = situations.FirstOrDefault(s => s.Id == selected.SituationId);
        if (person == null || company == null) {
            return;
        }
        PersonComboBox.Text = person.Name;
        CompanyComboBox.Text = company.Name;
        RelationshipComboBox.SelectedIndex = (int)person.Relationship;
        // Set opposite direction for reply (incoming <-> outgoing, mutual -> outgoing by default)
        DirectionComboBox.SelectedIndex = selected.Direction == InteractionDirection.Incoming
                                          ? (int)InteractionDirection.Outgoing
                                          : (int)InteractionDirection.Incoming;
        if (selected.Direction == InteractionDirection.Mutual) {
            DirectionComboBox.SelectedIndex = (int)InteractionDirection.Outgoing;
        }
        InteractionTypeComboBox.SelectedIndex = (int)selected.Type;
        SituationComboBox.Text = situation?.Title ?? "";
        NotesTextBox.Text = "";
        // Enable form for new entry
        SetFormReadonly(false);
        // Clear selection to avoid confusion
        HistoryList.SelectedItem = null;
    }

    private void SetFormReadonly (bool isReadonly) {
        PersonComboBox.IsEnabled = !isReadonly;
        CompanyComboBox.IsEnabled = !isReadonly;
        NotesTextBox.IsReadOnly = isReadonly;
        SituationComboBox.IsEnabled = !isReadonly;
        DirectionComboBox.IsEnabled = !isReadonly;
        InteractionTypeComboBox.IsEnabled = !isReadonly;
        RelationshipComboBox.IsEnabled = !isReadonly;
        SaveButton.IsEnabled = !isReadonly;
    }

    // Helper methods to prefill data for new interactions/replies:

    public void PrefillPerson (Person person) {
        // Select the person to auto-fill their details
        PersonComboBox.SelectedItem = person;
    }

    public void PrefillSituation (Situation situation) {
        SituationComboBox.SelectedItem = situation;
    }

    public void PrefillCompany (Company company) {
        CompanyComboBox.SelectedItem = company;
    }

    public void PrefillForReply (Interaction selected) {
        var person = people.FirstOrDefault(p => p.Id == selected.PersonId);
        var company = companies.FirstOrDefault(c => c.Id == selected.CompanyId);
        var situation = situations.FirstOrDefault(s => s.Id == selected.SituationId);

        PersonComboBox.Text = person?.Name ?? "";
        CompanyComboBox.Text = company?.Name ?? "";
        RelationshipComboBox.SelectedIndex = person != null ? (int)person.Relationship : 0;
        SituationComboBox.Text = situation?.Title ?? "";
        // Set reply direction (opposite or outgoing if mutual)
        DirectionComboBox.SelectedIndex = selected.Direction == InteractionDirection.Incoming
                                          ? (int)InteractionDirection.Outgoing
                                          : selected.Direction == InteractionDirection.Outgoing
                                              ? (int)InteractionDirection.Incoming
                                              : (int)InteractionDirection.Outgoing;
        InteractionTypeComboBox.SelectedIndex = (int)selected.Type;
        NotesTextBox.Text = "";
        SetFormReadonly(false);
        HistoryList.SelectedItem = null;
    }
}
