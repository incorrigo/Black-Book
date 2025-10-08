using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BlackBook.Models;

public class Objective : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged ([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public void NotifyListsChanged () {
        OnPropertyChanged(nameof(RelatedCorrespondence));
    }

    private Guid _id = Guid.NewGuid();
    public Guid Id {
        get => _id;
        set { _id = value; OnPropertyChanged(); }
    }

    private bool _completed = false;
    public bool Completed {
        get => _completed;
        set { _completed = value; OnPropertyChanged(); }
    }

    private bool _isArchived = false;
    public bool IsArchived {
        get => _isArchived;
        set { _isArchived = value; OnPropertyChanged(); }
    }

    private string _title = string.Empty;
    public string Title {
        get => _title;
        set { _title = value; OnPropertyChanged(); }
    }

    private Guid _situationId;
    public Guid SituationId {
        get => _situationId;
        set { _situationId = value; OnPropertyChanged(); }
    }

    private Guid? _personId;
    public Guid? PersonId {
        get => _personId;
        set { _personId = value; OnPropertyChanged(); }
    }

    private string _description = string.Empty;
    public string Description {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    private bool _waiting = false;
    public bool Waiting {
        get => _waiting;
        set { _waiting = value; OnPropertyChanged(); }
    }

    private string _reasonForWait = string.Empty;
    public string ReasonForWait {
        get => _reasonForWait;
        set { _reasonForWait = value; OnPropertyChanged(); }
    }

    private DateTime _created = DateTime.UtcNow;
    public DateTime Created {
        get => _created;
        set { _created = value; OnPropertyChanged(); }
    }

    private DateTime? _dueDate;
    public DateTime? DueDate {
        get => _dueDate;
        set { _dueDate = value; OnPropertyChanged(); }
    }

    private DateTime? _completedOn;
    public DateTime? CompletedOn {
        get => _completedOn;
        set { _completedOn = value; OnPropertyChanged(); }
    }

    private DateTime? _followUp;
    public DateTime? FollowUp {
        get => _followUp;
        set { _followUp = value; OnPropertyChanged(); }
    }

    private Priority _importance = Priority.Easy;
    public Priority Importance {
        get => _importance;
        set { _importance = value; OnPropertyChanged(); }
    }

    public ObservableCollection<Interaction> RelatedCorrespondence { get; set; } = new();

    public enum Priority {
        Easy,       // not the end of the world
        Medium,     // plenty of time
        Important,  // needs doing soon
        Emergency,  // get it done now
        Waiting,    // waiting for something
        Delegated,  // someone else is doing it
        Cancelled,  // no longer needed
        Done        // completed
    }
}
