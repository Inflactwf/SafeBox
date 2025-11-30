using SafeBox.Infrastructure;
using SafeBox.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace SafeBox.Converters;

public class CollectionToSideCategoryCountConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!int.TryParse(parameter!.ToString(), out var categoryId))
        {
            Logger.Fatal($"{nameof(CollectionToSideCategoryCountConverter)}: Argument '{nameof(parameter)}' cannot be null or empty.");
            throw new ArgumentNullException(nameof(parameter));
        }

        if (value is IList<StorageMember> { Count: > 0 } collection)
            return collection.Count(x => (int)x.Category == categoryId);

        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}