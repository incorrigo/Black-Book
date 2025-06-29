// PeopleManager.xaml.cs
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlackBook.Models;
using BlackBook.Security;
using BlackBook.Storage;

namespace BlackBook.Views {
    public partial class PeopleManager : UserControl {
        public PeopleManager () {
            InitializeComponent();
            DataContext = SessionManager.Data;
        }

        public void SelectPerson (Person person) {
            PeopleList.SelectedItem = person;
            if (PeopleList.SelectedItem != null) {
                PeopleList.ScrollIntoView(PeopleList.SelectedItem);
            }
        }

        private void PeopleList_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
            if (PeopleList.SelectedItem is Person person) {
                var personWindow = new PersonEntryWindow(person);
                personWindow.Owner = Window.GetWindow(this);
                bool? result = personWindow.ShowDialog();
                if (result == true) {
                    // Refresh list and detail to show updated info
                    PeopleList.Items.Refresh();
                    int idx = PeopleList.SelectedIndex;
                    PeopleList.SelectedIndex = -1;
                    PeopleList.SelectedIndex = idx;
                }
            }
        }

        private void EditPerson_Click (object sender, RoutedEventArgs e) {
            PeopleList_MouseDoubleClick(sender, null!);
        }

        private async void DeletePerson_Click (object sender, RoutedEventArgs e) {
            if (PeopleList.SelectedItem is Person person) {
                var confirm = MessageBox.Show($"Are you sure you want to delete {person.Name} and all related correspondence?",
                                              "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) return;
                // Remove all interactions involving this person
                var toRemove = SessionManager.Data!.Interactions.Where(i => i.PersonId == person.Id).ToList();
                foreach (var inter in toRemove) {
                    SessionManager.Data.Interactions.Remove(inter);
                }
                // Remove person
                SessionManager.Data.People.Remove(person);
                try {
                    await SecureProfileManager.SaveProfileAsync(SessionManager.CurrentUserName, SessionManager.CurrentPassword,
                                                                SessionManager.Data,
                                                                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
                                                                CancellationToken.None);
                }
                catch (System.Exception ex) {
                    MessageBox.Show($"Failed to save data:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                MessageBox.Show("Person deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                // Clear selection
                PeopleList.SelectedItem = null;
            }
        }

        private void AddCorrespondence_Click (object sender, RoutedEventArgs e) {
            if (PeopleList.SelectedItem is Person person) {
                var window = new CorrespondenceEntryWindow();
                window.Owner = Window.GetWindow(this);
                window.PrefillPerson(person);
                window.ShowDialog();
            }
        }
    }
}
