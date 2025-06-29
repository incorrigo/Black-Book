// SituationManager.xaml.cs
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
    public partial class SituationManager : UserControl {
        public SituationManager () {
            InitializeComponent();
            DataContext = SessionManager.Data;
        }

        public void SelectSituation (Situation situation) {
            SituationsList.SelectedItem = situation;
            if (SituationsList.SelectedItem != null) {
                SituationsList.ScrollIntoView(SituationsList.SelectedItem);
            }
        }

        private void SituationsList_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
            if (SituationsList.SelectedItem is Situation situation) {
                var window = new SituationEntryWindow(situation);
                window.Owner = Window.GetWindow(this);
                bool? result = window.ShowDialog();
                if (result == true) {
                    SituationsList.Items.Refresh();
                    int idx = SituationsList.SelectedIndex;
                    SituationsList.SelectedIndex = -1;
                    SituationsList.SelectedIndex = idx;
                }
            }
        }

        private void EditSituation_Click (object sender, RoutedEventArgs e) {
            SituationsList_MouseDoubleClick(sender, null!);
        }

        private async void DeleteSituation_Click (object sender, RoutedEventArgs e) {
            if (SituationsList.SelectedItem is Situation situation) {
                // Confirm deletion, warn if correspondence exists
                var relatedInteractions = SessionManager.Data!.Interactions.Where(i => i.SituationId == situation.Id).ToList();
                if (relatedInteractions.Any()) {
                    var confirm = MessageBox.Show(
                        $"Deleting this situation will also delete {relatedInteractions.Count} related correspondence entries. Continue?",
                        "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (confirm != MessageBoxResult.Yes) return;
                }
                else {
                    var confirm = MessageBox.Show("Are you sure you want to delete this situation?",
                                                  "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirm != MessageBoxResult.Yes) return;
                }
                // Remove interactions tied to this situation
                foreach (var inter in relatedInteractions) {
                    SessionManager.Data.Interactions.Remove(inter);
                }
                // Remove situation
                SessionManager.Data.Situations.Remove(situation);
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
                MessageBox.Show("Situation deleted successfully.", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddCorrespondence_Click (object sender, RoutedEventArgs e) {
            if (SituationsList.SelectedItem is Situation situation) {
                var window = new CorrespondenceEntryWindow();
                window.Owner = Window.GetWindow(this);
                window.PrefillSituation(situation);
                window.ShowDialog();
            }
        }

        private void ReplyToSelected_Click (object sender, RoutedEventArgs e) {
            if (HistoryList.SelectedItem is Interaction interaction) {
                // Open correspondence window pre-filled as a response
                var window = new CorrespondenceEntryWindow();
                window.Owner = Window.GetWindow(this);
                window.PrefillForReply(interaction);
                window.ShowDialog();
            }
        }

        private void PeopleListInSituation_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
            if (PeopleListInSituation.SelectedItem is Person person) {
                var mainWin = (MainWindow)Window.GetWindow(this);
                mainWin.PeopleManagerView.SelectPerson(person);
                mainWin.MainTabControl.SelectedIndex = 0;
            }
        }
    }
}
