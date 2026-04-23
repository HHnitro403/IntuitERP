using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace IntuiERP.Avalonia.UI.Helpers
{
    public class BoolToStatusConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? "Ativo" : "Inativo";
            }
            return "Inativo";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue == "Ativo";
            }
            return false;
        }
    }
}
