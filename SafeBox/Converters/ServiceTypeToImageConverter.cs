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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value != null
            ? StaticResources.GetServiceImage((ServiceType)value)
            : value;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            DependencyProperty.UnsetValue;
    }
}
