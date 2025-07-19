// CorrespondenceEditor.xaml.cs
using BlackBook.Models;
using BlackBook.Security;
using BlackBook.Storage;
using System;
using System.Linq;
using System.Windows;

namespace BlackBook.Views;

public partial class CorrespondenceEditor : Window {
    public string SituationTitle { get; set; }
    public InteractionType InteractionType { get; set; }
    public InteractionDirection InteractionDirection { get; set; }
    public string PersonName { get; set; }
    public string Notes { get; set; }
    public DateTime Timestamp { get; set; }  // Ensure this property is set
    public string InteractionGuid => _interaction.Id.ToString();

    private readonly Interaction _interaction;
    private readonly BlackBookContainer _data;

    public CorrespondenceEditor (Interaction interaction, BlackBookContainer data) {
        InitializeComponent();

        _interaction = interaction;
        _data = data;

        SituationTitle = data.Situations.FirstOrDefault(s => s.Id == interaction.SituationId)?.Title ?? "[No Situation]";
        InteractionType = interaction.Type;
        InteractionDirection = interaction.Direction;
        PersonName = data.People.FirstOrDefault(p => p.Id == interaction.PersonId)?.Name ?? "[No Person]";
        Notes = interaction.Notes;
        Timestamp = interaction.Timestamp;  // Explicitly set timestamp

        DataContext = this;

        Loaded += (sender, e) => {
            MinWidth = ActualWidth;
            MinHeight = ActualHeight;
        };
    }

    private void Cancel_Click (object sender, RoutedEventArgs e) => DialogResult = false;

    private async void Save_Click (object sender, RoutedEventArgs e) {
        _interaction.Notes = Notes.Trim();

        try {
            await SecureProfileManager.SaveProfileAsync(
                SessionManager.CurrentUserName,
                SessionManager.CurrentPassword,
                _data,
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users"));
        }
        catch (Exception ex) {
            MessageBox.Show($"Failed to save changes:\n{ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        MessageBox.Show("Your updated information has been filed",
                        "Black Book", MessageBoxButton.OK, MessageBoxImage.Information);
        DialogResult = true;
    }
}
