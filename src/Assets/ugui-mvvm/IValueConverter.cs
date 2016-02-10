using System;

namespace uguimvvm
{
    public interface IValueConverter
    {
        object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture);
        object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture);
    }
}