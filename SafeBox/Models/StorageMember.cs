using Newtonsoft.Json;
using SafeBox.Enums;
using SafeBox.ViewModels;
using System;

namespace SafeBox.Models
{
    public class StorageMember(string resourceName, ServiceType serviceType, string login, string passwordHash) : ViewModelBase, IComparable
    {
        #region Private Fields

        private string _resourceName = resourceName;
        private ServiceType _serviceType = serviceType;
        private string _login = login;
        private string _passwordHash = passwordHash;
        private bool _isPasswordVisible;
        private string _displayInsecurePassword;

        #endregion

        public string ResourceName { get => _resourceName; set => Set(ref _resourceName, value); }
        public ServiceType ServiceType { get => _serviceType; set => Set(ref _serviceType, value); }
        public string Login { get => _login; set => Set(ref _login, value); }
        public string PasswordHash { get => _passwordHash; set => Set(ref _passwordHash, value); }

        [JsonIgnore]
        public bool IsPasswordVisible { get => _isPasswordVisible; set => Set(ref _isPasswordVisible, value); }

        [JsonIgnore]
        public string DisplayInsecurePassword { get => _displayInsecurePassword; set => Set(ref _displayInsecurePassword, value); }

        public StorageMember Clone() =>
            new(ResourceName, ServiceType, Login, PasswordHash);

        public int CompareTo(object obj)
        {
            if (obj is StorageMember sm)
            {
                if (sm.ResourceName == ResourceName &&
                    sm.ServiceType == ServiceType &&
                    sm.Login == Login &&
                    sm.PasswordHash == PasswordHash)
                {
                    return 0;
                }
            }

            return 1;
        }
    }
}
