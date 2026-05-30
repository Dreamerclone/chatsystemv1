using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ChatSystem.Infrastructure;

public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => !(bool)value;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => !(bool)value;
}

public class ConnectionStatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? Brushes.Green : Brushes.Red;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
