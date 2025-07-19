using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BlackBook.Models;

namespace BlackBook.Converters;

public class PriorityToBrushConverter : IValueConverter {
    public object Convert (object value, Type targetType, object parameter, CultureInfo culture) =>
        (Objective.Priority)value switch {
            Objective.Priority.Easy => Brushes.LightGray,
            Objective.Priority.Medium => Brushes.Green,
            Objective.Priority.Important => Brushes.Orange,
            Objective.Priority.Emergency => Brushes.Red,
            Objective.Priority.Waiting => Brushes.Blue,
            Objective.Priority.Delegated => Brushes.Purple,
            Objective.Priority.Cancelled => Brushes.Gray,
            Objective.Priority.Done => Brushes.Black,
            _ => Brushes.Transparent
        };

    public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
