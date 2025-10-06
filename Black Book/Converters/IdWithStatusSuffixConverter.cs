using System;
using System.Globalization;
using System.Windows.Data;

namespace BlackBook.Converters;

public class IdWithStatusSuffixConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var id = values.Length > 0 && values[0] != null ? values[0].ToString() : string.Empty;
        var status = values.Length > 1 ? values[1] : null;

        var suffix = status?.ToString() switch
        {
            nameof(ObjectiveStatus.Overdue) => " [late]",
            nameof(ObjectiveStatus.FollowUpExpired) => " [follow up]",
            _ => string.Empty
        };

        return string.IsNullOrEmpty(id) ? string.Empty : id + suffix;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
