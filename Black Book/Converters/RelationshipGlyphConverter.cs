// Converters/RelationshipGlyphConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using BlackBook.Models;

namespace BlackBook.Converters;

/// <summary>Turns a relationship string or enum (“Family”, “Friendly”, etc) into a small glyph.</summary>
public sealed class RelationshipGlyphConverter : IValueConverter {
    public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
        // Accept either RelationshipType enum or string, and map to a glyph.
        string? rel = value is RelationshipType relType ? relType.ToString()
                                                        : value as string;
        return rel?.ToLowerInvariant() switch {
            "family" => "\uE716",   // Contact Heart icon
            "friend" => "\uE8D4",   // Heart icon
            "friendly" => "\uE8D4",   // (treat "Friendly" same as friend)
            "client" => "\uEC50",   // Briefcase icon
            "supplier" => "\uE14F",   // Shop icon
            _ => "\uE77B"    // Default Contact icon
        };
    }

    public object ConvertBack (object value, Type t, object p, CultureInfo c)
        => throw new NotSupportedException();
}
