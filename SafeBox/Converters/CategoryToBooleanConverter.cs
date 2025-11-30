using SafeBox.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SafeBox.Converters;

public class CategoryToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        Enum.TryParse<Category>(value?.ToString(), true, out var category) &&
        int.TryParse(parameter?.ToString(), out var val) &&
        (int)category == val;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value?.Equals(true) == true ? parameter : Binding.DoNothing;
}