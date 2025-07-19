// SituationEntryWindow.xaml.cs
using System;
using System.Threading;
using System.Windows;
using BlackBook.Models;
using BlackBook.Security;
using BlackBook.Storage;
using System.IO;

namespace BlackBook.Views;

public partial class SituationEntryWindow : Window {
    private readonly BlackBookContainer data;
    private Situation? editingSituation;

    public SituationEntryWindow (Situation? situationToEdit = null) {
        InitializeComponent();
        data = SessionManager.Data!;
        if (situationToEdit != null) {
            Title = "Edit Situation";
            TitleBox.Text = situationToEdit.Title;
            StatusBox.SelectedIndex = (int)situationToEdit.Status;
            DescBox.Text = situationToEdit.Description;
            editingSituation = situationToEdit;
        }
    }

    private void Cancel_Click (object sender, RoutedEventArgs e) {
        DialogResult = false;
        Close();
    }

    private async void Save_Click (object sender, RoutedEventArgs e) {
        var title = TitleBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(title)) {
            MessageBox.Show("Situation title is required.", "Incomplete",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (title.Length > 37) {
            MessageBox.Show("Situation title cannot exceed 37 characters.",
                            "Title Too Long",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var status = (SituationStatus)StatusBox.SelectedIndex;

        if (editingSituation != null) {
            // Update existing situation
            editingSituation.Title = title;
            var oldStatus = editingSituation.Status;
            editingSituation.Status = status;
            editingSituation.Description = DescBox.Text.Trim();

            if (oldStatus != SituationStatus.DoneWith && status == SituationStatus.DoneWith) {
                editingSituation.Closed = DateTime.UtcNow;
            }
            else if (oldStatus == SituationStatus.DoneWith && status != SituationStatus.DoneWith) {
                editingSituation.Closed = null;
            }
        }
        else {
            // Create new situation
            var situation = new Situation {
                Title = title,
                Status = status,
                Description = DescBox.Text.Trim(),
                Closed = status == SituationStatus.DoneWith ? DateTime.UtcNow : null
            };

            data.Situations.Add(situation);
        }

        try {
            await SecureProfileManager.SaveProfileAsync(
                SessionManager.CurrentUserName,
                SessionManager.CurrentPassword,
                data,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
                CancellationToken.None);
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to save data:\n{ex.Message}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
            return;
        }

        MessageBox.Show(editingSituation != null
                        ? "Situation updated successfully."
                        : "Situation added successfully.",
                        "Saved",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

        DialogResult = true;

        // Refresh situation grouping
        var mainWin = (MainWindow)Owner;
        mainWin.SituationManagerView.RefreshGrouping();

        Close();
    }

}
