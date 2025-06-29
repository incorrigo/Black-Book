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
                InteractionDirection.Incoming => new SolidColorBrush(Color.FromRgb(140, 180, 255)),   // bluish for Incoming
                InteractionDirection.Outgoing => new SolidColorBrush(Color.FromRgb(255, 160, 160)),   // soft red for Outgoing
                InteractionDirection.Mutual => new SolidColorBrush(Color.FromRgb(200, 180, 255)),   // purple-ish for Mutual
                _ => Brushes.Gray
            };
        }
        return Brushes.Gray;
    }

    public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
