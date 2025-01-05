using SafeBox.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace SafeBox.Infrastructure
{
    public static class StaticResources
    {
        public static List<ServiceType> ServiceTypes =
             Enum.GetValues(typeof(ServiceType))
             .OfType<ServiceType>()
             .ToList();

        public static BitmapImage GetServiceImage(ServiceType serviceType) =>
            serviceType switch
            {
                ServiceType.Application => new BitmapImage(new("pack://application:,,,/Resources/Application.png")),
                ServiceType.Steam => new BitmapImage(new("pack://application:,,,/Resources/Steam.png")),
                ServiceType.Origin => new BitmapImage(new("pack://application:,,,/Resources/Origin.png")),
                ServiceType.Uplay => new BitmapImage(new("pack://application:,,,/Resources/Uplay.png")),
                ServiceType.BattleNet => new BitmapImage(new("pack://application:,,,/Resources/BattleNet.png")),
                ServiceType.Epic => new BitmapImage(new("pack://application:,,,/Resources/Epic.png")),
                _ => new BitmapImage(new("pack://application:,,,/Resources/Website.png")),
            };
    }
}
