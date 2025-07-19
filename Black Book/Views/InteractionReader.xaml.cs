// InteractionReader.xaml.cs
using BlackBook.Models;
using BlackBook.Storage;
using System.Linq;
using System.Windows;
using System;

namespace BlackBook.Views;

public partial class InteractionReader : Window {
    public string SituationTitle { get; set; }
    public InteractionType InteractionType { get; set; }
    public InteractionDirection InteractionDirection { get; set; }
    public string PersonName { get; set; }
    public string Notes { get; set; }
    public string InteractionGuid { get; set; }
    public DateTime Timestamp { get; set; }  // ← Added this

    public InteractionReader (Interaction interaction, BlackBookContainer data) {
        InitializeComponent();

        SituationTitle = data.Situations.FirstOrDefault(s => s.Id == interaction.SituationId)?.Title ?? "[No Situation]";
        InteractionType = interaction.Type;
        InteractionDirection = interaction.Direction;
        PersonName = data.People.FirstOrDefault(p => p.Id == interaction.PersonId)?.Name ?? "[No Person]";
        Notes = interaction.Notes;
        InteractionGuid = interaction.Id.ToString();
        Timestamp = interaction.Timestamp;  // ← Populated from the interaction

        DataContext = this;

        Loaded += (sender, e) => {
            MinWidth = ActualWidth;
            MinHeight = ActualHeight;
        };
    }

    private void Close_Click (object sender, RoutedEventArgs e) => Close();
}
