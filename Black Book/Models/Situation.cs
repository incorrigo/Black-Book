using System;

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
    AdHoc,
    New,
    Ongoing,
    DoneWith
}
