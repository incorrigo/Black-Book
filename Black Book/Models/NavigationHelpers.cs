// Models/NavigationHelpers.cs
using System.Collections.ObjectModel;
using System.Linq;
using BlackBook;

namespace BlackBook.Models {
    /// <summary>Extra read‑only properties needed only for data‑binding.</summary>
    public partial class Person {
        public ObservableCollection<Situation> Situations =>
            new(SessionManager.Data?.Situations
                    .Where(s => SessionManager.Data!.Interactions
                                        .Any(i => i.PersonId == Id && i.SituationId == s.Id))
                ?? []);

        public Company? Company =>
            SessionManager.Data?.Companies.FirstOrDefault(c => c.Id == CompanyId);
    }

    public partial class Situation {
        public ObservableCollection<Interaction> Interactions =>
            new(SessionManager.Data?.Interactions
                    .Where(i => i.SituationId == Id) ?? []);
    }

    public partial class Interaction {
        public Person? Person =>
            SessionManager.Data?.People.FirstOrDefault(p => p.Id == PersonId);

        public Company? Company =>
            SessionManager.Data?.Companies.FirstOrDefault(c => c.Id == CompanyId);
    }
}
