// Models/Person.cs
using System;

namespace BlackBook.Models {
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
}
