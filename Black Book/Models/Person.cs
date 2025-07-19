/////
/// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
/// h t t p s : / / i n c o r r i g o . i o /
////
/// Personal Contact Management

using BlackBook.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BlackBook.Models; 
    public partial class Person {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Guid? CompanyId { get; set; }
        public RelationshipType Relationship { get; set; } = RelationshipType.Unknown;
        public string Position { get; set; }
        public string Notes { get; set; }
    }

    public enum RelationshipType {
        Unknown,
        Friendly,
        Neutral,
        Adversarial
    }

public partial class Person : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged ([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public void NotifyListsChanged () {
        OnPropertyChanged(nameof(Interactions));
        OnPropertyChanged(nameof(Situations));
        OnPropertyChanged(nameof(Company));
    }
}
