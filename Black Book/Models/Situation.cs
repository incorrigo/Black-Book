/////
/// INCORRIGO SYX DIGITAL COMMUNICATION SYSTEMS
/// h t t p s : / / i n c o r r i g o . i o /
////
/// Situation Management Solutions

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
    New,        // This just in ...
    Ongoing,    // still going on
    AdHoc,      // just a defacto situation
    DoneWith    //,F.R.O. (fuck right off)
                //`--> consigned to history 
}

// When a situation develops (new correspondence) this will update history lists
// everywhere without you reloading them
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