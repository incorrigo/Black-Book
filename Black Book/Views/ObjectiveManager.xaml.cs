using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using BlackBook.Helpers;
using BlackBook.Models;
using BlackBook.Storage;
using BlackBook.Security;

namespace BlackBook.Views;

public partial class ObjectiveManager : UserControl {
    private BlackBookContainer data;

    public ObjectiveManager () {
        InitializeComponent();
        data = SessionManager.Data!;
        DataContext = data;

        // Apply custom sort: Importance (custom rank) then DueDate ascending
        var view = (ListCollectionView)CollectionViewSource.GetDefaultView(data.Objectives);
        view.CustomSort = new ObjectiveSortComparer();
        view.Filter = o => o is Objective obj && !obj.IsArchived; // hide archived
    }

    private void ObjectivesList_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
        if (ObjectivesList.SelectedItem is Objective selectedObjective) {
            var editWindow = new ObjectiveEntryWindow(selectedObjective) {
                Owner = Window.GetWindow(this)
            };

            if (editWindow.ShowDialog() == true) {
                // Refresh to re-apply sorting and visuals
                CollectionViewSource.GetDefaultView(data.Objectives)?.Refresh();
                ObjectivesList.Items.Refresh(); // Explicitly refresh UI list after editing
            }
        }
    }

    private async void ArchiveObjective_Click (object sender, RoutedEventArgs e) {
        if (ObjectivesList.SelectedItem is Objective obj) {
            obj.IsArchived = true;
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(data.Objectives);
            view.Refresh();
            try {
                await SecureProfileManager.SaveProfileAsync(SessionManager.CurrentUserName, SessionManager.CurrentPassword,
                    SessionManager.Data!, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"), System.Threading.CancellationToken.None);
            } catch (Exception ex) {
                MessageBox.Show($"Failed to save data:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void OpenObjectiveArchive_Click (object sender, RoutedEventArgs e) {
        var dlg = new ArchiveDialog(ArchiveDialog.ArchiveKind.Objectives) { Owner = Window.GetWindow(this) };
        dlg.ShowDialog();
        // refresh after potential restores
        CollectionViewSource.GetDefaultView(data.Objectives)?.Refresh();
        ObjectivesList.Items.Refresh();
    }
}
