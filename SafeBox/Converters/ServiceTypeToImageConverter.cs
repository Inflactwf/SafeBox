using SafeBox.Enums;
using SafeBox.Infrastructure;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SafeBox.Converters
{
    public class ServiceTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return value;

            return StaticResources.GetServiceImage((ServiceType)value);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            DependencyProperty.UnsetValue;
    }
}
