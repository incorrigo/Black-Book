using BlackBook.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BlackBook.Converters;

public class StatusBrushConverter : IValueConverter {
    public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is SituationStatus status) {
            return status switch {
                SituationStatus.AdHoc => Brushes.Gray,
                SituationStatus.New => Brushes.SteelBlue,
                SituationStatus.Ongoing => Brushes.Orange,
                SituationStatus.DoneWith => Brushes.Green,
                _ => Brushes.Gray
            };
        }
        return Brushes.Gray;
    }
    public object ConvertBack (object value, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}
