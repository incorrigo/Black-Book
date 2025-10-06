using System;
using System.Globalization;
using System.Windows.Data;
using BlackBook.Models;

namespace BlackBook.Converters;

public enum ObjectiveStatus
{
    Normal,
    Overdue,
    FollowUpExpired,
    Cancelled
}

public class ObjectiveStatusConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Objective o) return ObjectiveStatus.Normal;

        var today = DateTime.Today;

        if (o.Importance == Objective.Priority.Cancelled)
            return ObjectiveStatus.Cancelled;

        if (o.DueDate is DateTime due && due.Date < today && o.Importance != Objective.Priority.Done)
            return ObjectiveStatus.Overdue;

        if ((o.Waiting || o.Importance == Objective.Priority.Delegated)
            && o.FollowUp is DateTime fu && fu.Date < today)
            return ObjectiveStatus.FollowUpExpired;

        return ObjectiveStatus.Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
