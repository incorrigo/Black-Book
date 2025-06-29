// Converters/InteractionTypeFriendlyConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using BlackBook.Models;

namespace BlackBook.Converters {
    public sealed class InteractionTypeFriendlyConverter : IValueConverter {
        public object Convert (object value, Type _, object __, CultureInfo ___)
            => value switch {
                InteractionType.Email => "Email",
                InteractionType.PhoneCall => "Call",
                InteractionType.TextMessage => "SMS",
                InteractionType.InPersonMeeting => "Meeting",
                InteractionType.VideoConference => "Video",
                InteractionType.PostalMail => "Mail",
                InteractionType.SocialMedia => "Social",
                _ => string.Empty
            };

        public object ConvertBack (object value, Type _, object __, CultureInfo ___)
            => throw new NotSupportedException();
    }
}
