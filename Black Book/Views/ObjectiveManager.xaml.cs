using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using BlackBook.Helpers;
using BlackBook.Models;
using BlackBook.Storage;

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
}
