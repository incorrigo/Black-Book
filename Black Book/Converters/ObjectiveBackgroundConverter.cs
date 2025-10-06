using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BlackBook.Models;

namespace BlackBook.Converters {
    public class ObjectiveBackgroundConverter : IValueConverter {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is not Objective o) return null;

            var today = DateTime.Today;

            // Grey background for cancelled objectives
            if (o.Importance == Objective.Priority.Cancelled) {
                return Brushes.LightGray;
            }

            // Red background if deadline elapsed and not Cancelled/Done
            if (o.DueDate is DateTime due && due.Date < today && o.Importance != Objective.Priority.Done) {
                return Brushes.IndianRed;
            }

            // Dark orange if waiting or delegated and follow-up elapsed
            if ((o.Waiting || o.Importance == Objective.Priority.Delegated)
                && o.FollowUp is DateTime fu && fu.Date < today) {
                return Brushes.DarkOrange;
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}