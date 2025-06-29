using System;

namespace BlackBook.Models;

public partial class Interaction {
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PersonId { get; set; }
    public Guid? CompanyId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public InteractionDirection Direction { get; set; }
    public InteractionType Type { get; set; }
    public string Notes { get; set; }
    public Guid? SituationId { get; set; } // clearly linked to Situation
}

public enum InteractionDirection {
    Incoming,
    Outgoing,
    Mutual
}

public enum InteractionType {
    Email,
    PhoneCall,
    TextMessage,
    InPersonMeeting,
    VideoConference,
    PostalMail,
    SocialMedia,
    Other
}
