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

namespace BlackBook.Views {
    public partial class CorrespondenceEntryWindow : Window {
        private readonly BlackBookContainer data;
        private readonly ObservableCollection<Person> people;
        private readonly ObservableCollection<Company> companies;
        private readonly ObservableCollection<Situation> situations;
        private readonly ObservableCollection<Interaction> interactions;

        public CorrespondenceEntryWindow () {
            InitializeComponent();
            data = SessionManager.Data!;
            people = data.People;
            companies = data.Companies;
            situations = data.Situations;
            interactions = data.Interactions;
            // Initialize UI fields
            TimestampText.Text = DateTime.Now.ToString("yyyy-MM-dd [dddd] - HH:mm");
            // Setup event handlers
            PersonTextBox.TextChanged += (_, _) => LoadInteractionHistory();
            HistoryList.SelectionChanged += HistoryList_SelectionChanged;
            // Pre-load history if person field already has content (e.g., from Reply action)
            LoadInteractionHistory();
        }

        private void LoadInteractionHistory () {
            var name = PersonTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) {
                HistoryList.ItemsSource = null;
            }
            else {
                // Find interactions involving a person with this name
                var relevant = interactions
                    .Where(i => {
                        var p = people.FirstOrDefault(p => p.Id == i.PersonId);
                        return p != null && p.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
                    })
                    .OrderByDescending(i => i.Timestamp)
                    .ToList();
                HistoryList.ItemsSource = relevant;
            }
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
            // Populate the form fields with the selected interaction's data for viewing
            var person = people.FirstOrDefault(p => p.Id == selected.PersonId);
            var company = companies.FirstOrDefault(c => c.Id == selected.CompanyId);
            var situation = situations.FirstOrDefault(s => s.Id == selected.SituationId);

            PersonTextBox.Text = person?.Name ?? "";
            CompanyTextBox.Text = company?.Name ?? "";
            NotesTextBox.Text = selected.Notes ?? "";
            DirectionComboBox.SelectedIndex = (int)selected.Direction;
            InteractionTypeComboBox.SelectedIndex = (int)selected.Type;
            RelationshipComboBox.SelectedIndex = person is not null ? (int)person.Relationship : 0;
            SituationComboBox.Text = situation?.Title ?? "";

            // Make form read-only when viewing a past interaction (to avoid accidental edits)
            SetFormReadonly(true);
        }

        private void Cancel_Click (object sender, RoutedEventArgs e) {
            Close();
        }

        private async void Save_Click (object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(PersonTextBox.Text)
             || string.IsNullOrWhiteSpace(CompanyTextBox.Text)
             || string.IsNullOrWhiteSpace(NotesTextBox.Text)) {
                MessageBox.Show("Please fill out all required fields (Person, Company, and Notes).", "Incomplete",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Resolve or create Person
            var personName = PersonTextBox.Text.Trim();
            var person = people.FirstOrDefault(p => p.Name.Equals(personName, StringComparison.OrdinalIgnoreCase));
            if (person == null) {
                person = new Person { Name = personName };
                // Set relationship from UI selection
                person.Relationship = (RelationshipType)RelationshipComboBox.SelectedIndex;
                people.Add(person);
            }
            else {
                // Update relationship in case it changed
                person.Relationship = (RelationshipType)RelationshipComboBox.SelectedIndex;
            }

            // Resolve or create Company
            var companyName = CompanyTextBox.Text.Trim();
            var company = companies.FirstOrDefault(c => c.Name.Equals(companyName, StringComparison.OrdinalIgnoreCase));
            if (company == null) {
                company = new Company { Name = companyName };
                companies.Add(company);
            }

            // Ensure the person’s CompanyId is set (if person is new or changed company)
            person.CompanyId = company.Id;

            // Resolve or create Situation
            Situation situation = null!;
            var situationTitle = SituationComboBox.Text.Trim();
            if (!string.IsNullOrEmpty(situationTitle)) {
                situation = situations.FirstOrDefault(s => s.Title.Equals(situationTitle, StringComparison.OrdinalIgnoreCase));
                if (situation == null) {
                    situation = new Situation { Title = situationTitle, Status = SituationStatus.Ongoing };
                    situations.Add(situation);
                }
            }
            else {
                // If no situation title provided, use a default "AdHoc" situation
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

            // Persist everything (new or updated Person, Company, Situation, plus new Interaction)
            try {
                await SecureProfileManager.SaveProfileAsync(
                    SessionManager.CurrentUserName,
                    SessionManager.CurrentPassword,
                    SessionManager.Data!,
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
                    CancellationToken.None
                );
                MessageBox.Show("Correspondence saved successfully.", "Saved",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to save data:\n{ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            // Pre-fill fields for reply: same person & company, same type, same situation, opposite direction
            PersonTextBox.Text = person.Name;
            CompanyTextBox.Text = company.Name;
            RelationshipComboBox.SelectedIndex = (int)person.Relationship;
            DirectionComboBox.SelectedIndex = selected.Direction == InteractionDirection.Incoming
                                              ? (int)InteractionDirection.Outgoing
                                              : (int)InteractionDirection.Incoming;
            InteractionTypeComboBox.SelectedIndex = (int)selected.Type;
            SituationComboBox.Text = situation?.Title ?? "";
            NotesTextBox.Text = "";
            // Make the form editable for new entry
            SetFormReadonly(false);
            // Clear any selection in history to avoid confusion
            HistoryList.SelectedItem = null;
        }
    }
}
