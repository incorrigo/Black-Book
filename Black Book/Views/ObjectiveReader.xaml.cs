using System;
using System.Text;
using System.Windows;
using BlackBook.Models;

namespace BlackBook.Views;

public partial class ObjectiveReader : Window {
    public ObjectiveReader(Objective objective) {
        InitializeComponent();
        DataContext = new ObjectiveReaderVm(objective);
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private sealed class ObjectiveReaderVm {
        private readonly Objective _o;
        public ObjectiveReaderVm(Objective o) { _o = o; }

        // Basic passthroughs
        public string Title => _o.Title;
        public string Description => _o.Description;
        public Objective.Priority Importance => _o.Importance;
        public Guid SituationId => _o.SituationId;
        public Guid? PersonId => _o.PersonId;
        public Guid Id => _o.Id;
        public DateTime Created => _o.Created;

        public string DeadlineLine {
            get {
                if (_o.DueDate is DateTime d)
                    return $"Deadline: {d:yyyy-MM-dd} [{d:dddd}]";
                return "Deadline: —";
            }
        }

        public string ExistedLine {
            get {
                var end = _o.CompletedOn ?? DateTime.UtcNow;
                return $"Existed: {FormatDuration(_o.Created, end)}";
            }
        }

        private static string FormatDuration(DateTime start, DateTime end) {
            if (end < start) (start, end) = (end, start);
            var totalDays = (end - start).Days;
            var years = totalDays / 365; totalDays %= 365;
            var weeks = totalDays / 7; totalDays %= 7;
            var days = totalDays;
            var sb = new StringBuilder();
            if (years > 0) sb.Append(years).Append(years == 1 ? " year " : " years ");
            if (weeks > 0) sb.Append(weeks).Append(weeks == 1 ? " week " : " weeks ");
            sb.Append(days).Append(days == 1 ? " day" : " days");
            return sb.ToString();
        }
    }
}
