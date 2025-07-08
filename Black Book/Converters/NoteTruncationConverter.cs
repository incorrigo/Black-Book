using System;
using System.Globalization;
using System.Windows.Data;

namespace BlackBook.Converters;

public class NoteTruncationConverter : IValueConverter {
    private const int MaxChars = 110; // not too generous, but enough for a short note

    public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
        var note = value as string ?? string.Empty;
        if (note.Length <= MaxChars) {
            return note;
        }
        return note.Substring(0, MaxChars).TrimEnd() + "..."; // 3 dots for &c.
    }

    public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
        return value; // not needed, one-way only
    }
}
