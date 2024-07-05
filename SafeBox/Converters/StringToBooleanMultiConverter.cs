using SafeBox.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace SafeBox.Converters
{
    internal class StringToBooleanMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) =>
            !values.Any(x => ((string)x).IsNullOrWhiteSpace());

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
