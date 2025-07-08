// InteractionDirectionFriendlyConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using BlackBook.Models;

namespace BlackBook.Converters;

public class InteractionDirectionFriendlyConverter : IValueConverter {
    public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
        return value is InteractionDirection direction ? direction switch {
            InteractionDirection.Incoming => "from",
            InteractionDirection.Outgoing => "to",
            InteractionDirection.Mutual => "with",
            _ => direction.ToString()
        } : "[Unknown]";
    }

    public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
