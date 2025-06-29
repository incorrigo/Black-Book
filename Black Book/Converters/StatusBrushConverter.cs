using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BlackBook.Models;

namespace BlackBook.Converters;

/// <summary>Maps a <see cref="SituationStatus"/> to a coloured brush for UI dots.</summary>
public sealed class StatusBrushConverter : IValueConverter {
    public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is not SituationStatus s) return Brushes.Gray;

        return s switch {
            SituationStatus.AdHoc => new SolidColorBrush(Color.FromRgb(180, 180, 180)), // grey
            SituationStatus.New => new SolidColorBrush(Color.FromRgb(70, 195, 255)), // sky‑blue
            SituationStatus.Ongoing => new SolidColorBrush(Color.FromRgb(255, 190, 70)), // amber
            SituationStatus.DoneWith => new SolidColorBrush(Color.FromRgb(120, 200, 120)), // green
            _ => Brushes.Gray
        };
    }

    public object ConvertBack (object value, Type t, object p, CultureInfo c)
        => throw new NotSupportedException();
}
