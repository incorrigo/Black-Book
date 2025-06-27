using System;

namespace BlackBook.Models;

public class Interaction {
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PersonId { get; set; }
    public string CompanyId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public InteractionDirection Direction { get; set; }
    public InteractionType Type { get; set; }
    public string Notes { get; set; }
    public string SituationId { get; set; } // clearly linked to Situation
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
