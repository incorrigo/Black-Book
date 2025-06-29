// Converters/NullToVisibilityConverter.cs (new)
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BlackBook.Converters;

/// <summary>Converts a null value to Collapsed, non-null to Visible (for UI element visibility).</summary>
public class NullToVisibilityConverter : IValueConverter {
    public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
        return value is null ? Visibility.Collapsed : Visibility.Visible;
    }
    public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
