// Converters/SituationStatusConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using BlackBook.Models;

namespace BlackBook.Converters;

/// <summary>Converts a SituationStatus enum to a user‑friendly string.</summary>
public class SituationStatusConverter : IValueConverter {
    public object Convert (object value, Type targetType,
                          object parameter, CultureInfo culture) {
        if (value is not SituationStatus status)
            return value?.ToString() ?? string.Empty;

        return status switch {
            SituationStatus.AdHoc => "Ad Hoc",
            SituationStatus.New => "New",
            SituationStatus.Ongoing => "Ongoing",
            SituationStatus.DoneWith => "Done With",
            _ => status.ToString()
        };
    }

    public object ConvertBack (object value, Type targetType,
                              object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
