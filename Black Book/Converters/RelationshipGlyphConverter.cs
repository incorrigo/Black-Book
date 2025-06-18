using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace BlackBook.Converters;
    /// <summary>Turns a relationship string (“Family”, “Client”…)
    /// into a little MDL2/Segoe Fluent glyph.</summary>
    public sealed class RelationshipGlyphConverter : IValueConverter {
        public object Convert (object value, Type targetType,
                              object parameter, CultureInfo culture) {
            // Pick any glyphs you like – these are MDL2 IDs
            return (value as string)?.ToLowerInvariant() switch {
                "family" => "\uE716",   // Contact heart
                "friend" => "\uE8D4",   // Heart
                "client" => "\uEC50",   // Briefcase
                "supplier" => "\uE14F",   // Shop
                _ => "\uE77B"   // Contact
            };
        }

        public object ConvertBack (object value, Type t, object p, CultureInfo c)
            => throw new NotSupportedException();
    }
