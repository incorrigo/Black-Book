using System;
using System.Linq;
using System.Threading;
using System.Windows;
using BlackBook.Models;
using BlackBook.Security;
using BlackBook.Storage;

namespace BlackBook.Views;

public partial class ObjectiveEntryWindow : Window {
    private readonly BlackBookContainer data;
    private Objective? editingObjective;
    private bool _isSyncingWaitingImportance;

    public ObjectiveEntryWindow (Objective? objectiveToEdit = null) {
        InitializeComponent();
        data = SessionManager.Data!;
        DataContext = data;

        if (objectiveToEdit != null) {
            Title = "Black Book Editor";
            TitleBox.Text = objectiveToEdit.Title;
            SituationComboBox.SelectedItem = data.Situations
                .FirstOrDefault(s => s.Id == objectiveToEdit.SituationId);
            PersonComboBox.SelectedItem = data.People
                .FirstOrDefault(p => p.Id == objectiveToEdit.PersonId);
            ImportanceComboBox.SelectedIndex = (int)objectiveToEdit.Importance;
            WaitingCheckBox.IsChecked = objectiveToEdit.Waiting;
            ReasonBox.Text = objectiveToEdit.ReasonForWait;
            FollowUpDatePicker.SelectedDate = objectiveToEdit.FollowUp;
            DueDatePicker.SelectedDate = objectiveToEdit.DueDate;
            DescBox.Text = objectiveToEdit.Description;
            editingObjective = objectiveToEdit;
        }
    }

    private void WaitingCheckBox_Checked (object sender, RoutedEventArgs e) {
        if (!IsLoaded || ImportanceComboBox == null || ReasonBox == null) return;
        // Auto-select 'Waiting' in the importance drop-down and focus the reason box
        ImportanceComboBox.SelectedIndex = 4; // 'Waiting'
        ReasonBox.Focus();
        // Place caret at end so user can start typing immediately
        ReasonBox.CaretIndex = ReasonBox.Text?.Length ?? 0;
    }

    private void ImportanceComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
        // Ignore selection changes that fire during XAML initialization before all controls exist
        if (_isSyncingWaitingImportance) return;
        if (!IsLoaded || WaitingCheckBox == null || ReasonBox == null) return;
        try {
            _isSyncingWaitingImportance = true;
            var selectedIndex = ImportanceComboBox.SelectedIndex;
            if (selectedIndex == (int)Objective.Priority.Waiting) {
                WaitingCheckBox.IsChecked = true;
                // Focus reason to encourage providing context
                ReasonBox.Focus();
                ReasonBox.CaretIndex = ReasonBox.Text?.Length ?? 0;
            }
            else {
                if (WaitingCheckBox.IsChecked == true) WaitingCheckBox.IsChecked = false;
            }
        }
        finally {
            _isSyncingWaitingImportance = false;
        }
    }

    private void Cancel_Click (object sender, RoutedEventArgs e) {
        DialogResult = false;
        Close();
    }

    private async void Save_Click (object sender, RoutedEventArgs e) {
        if (string.IsNullOrWhiteSpace(TitleBox.Text) || SituationComboBox.SelectedItem == null) {
            MessageBox.Show("An objective must have a title and a situation",
                            "Incomplete", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Objective must have a deadline
        if (DueDatePicker.SelectedDate == null) {
            MessageBox.Show("An objective must have a deadline date",
                            "Objective Entry", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (WaitingCheckBox.IsChecked == true && (string.IsNullOrWhiteSpace(ReasonBox.Text) || FollowUpDatePicker.SelectedDate == null)) {
            MessageBox.Show("Waiting objective must have a reason message and follow-up date",
                            "Waiting Objective", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Enforce delegated/waiting follow-up rules and date relationships
        var importance = (Objective.Priority)ImportanceComboBox.SelectedIndex;
        var dueDate = DueDatePicker.SelectedDate!.Value.Date;
        DateTime? followUp = FollowUpDatePicker.SelectedDate?.Date;

        if (importance == Objective.Priority.Delegated && followUp == null) {
            MessageBox.Show("Delegated objective must have a follow-up date",
                            "Delegated Objective", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if ((importance == Objective.Priority.Delegated || WaitingCheckBox.IsChecked == true) && followUp != null && followUp.Value >= dueDate) {
            MessageBox.Show("Follow-up date must be before the deadline. If you need a later follow-up, move the deadline later.",
                            "Follow-Up vs Deadline", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var selectedSituation = (Situation)SituationComboBox.SelectedItem;
        var selectedPerson = PersonComboBox.SelectedItem as Person;

        if (editingObjective != null) {
            // Objective modification
            editingObjective.Title = TitleBox.Text.Trim();
            editingObjective.SituationId = selectedSituation.Id;
            editingObjective.PersonId = selectedPerson?.Id;
            editingObjective.Importance = (Objective.Priority)ImportanceComboBox.SelectedIndex;
            editingObjective.Waiting = WaitingCheckBox.IsChecked == true;
            editingObjective.ReasonForWait = ReasonBox.Text.Trim();
            editingObjective.FollowUp = FollowUpDatePicker.SelectedDate;
            editingObjective.DueDate = DueDatePicker.SelectedDate;
            editingObjective.Description = DescBox.Text.Trim();
        }
        else {
            // Create new objective
            var objective = new Objective {
                Title = TitleBox.Text.Trim(),
                SituationId = selectedSituation.Id,
                PersonId = selectedPerson?.Id,
                Importance = (Objective.Priority)ImportanceComboBox.SelectedIndex,
                Waiting = WaitingCheckBox.IsChecked == true,
                ReasonForWait = ReasonBox.Text.Trim(),
                FollowUp = FollowUpDatePicker.SelectedDate,
                DueDate = DueDatePicker.SelectedDate,
                Description = DescBox.Text.Trim(),
                Created = DateTime.UtcNow
            };
            data.Objectives.Add(objective);
        }

        try {
            await SecureProfileManager.SaveProfileAsync(
                SessionManager.CurrentUserName, SessionManager.CurrentPassword, data,
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"),
                CancellationToken.None);
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to save objective:\r\n\r\n{ex.Message}", "Exception",
                            MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        MessageBox.Show(editingObjective != null
                        ? "Objective amendments have been filed"
                        : "Your new objective has been filed",
                        "Black Book", MessageBoxButton.OK, MessageBoxImage.None);

        DialogResult = true;
        Close();
    }

    public void PrefillSituation (Situation situation) {
        SituationComboBox.SelectedItem = situation;
    }

    public void PrefillPerson (Person person) {
        PersonComboBox.SelectedItem = person;
    }
}
