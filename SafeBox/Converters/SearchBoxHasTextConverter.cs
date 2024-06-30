using SafeBox.Extensions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SafeBox.Converters
{
    public class SearchBoxHasTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            !(value as string).IsNullOrWhiteSpace();

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
