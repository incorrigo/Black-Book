using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using BlackBook.Models;
using BlackBook.Storage;
using BlackBook.Security;

namespace BlackBook.Views;

public partial class ArchiveDialog : Window {
    public enum ArchiveKind { Objectives, Situations }

    private readonly ArchiveKind _kind;
    private readonly BlackBookContainer _data;
    private readonly ListCollectionView _view;

    public ArchiveDialog(ArchiveKind kind) {
        InitializeComponent();
        _kind = kind;
        _data = SessionManager.Data!;

        if (_kind == ArchiveKind.Objectives) {
            Title = "Objective archive";
            HeaderText.Text = "Archived objectives";
            var cvs = new CollectionViewSource { Source = _data.Objectives };
            _view = (ListCollectionView)cvs.View;
            _view.Filter = o => o is Objective obj && obj.IsArchived;
            ArchiveList.ItemsSource = _view;
        } else {
            Title = "Situation archive";
            HeaderText.Text = "Archived situations";
            var cvs = new CollectionViewSource { Source = _data.Situations };
            _view = (ListCollectionView)cvs.View;
            _view.Filter = o => o is Situation s && s.IsArchived;
            ArchiveList.ItemsSource = _view;
        }
    }

    private async void RestoreButton_Click(object sender, RoutedEventArgs e) {
        RestoreSelected();
        try {
            await SecureProfileManager.SaveProfileAsync(SessionManager.CurrentUserName, SessionManager.CurrentPassword,
                SessionManager.Data!, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"), CancellationToken.None);
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to save data:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ArchiveList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        if (_kind == ArchiveKind.Objectives && ArchiveList.SelectedItem is Objective o) {
            var viewer = new ObjectiveReader(o) { Owner = this };
            viewer.ShowDialog();
        } else {
            // For situations keep existing behavior: double-click restores
            RestoreSelected();
        }
    }

    private void RestoreSelected() {
        if (_kind == ArchiveKind.Objectives && ArchiveList.SelectedItem is Objective o) {
            o.IsArchived = false;
            _view.Refresh();
        }
        else if (_kind == ArchiveKind.Situations && ArchiveList.SelectedItem is Situation s) {
            s.IsArchived = false;
            _view.Refresh();
        }
    }

    private void ArchiveList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        RestoreButton.IsEnabled = ArchiveList.SelectedItem != null;
    }
}