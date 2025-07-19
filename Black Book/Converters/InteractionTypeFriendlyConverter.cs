// Converters/InteractionTypeFriendlyConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using BlackBook.Models;

namespace BlackBook.Converters;

public sealed class InteractionTypeFriendlyConverter : IValueConverter {
    public object Convert (object value, Type _, object __, CultureInfo ___)
        => value switch {
            InteractionType.Email => "Email",
            InteractionType.PhoneCall => "Phone Call",
            InteractionType.TextMessage => "Text Message",
            InteractionType.InPersonMeeting => "Meeting",
            InteractionType.VideoConference => "Video Call",
            InteractionType.PostalMail => "Letter",
            InteractionType.SocialMedia => "Social Media Post",
            InteractionType.Other => "Other",  // << this line fixes the issue
            _ => "Unknown" // <-- this also handles any unknown/null cases
        };

    public object ConvertBack (object value, Type _, object __, CultureInfo ___)
        => throw new NotSupportedException();
}
