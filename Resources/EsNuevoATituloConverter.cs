using System.Globalization;
using System.Windows.Data;

namespace POS.Resources;

public class EsNuevoATituloConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? "Nuevo producto" : "Editar producto";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}