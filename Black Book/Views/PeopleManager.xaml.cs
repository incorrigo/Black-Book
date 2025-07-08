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

namespace BlackBook.Views;

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
            var confirm = MessageBox.Show($"Delete {person.Name}\r\n" +
                $"\r\nand all related correspondence?",
                                          "Black Book", MessageBoxButton.YesNo, MessageBoxImage.Warning);
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
            MessageBox.Show($"{person.Name} has been deleted", "Black Book", MessageBoxButton.OK, MessageBoxImage.None);
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

    private void InteractionList_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
        if (InteractionList.SelectedItem is Interaction interaction) {
            var reader = new InteractionReader(interaction, SessionManager.Data!);
            reader.Owner = Window.GetWindow(this);
            reader.ShowDialog();
        }
    }

    private void InteractionList_PreviewMouseWheel (object sender, MouseWheelEventArgs e) {
        if (!e.Handled) {
            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };
            ((Control)sender).RaiseEvent(eventArg);
        }
    }


}
