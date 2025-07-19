using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using BlackBook.Storage;

namespace BlackBook.Converters;

public class SituationIdToNameConverter : IValueConverter {
    public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is Guid id && SessionManager.Data != null) {
            var situation = SessionManager.Data.Situations.FirstOrDefault(s => s.Id == id);
            return situation?.Title ?? "[Unknown Situation]";
        }
        return "[Unknown Situation]";
    }

    public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
