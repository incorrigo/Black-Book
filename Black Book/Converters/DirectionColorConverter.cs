using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BlackBook.Models;

namespace BlackBook.Converters;

public class DirectionColorConverter : IValueConverter {
    public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
        return value is InteractionDirection dir && dir == InteractionDirection.Incoming
            ? new SolidColorBrush(Color.FromRgb(140, 180, 255)) // bluish for Incoming
            : new SolidColorBrush(Color.FromRgb(255, 160, 160)); // soft red for Outgoing
    }

    public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
