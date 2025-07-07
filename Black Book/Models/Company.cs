// Models/Company.cs
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BlackBook.Models;

public partial class Company {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Address { get; set; }
    public string Description { get; set; }
    public string PhoneNumber { get; set; }
    public string Website { get; set; }
}

public partial class Company : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged ([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public void NotifyListsChanged () {
        OnPropertyChanged(nameof(Interactions));
        OnPropertyChanged(nameof(People));
    }
}
