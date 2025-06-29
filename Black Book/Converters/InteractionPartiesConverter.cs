using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using BlackBook.Models;

namespace BlackBook.Converters;

/// <summary>
/// Returns a “A → B” / “B → A” / “A ⇄ B” label where  
/// *A*  = the currently‑logged‑in profile (SessionManager.CurrentUserName)  
/// *B*  = the counter‑party (person’s name).
/// </summary>
public sealed class InteractionPartiesConverter : IValueConverter {
    public object Convert (object value, Type _, object __, CultureInfo ___) {
        if (value is not Interaction i || SessionManager.Data is null)
            return string.Empty;

        string self = SessionManager.CurrentUserName;                                // “Newton Ridley”
        string other = SessionManager.Data.People
                               .FirstOrDefault(p => p.Id == i.PersonId)?
                               .Name ?? "Unknown";

        return i.Direction switch {
            InteractionDirection.Outgoing => $"{self} → {other}",
            InteractionDirection.Incoming => $"{other} → {self}",
            InteractionDirection.Mutual => $"{self} ⇄ {other}",
            _ => $"{self} — {other}"
        };
    }

    public object ConvertBack (object v, Type t, object p, CultureInfo c)
        => throw new NotSupportedException();
}
