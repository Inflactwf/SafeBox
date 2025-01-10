using SafeBox.Extensions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SafeBox.Converters
{
    public class ResourceUrlToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            (value as string).IsUrl();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
