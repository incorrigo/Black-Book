using System;

namespace BlackBook.Models;

public class Company {
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Address { get; set; }
    public string Description { get; set; }
}
