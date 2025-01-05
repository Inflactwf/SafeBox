using SafeBox.Infrastructure;
using System.Configuration;
using System.Windows;

namespace SafeBox.Handlers
{
    public static class ConfigurationHandler
    {
        private static readonly Configuration appConfiguration;

        static ConfigurationHandler()
        {
            appConfiguration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public static void AddOrUpdate(string key, string value)
        {
            try
            {
                var settings = appConfiguration.AppSettings.Settings;

                if (settings[key] == null)
                    settings.Add(key, value);
                else
                    settings[key].Value = value;

                appConfiguration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(appConfiguration.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Unable to update configuration, try again later.", "SafeBox", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void UpdateSecureKey(string value) => AddOrUpdate(Constants.SecureKeyParameterName, value);

        public static void UpdateStorageFullPath(string value) => AddOrUpdate(Constants.StoragePathParameterName, value);

        public static string GetValue(string key) =>
            appConfiguration.AppSettings.Settings[key]?.Value;

        public static string SecureKey => GetValue(Constants.SecureKeyParameterName);

        public static string StorageFullPath => GetValue(Constants.StoragePathParameterName);
    }
}
