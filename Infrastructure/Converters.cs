using System.Globalization;
using System.Windows;
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
        return (bool)value ? new SolidColorBrush(Color.FromRgb(46, 204, 113)) : new SolidColorBrush(Color.FromRgb(231, 76, 60));
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

// New Converter for Bubble Alignment
public class MessageAlignmentConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var messageUser = values[0] as string;
        var currentUser = values[1] as string;
        return messageUser == currentUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

// New Converter for Bubble Colors
public class MessageColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var messageUser = values[0] as string;
        var currentUser = values[1] as string;
        // Right side (Me) = Blue, Left side (Them) = White/Gray
        return messageUser == currentUser ? new SolidColorBrush(Color.FromRgb(0, 132, 255)) : new SolidColorBrush(Color.FromRgb(240, 240, 240));
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

// New Converter for Text Colors
public class MessageTextColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var messageUser = values[0] as string;
        var currentUser = values[1] as string;
        return messageUser == currentUser ? Brushes.White : Brushes.Black;
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
