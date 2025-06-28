// Converters/PersonNameConverter.cs
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using BlackBook;

namespace BlackBook.Converters;

    /// <summary>Returns the Person.Name that belongs to the supplied PersonId string.</summary>
    public sealed class PersonNameConverter : IValueConverter {
        public object Convert (object value, Type t, object p, CultureInfo c) {
            var id = value as string;
            if (string.IsNullOrEmpty(id) || SessionManager.Data is null) return "–";
            return SessionManager.Data.People.FirstOrDefault(pe => pe.Id == id)?.Name ?? "–";
        }

        public object ConvertBack (object value, Type t, object p, CultureInfo c)
            => throw new NotSupportedException();
    }
