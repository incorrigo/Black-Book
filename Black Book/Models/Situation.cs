using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BlackBook.Models;

public partial class Situation {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; }
    public string Description { get; set; }
    public SituationStatus Status { get; set; } = SituationStatus.AdHoc;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime? Closed { get; set; }
}

public enum SituationStatus {
    New,        // These status settings
    Ongoing,    // are ordered to make
    AdHoc,      // the colour grouping
    DoneWith    // in situation manager
}

// This is for the "correspondence doesn't update correspondence list" problem
public partial class Situation : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged ([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    /// <summary>Call after adding / removing interactions so the UI refreshes.</summary>
    public void NotifyListsChanged () {
        OnPropertyChanged(nameof(Interactions));
        OnPropertyChanged(nameof(People));         // both depend on Interactions
    }
}