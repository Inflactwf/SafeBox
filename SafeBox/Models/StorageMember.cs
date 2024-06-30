using Newtonsoft.Json;
using SafeBox.Enums;
using SafeBox.ViewModels;
using System;

namespace SafeBox.Models
{
    public class StorageMember(string resourceName, ServiceType serviceType, string login, string passwordHash) : ViewModelBase, IComparable
    {
        private bool _isPasswordVisible;
        private string _displayInsecurePassword;

        public string ResourceName { get; } = resourceName;
        public ServiceType ServiceType { get; } = serviceType;
        public string Login { get; } = login;
        public string PasswordHash { get; private set; } = passwordHash;

        [JsonIgnore]
        public bool IsPasswordVisible { get => _isPasswordVisible; set => Set(ref _isPasswordVisible, value); }

        [JsonIgnore]
        public string DisplayInsecurePassword { get => _displayInsecurePassword; set => Set(ref _displayInsecurePassword, value); }

        public void ReplacePasswordHash(string hash) =>
            PasswordHash = hash;

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
