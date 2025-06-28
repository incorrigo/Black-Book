// Converters/CompanyNameConverter.cs
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using BlackBook;

namespace BlackBook.Converters;

    /// <summary>Returns the Company.Name that belongs to the supplied CompanyId string.</summary>
    public sealed class CompanyNameConverter : IValueConverter {
        public object Convert (object value, Type t, object p, CultureInfo c) {
            var id = value as string;
            if (string.IsNullOrEmpty(id) || SessionManager.Data is null) return "–";
            return SessionManager.Data.Companies.FirstOrDefault(co => co.Id == id)?.Name ?? "–";
        }

        public object ConvertBack (object value, Type t, object p, CultureInfo c)
            => throw new NotSupportedException();
    }
