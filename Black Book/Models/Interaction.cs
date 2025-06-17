namespace BlackBook.Models;

public class Interaction {
    public string Id { get; set; }
    public string PersonId { get; set; }
    public string CompanyId { get; set; }
    public DateTime Timestamp { get; set; }
    public InteractionDirection Direction { get; set; }
    public string Details { get; set; }
}
