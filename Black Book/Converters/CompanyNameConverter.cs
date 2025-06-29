// Converters/CompanyNameConverter.cs
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace BlackBook.Converters;

public sealed class CompanyNameConverter : IValueConverter {
    public object Convert (object value, Type _, object __, CultureInfo ___) {
        if (SessionManager.Data is null || value is null) return "–";

        Guid id = ExtractGuid(value);
        if (id == Guid.Empty) return "–";

        return SessionManager.Data.Companies
                                 .FirstOrDefault(c => c.Id == id)
                                ?.Name ?? "–";
    }

    public object ConvertBack (object value, Type _, object __, CultureInfo ___)
        => throw new NotSupportedException();

    private static Guid ExtractGuid (object value) {
        if (value is Guid g) return g;
        if (value is string s && Guid.TryParse(s, out var p)) return p;
        return Guid.Empty;
    }
}
