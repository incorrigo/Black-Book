// InteractionTypeBrushConverter.cs
using BlackBook.Models;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BlackBook.Converters;

public sealed class InteractionTypeBrushConverter : IValueConverter {
    public object Convert (object value, Type _, object __, CultureInfo ___)
        => value switch {
            InteractionType.Email => new SolidColorBrush(Color.FromRgb(52, 152, 219)),           // Light Blue
            InteractionType.PhoneCall => new SolidColorBrush(Color.FromRgb(0, 0, 255)),          // Blue
            InteractionType.TextMessage => new SolidColorBrush(Color.FromRgb(230, 126, 34)),     // Orange
            InteractionType.InPersonMeeting => new SolidColorBrush(Color.FromRgb(255, 33, 214)), // Pink
            InteractionType.VideoConference => new SolidColorBrush(Color.FromRgb(155, 89, 182)), // Purple
            InteractionType.PostalMail => new SolidColorBrush(Color.FromRgb(0, 0, 0)),           // Black
            InteractionType.SocialMedia => new SolidColorBrush(Color.FromRgb(231, 76, 60)),      // Red
            InteractionType.Other => new SolidColorBrush(Color.FromRgb(173, 173, 173)),          // Grey
            _ => new SolidColorBrush(Colors.Gray)
        };

    public object ConvertBack (object value, Type _, object __, CultureInfo ___)
        => throw new NotSupportedException();
}
