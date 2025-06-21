using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BlackBook.Models;
using BlackBook.Storage;

namespace BlackBook.Views;

public partial class SituationManager : Window {
    private List<Situation> _situations = new();
    private Situation? _current;

    public SituationManager () {
        InitializeComponent();
        LoadSituations();
        StatusComboBox.ItemsSource = Enum.GetValues(typeof(SituationStatus));
        StatusComboBox.SelectedIndex = 0;
    }

    /* ---------- data load/save ---------- */

    private void LoadSituations () {
        _situations = SessionManager.Data!.Situations;
        SituationList.ItemsSource = _situations;
    }

    private void SaveSituations () {
        var path = UserDirectoryManager.GetEncryptedDataPath(SessionManager.CurrentUserName);
        EncryptedContainerManager.SaveEncrypted(SessionManager.Data!, SessionManager.Certificate!, path);
    }

    /* ---------- UI helpers ---------- */

    private void BindTo (Situation? s) {
        _current = s;
        if (s == null) {
            TitleBox.Text = "";
            DescriptionBox.Text = "";
            StatusComboBox.SelectedIndex = 0;
            return;
        }

        TitleBox.Text = s.Title;
        DescriptionBox.Text = s.Description;
        StatusComboBox.SelectedItem = s.Status;
    }

    private bool ValidateForm () {
        if (string.IsNullOrWhiteSpace(TitleBox.Text)) {
            MessageBox.Show("Title cannot be empty.", "Invalid", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }

    /* ---------- event handlers ---------- */

    private void SituationList_SelectionChanged (object sender, SelectionChangedEventArgs e) =>
        BindTo(SituationList.SelectedItem as Situation);

    private void Save_Click (object sender, RoutedEventArgs e) {
        if (!ValidateForm()) return;

        if (_current == null) {
            _current = new Situation();
            _situations.Add(_current);
        }

        _current.Title = TitleBox.Text.Trim();
        _current.Description = DescriptionBox.Text.Trim();
        _current.Status = (SituationStatus)StatusComboBox.SelectedItem;

        if (_current.Status == SituationStatus.DoneWith && _current.Closed == null)
            _current.Closed = DateTime.UtcNow;
        if (_current.Status != SituationStatus.DoneWith)
            _current.Closed = null;

        SaveSituations();
        SituationList.Items.Refresh();
        MessageBox.Show("Saved.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Cancel_Click (object sender, RoutedEventArgs e) => Close();
}
