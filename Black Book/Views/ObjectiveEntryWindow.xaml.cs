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

    private void Cancel_Click (object sender, RoutedEventArgs e) {
        DialogResult = false;
        Close();
    }

    private async void Save_Click (object sender, RoutedEventArgs e) {
        if (string.IsNullOrWhiteSpace(TitleBox.Text) || SituationComboBox.SelectedItem == null) {
            MessageBox.Show("Enter a title and select a situation.",
                            "Incomplete", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (WaitingCheckBox.IsChecked == true && (string.IsNullOrWhiteSpace(ReasonBox.Text) || FollowUpDatePicker.SelectedDate == null)) {
            MessageBox.Show("You must provide both a reason and a follow-up date if waiting.",
                            "Incomplete", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        ? "Your amendments have been filed"
                        : "Objective information has been filed",
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
