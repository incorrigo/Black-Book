using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlackBook.Models;
using BlackBook.Storage;

namespace BlackBook.Views;

public partial class ObjectiveManager : UserControl {
    private BlackBookContainer data;

    public ObjectiveManager () {
        InitializeComponent();
        data = SessionManager.Data!;
        DataContext = data;
    }

    private void ObjectivesList_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
        if (ObjectivesList.SelectedItem is Objective selectedObjective) {
            var editWindow = new ObjectiveEntryWindow(selectedObjective) {
                Owner = Window.GetWindow(this)
            };

            if (editWindow.ShowDialog() == true) {
                ObjectivesList.Items.Refresh(); // Explicitly refresh UI list after editing
            }
        }
    }
}
