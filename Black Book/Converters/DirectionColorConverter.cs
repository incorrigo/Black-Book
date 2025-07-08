// Converters/DirectionColorConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BlackBook.Models;

namespace BlackBook.Converters;

public class DirectionColorConverter : IValueConverter {
    public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is InteractionDirection dir) {
            return dir switch {
                InteractionDirection.Incoming => new SolidColorBrush(Color.FromRgb(0, 0, 255)),         // BLUE
                InteractionDirection.Outgoing => new SolidColorBrush(Color.FromRgb(173, 173, 173)),     // GREY
                InteractionDirection.Mutual => new SolidColorBrush(Color.FromRgb(0, 0, 0)),             // BLACK
                _ => Brushes.Gray
            };
        }
        return Brushes.Gray;
    }

    public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
