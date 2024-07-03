using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SafeBox.Converters
{
    public class ZeroCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value != null && long.TryParse(value.ToString(), out var val) && val > 0
            ? Visibility.Visible
            : Visibility.Hidden;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
