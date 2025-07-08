// InteractionReader.xaml.cs
using BlackBook.Models;
using BlackBook.Storage;
using System.Linq;
using System.Windows;

namespace BlackBook.Views;

public partial class InteractionReader : Window {
    public string SituationTitle { get; set; }
    public string InteractionType { get; set; }
    public string InteractionDirection { get; set; }
    public string PersonName { get; set; }
    public string Relationship { get; set; }
    public string Notes { get; set; }
    public string SituationGuid { get; set; }
    public string InteractionGuid { get; set; }

    public InteractionReader (Interaction interaction, BlackBookContainer data) {
        InitializeComponent();

        SituationTitle = data.Situations.FirstOrDefault(s => s.Id == interaction.SituationId)?.Title ?? "[No Situation]";
        InteractionType = interaction.Type.ToString();
        InteractionDirection = interaction.Direction.ToString();
        var person = data.People.FirstOrDefault(p => p.Id == interaction.PersonId);
        PersonName = person?.Name ?? "[No Person]";
        Relationship = person?.Relationship.ToString() ?? "[No Relationship]";
        Notes = interaction.Notes;
        SituationGuid = interaction.SituationId.ToString();
        InteractionGuid = interaction.Id.ToString();

        DataContext = this;
    }

    private void Close_Click (object sender, RoutedEventArgs e) {
        Close();
    }
}
