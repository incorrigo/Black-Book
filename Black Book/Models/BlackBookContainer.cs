using System.Collections.Generic;
using BlackBook.Models;

namespace BlackBook.Storage;

public class BlackBookContainer {
    public List<Person> People { get; set; } = new();
    public List<Company> Companies { get; set; } = new();
    public List<Interaction> Interactions { get; set; } = new();
    public List<Situation> Situations { get; set; } = new();
}