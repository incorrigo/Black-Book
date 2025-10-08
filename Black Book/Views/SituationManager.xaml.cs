// SituationManager.xaml.cs
using BlackBook.Models;
using BlackBook.Security;
using BlackBook.Storage;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace BlackBook.Views;

public partial class SituationManager : UserControl {
    public SituationManager () {
        InitializeComponent();
        DataContext = SessionManager.Data;

        // Filter out archived situations from the grouped view
        var cvs = (CollectionViewSource)Resources["GroupedSituations"];
        cvs.Filter += GroupedSituations_Filter;
    }

    private void GroupedSituations_Filter(object sender, FilterEventArgs e) {
        if (e.Item is Situation s) {
            e.Accepted = !s.IsArchived;
        }
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

    public void RefreshGrouping () {
        CollectionViewSource.GetDefaultView(SituationsList.ItemsSource).Refresh();
    }


    private async void DeleteSituation_Click (object sender, RoutedEventArgs e) {
        if (SituationsList.SelectedItem is Situation situation) {
            // Confirm deletion, warn if correspondence exists
            var relatedInteractions = SessionManager.Data!.Interactions.Where(i => i.SituationId == situation.Id).ToList();
            if (relatedInteractions.Any()) {
                var confirm = MessageBox.Show(
                    $"You have {relatedInteractions.Count} item(s) of correspondence in {situation.Title}\r\n" +
                    $"\r\nPermanently delete this case file?",
                    "Black Book", MessageBoxButton.YesNo, MessageBoxImage.None);
                if (confirm != MessageBoxResult.Yes) return;
            }
            else {
                var confirm = MessageBox.Show($"Situation file: {situation.Title}\r\n" +
                                              $"\r\nDo you want to delete this situation?",
                                              "Delete Situation", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
            MessageBox.Show("Situation file has been deleted", "Black Book", MessageBoxButton.OK, MessageBoxImage.None);
        }
    }

    private void EditCorrespondence_Click (object sender, RoutedEventArgs e) {
        if (HistoryList.SelectedItem is Interaction selectedInteraction) {
            var window = new CorrespondenceEditor(selectedInteraction, SessionManager.Data!);
            window.Owner = Window.GetWindow(this);
            bool? result = window.ShowDialog();

            if (result == true) {
                selectedInteraction.Person?.NotifyListsChanged();

                if (selectedInteraction.SituationId.HasValue) {
                    var situation = SessionManager.Data.Situations.FirstOrDefault(s => s.Id == selectedInteraction.SituationId.Value);
                    situation?.NotifyListsChanged();
                }

                CollectionViewSource.GetDefaultView(HistoryList.ItemsSource).Refresh();
            }
        }
    }



    private async void DeleteCorrespondence_Click (object sender, RoutedEventArgs e) {
        if (HistoryList.SelectedItem is Interaction interaction) {
            var confirm = MessageBox.Show(
                "There ain't no recycle bin goin on here\r\n" +
                "\r\nDo you want to delete this entry?",
                "Black Book",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            SessionManager.Data!.Interactions.Remove(interaction);
            interaction.Person?.NotifyListsChanged();

            if (interaction.SituationId.HasValue) {
                var situation = SessionManager.Data.Situations.FirstOrDefault(s => s.Id == interaction.SituationId.Value);
                situation?.NotifyListsChanged();
            }

            try {
                await SecureProfileManager.SaveProfileAsync(SessionManager.CurrentUserName,
                                                            SessionManager.CurrentPassword,
                                                            SessionManager.Data,
                                                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"));
            }
            catch (Exception ex) {
                MessageBox.Show($"Failed to delete correspondence:\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            CollectionViewSource.GetDefaultView(HistoryList.ItemsSource).Refresh();
            MessageBox.Show("This record has been deleted",
                            "Black Book", MessageBoxButton.OK, MessageBoxImage.None);
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

    private async void ArchiveSituation_Click(object sender, RoutedEventArgs e) {
        if (SituationsList.SelectedItem is Situation situation) {
            situation.IsArchived = true;
            try {
                await SecureProfileManager.SaveProfileAsync(SessionManager.CurrentUserName, SessionManager.CurrentPassword,
                                                            SessionManager.Data!,
                                                            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
                                                            System.Threading.CancellationToken.None);
            }
            catch (System.Exception ex) {
                MessageBox.Show($"Failed to save data:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            RefreshGrouping();
        }
    }

    private void OpenSituationArchive_Click(object sender, RoutedEventArgs e) {
        var dlg = new ArchiveDialog(ArchiveDialog.ArchiveKind.Situations) { Owner = Window.GetWindow(this) };
        dlg.ShowDialog();
        RefreshGrouping();
    }

    private void PeopleListInSituation_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
        if (PeopleListInSituation.SelectedItem is Person person) {
            var mainWin = (MainWindow)Window.GetWindow(this);
            mainWin.PeopleManagerView.SelectPerson(person);
            mainWin.MainTabControl.SelectedIndex = 0;
        }
    }

    private void HistoryList_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
        if (HistoryList.SelectedItem is Interaction interaction) {
            var reader = new InteractionReader(interaction, SessionManager.Data!);
            reader.Owner = Window.GetWindow(this);
            reader.ShowDialog();
        }
    }

}

public static class Extensions {
    public static void Let<T> (this T? item, Action<T> action) where T : struct {
        if (item.HasValue) action(item.Value);
    }
}

