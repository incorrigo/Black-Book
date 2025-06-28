using System;

namespace BlackBook.Models;

public partial class Person {
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string CompanyId { get; set; }
    public RelationshipType Relationship { get; set; } = RelationshipType.Unknown;
}

public enum RelationshipType {
    Unknown,
    Friendly,
    Neutral,
    Adversarial
}
