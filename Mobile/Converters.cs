using System.Globalization;
using Microsoft.Maui.Graphics;

namespace ChatSystem.Mobile;

public class InverseBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => !(bool)(value ?? false);
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => !(bool)(value ?? false);
}

public class ConnectionStatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (bool)(value ?? false) ? Colors.Green : Colors.Red;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class MessageAlignmentConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return LayoutOptions.Start;
        var messageUser = values[0] as string;
        var currentUser = values[1] as string;
        return messageUser == currentUser ? LayoutOptions.End : LayoutOptions.Start;
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class MessageColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return Colors.LightGray;
        var messageUser = values[0] as string;
        var currentUser = values[1] as string;
        return messageUser == currentUser ? Color.FromRgb(0, 132, 255) : Color.FromRgb(240, 240, 240);
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class MessageTextColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return Colors.Black;
        var messageUser = values[0] as string;
        var currentUser = values[1] as string;
        return messageUser == currentUser ? Colors.White : Colors.Black;
    }
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
