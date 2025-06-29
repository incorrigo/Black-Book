// Models/NavigationHelper.cs
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;   // ← add
using BlackBook;

namespace BlackBook.Models {
    /* ----------  Person  ---------- */
    public partial class Person {
        [JsonIgnore]                           // ← add
        public ObservableCollection<Situation> Situations =>
            new(SessionManager.Data?.Situations
                    .Where(s => SessionManager.Data!.Interactions
                                     .Any(i => i.PersonId == Id && i.SituationId == s.Id))
                 ?? Enumerable.Empty<Situation>());

        [JsonIgnore]                           // ← add
        public Company? Company =>
            SessionManager.Data?.Companies.FirstOrDefault(c => c.Id == CompanyId);

        [JsonIgnore]
        public ObservableCollection<Interaction> Interactions =>
            new(SessionManager.Data?.Interactions
                    .Where(i => i.PersonId == Id)
                    .OrderByDescending(i => i.Timestamp)
                 ?? Enumerable.Empty<Interaction>());
    }

    /* ----------  Situation  ---------- */
    public partial class Situation {
        [JsonIgnore]
        public ObservableCollection<Interaction> Interactions =>
            new(SessionManager.Data?.Interactions
                    .Where(i => i.SituationId == Id)
                    .OrderByDescending(i => i.Timestamp)
                 ?? Enumerable.Empty<Interaction>());

        [JsonIgnore]
        public ObservableCollection<Person> People =>
            new(SessionManager.Data?.People
                    .Where(p => SessionManager.Data!.Interactions
                                    .Any(i => i.SituationId == Id && i.PersonId == p.Id))
                 ?? Enumerable.Empty<Person>());
    }

    /* ----------  Interaction  ---------- */
    public partial class Interaction {
        [JsonIgnore]
        public Person? Person =>
            SessionManager.Data?.People.FirstOrDefault(p => p.Id == PersonId);

        [JsonIgnore]
        public Company? Company =>
            SessionManager.Data?.Companies.FirstOrDefault(c => c.Id == CompanyId);
    }

    /* ----------  Company  ---------- */
    public partial class Company {
        [JsonIgnore]
        public ObservableCollection<Person> People =>
            new(SessionManager.Data?.People.Where(p => p.CompanyId == Id)
                 ?? Enumerable.Empty<Person>());

        [JsonIgnore]
        public ObservableCollection<Interaction> Interactions =>
            new(SessionManager.Data?.Interactions
                    .Where(i => i.CompanyId == Id)
                    .OrderByDescending(i => i.Timestamp)
                 ?? Enumerable.Empty<Interaction>());
    }
}
