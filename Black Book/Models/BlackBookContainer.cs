using BlackBook.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BlackBook.Storage;

public class BlackBookContainer {
    public ObservableCollection<Person>         People { get; set; } = new();
    public ObservableCollection<Company>        Companies { get; set; } = new();
    public ObservableCollection<Interaction>    Interactions { get; set; } = new();
    public ObservableCollection<Situation>      Situations { get; set; } = new();

    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastOpened { get; set; } = DateTime.UtcNow;
    public int AccessCount { get; set; } = 0;
    public string Version { get; set; } = "1.0.0";

    public void Clear () {
        People.Clear();
        Companies.Clear();
        Interactions.Clear();
        Situations.Clear();
    }
}
