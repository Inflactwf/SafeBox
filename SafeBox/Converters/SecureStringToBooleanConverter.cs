using System;
using System.Globalization;
using System.Security;
using System.Windows.Data;

namespace SafeBox.Converters
{
    public class SecureStringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is SecureString secureString && secureString.Length > 0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
