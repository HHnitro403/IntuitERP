using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuitERP.Helpers
{
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool ativo)
            {
                return ativo ? "Ativo" : "Excluído";
            }
            // Handle cases where Ativo might be null from the database,
            // though your ClienteModel likely defines it as non-nullable bool.
            if (value is null)
            {
                return "Indefinido"; // Or "N/A", or string.Empty
            }
            // Fallback for unexpected types, though ideally 'value' is always bool.
            return value?.ToString() ?? "Indefinido";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not needed for one-way conversion
            throw new NotImplementedException("BoolToStatusConverter ConvertBack is not implemented.");
        }
    }
}
