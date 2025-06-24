using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using BlackBook.Models;
using BlackBook.Security;
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

    private void LoadSituations () {
        _situations = SessionManager.Data!.Situations;
        SituationList.ItemsSource = _situations;
    }

    private bool ValidateForm () {
        if (string.IsNullOrWhiteSpace(TitleBox.Text)) {
            MessageBox.Show("Title cannot be empty.", "Invalid",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }

    private async Task SaveSituationsAsync () {
        // Persist the entire container back to disk
        await SecureProfileManager.SaveProfileAsync(
            SessionManager.CurrentUserName,
            SessionManager.CurrentPassword,
            SessionManager.Data!,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
            CancellationToken.None
        );
    }

    private void SituationList_SelectionChanged (object sender, SelectionChangedEventArgs e) {
        var s = SituationList.SelectedItem as Situation;
        if (s == null) {
            TitleBox.Text = "";
            DescriptionBox.Text = "";
            StatusComboBox.SelectedIndex = 0;
            _current = null;
            return;
        }

        _current = s;
        TitleBox.Text = s.Title;
        DescriptionBox.Text = s.Description;
        StatusComboBox.SelectedItem = s.Status;
    }

    private async void Save_Click (object sender, RoutedEventArgs e) {
        if (!ValidateForm()) return;

        if (_current == null) {
            _current = new Situation();
            _situations.Add(_current);
        }

        _current.Title = TitleBox.Text.Trim();
        _current.Description = DescriptionBox.Text.Trim();
        _current.Status = (SituationStatus)StatusComboBox.SelectedItem!;

        if (_current.Status == SituationStatus.DoneWith && _current.Closed == null)
            _current.Closed = DateTime.UtcNow;
        else if (_current.Status != SituationStatus.DoneWith)
            _current.Closed = null;

        SituationList.Items.Refresh();

        try {
            await SaveSituationsAsync();
            MessageBox.Show("Saved.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to save situations: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click (object sender, RoutedEventArgs e) => Close();
}
