// Models/Company.cs
using System;

namespace BlackBook.Models {
    public partial class Company {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string Website { get; set; }
    }
}
