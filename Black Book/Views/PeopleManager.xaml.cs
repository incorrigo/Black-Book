using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BlackBook.Models;
using BlackBook.Security;
using BlackBook;

namespace BlackBook.Views {
    public partial class PeopleManager : Window {
        public PeopleManager () {
            InitializeComponent();
            LoadCompanies();
            LoadSituations();
            RefreshPeopleList();
        }

        private void LoadCompanies () {
            CompanyBox.ItemsSource = SessionManager.Data.Companies;
            NewIntCompanyBox.ItemsSource = SessionManager.Data.Companies;
        }

        private void LoadSituations () {
            NewIntSituationBox.ItemsSource = SessionManager.Data.Situations;
        }

        private void RefreshPeopleList (string filter = "") {
            var list = string.IsNullOrWhiteSpace(filter)
                ? SessionManager.Data.People
                : SessionManager.Data.People.Where(p => p.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            PeopleList.ItemsSource = list;
        }

        private void SearchBox_KeyUp (object sender, System.Windows.Input.KeyEventArgs e) {
            RefreshPeopleList(SearchBox.Text.Trim());
        }

        private void PeopleList_SelectionChanged (object sender, SelectionChangedEventArgs e) {
            if (PeopleList.SelectedItem is not Person p) return;
            NameBox.Text = p.Name;
            EmailBox.Text = p.Email;
            PhoneBox.Text = p.PhoneNumber;
            RelationshipBox.SelectedIndex = (int)p.Relationship;
            CompanyBox.SelectedValue = p.CompanyId;

            var ints = SessionManager.Data.Interactions
                        .Where(i => i.PersonId == p.Id)
                        .OrderByDescending(i => i.Timestamp)
                        .ToList();
            InteractionList.ItemsSource = ints;
        }

        private async void SavePerson_Click (object sender, RoutedEventArgs e) {
            Person p;
            if (PeopleList.SelectedItem is Person existing) {
                p = existing;
            }
            else {
                p = new Person();
                SessionManager.Data.People.Add(p);
            }
            p.Name = NameBox.Text.Trim();
            p.Email = EmailBox.Text.Trim();
            p.PhoneNumber = PhoneBox.Text.Trim();
            p.Relationship = (RelationshipType)RelationshipBox.SelectedIndex;
            p.CompanyId = CompanyBox.SelectedValue as string;

            await PersistAsync();
            RefreshPeopleList(SearchBox.Text.Trim());
            PeopleList.SelectedValue = p.Id;
        }

        private async void SaveInteraction_Click (object sender, RoutedEventArgs e) {
            if (PeopleList.SelectedItem is not Person p) return;
            var newInt = new Interaction {
                PersonId = p.Id,
                CompanyId = NewIntCompanyBox.SelectedValue as string,
                SituationId = NewIntSituationBox.SelectedValue as string,
                Direction = (InteractionDirection)NewIntDirectionBox.SelectedIndex,
                Type = (InteractionType)NewIntTypeBox.SelectedIndex,
                Notes = NewIntNotesBox.Text.Trim(),
                Timestamp = DateTime.UtcNow
            };
            SessionManager.Data.Interactions.Add(newInt);
            await PersistAsync();
            PeopleList_SelectionChanged(null, null);
        }

        private async Task PersistAsync () {
            try {
                var usersRoot = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users");
                await SecureProfileManager.SaveProfileAsync(
                    SessionManager.CurrentUserName,
                    SessionManager.CurrentPassword,
                    SessionManager.Data,
                    usersRoot,
                    default);
            }
            catch (Exception ex) {
                MessageBox.Show($"Save failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click (object sender, RoutedEventArgs e) => Close();
    }
}
