using SafeBox.Enums;
using SafeBox.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace SafeBox.Infrastructure
{
    public static class StaticResources
    {
        public static string StorageFullPath => ConfigurationHandler.GetValue(Constants.StoragePathParameterName);

        public static List<ServiceType> ServiceTypes =
             Enum.GetValues(typeof(ServiceType))
             .OfType<ServiceType>()
             .ToList();

        public static BitmapImage GetServiceImage(ServiceType serviceType) =>
            serviceType switch
            {
                ServiceType.Application => new BitmapImage(new Uri("pack://application:,,,/Resources/Application.png")),
                ServiceType.Steam => new BitmapImage(new Uri("pack://application:,,,/Resources/Steam.png")),
                ServiceType.Origin => new BitmapImage(new Uri("pack://application:,,,/Resources/Origin.png")),
                ServiceType.Uplay => new BitmapImage(new Uri("pack://application:,,,/Resources/Uplay.png")),
                _ => new BitmapImage(new Uri("pack://application:,,,/Resources/Website.png")),
            };
    }
}
