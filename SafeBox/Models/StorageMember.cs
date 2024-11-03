using SafeBox.Enums;
using SafeBox.Interfaces;
using SafeBox.ViewModels;

namespace SafeBox.Models
{
    public class StorageMember(string resourceName, ServiceType serviceType, string login, string passwordHash, string description = null) : ViewModelBase, IStorageMember
    {
        #region Private Fields

        private string _resourceName = resourceName;
        private ServiceType _serviceType = serviceType;
        private string _description = description;
        private string _login = login;
        private string _passwordHash = passwordHash;
        private bool _isPasswordVisible;
        private string _displayInsecurePassword;

        #endregion

        public StorageMember() : this(null, ServiceType.Other, null, null) { }

        public string ResourceName { get => _resourceName; set => Set(ref _resourceName, value); }
        public ServiceType ServiceType { get => _serviceType; set => Set(ref _serviceType, value); }
        public string Description { get => _description; set => Set(ref _description, value == string.Empty ? null : value); }
        public string Login { get => _login; set => Set(ref _login, value); }
        public string PasswordHash { get => _passwordHash; set => Set(ref _passwordHash, value); }
        public bool IsPasswordVisible { get => _isPasswordVisible; set => Set(ref _isPasswordVisible, value); }
        public string DisplayInsecurePassword { get => _displayInsecurePassword; set => Set(ref _displayInsecurePassword, value); }

        public IStorageMember Clone() =>
            new StorageMember(ResourceName, ServiceType, Login, PasswordHash, Description);

        public int CompareTo(IStorageMember other)
        {
            if (other != null &&
                other.ResourceName == ResourceName &&
                other.ServiceType == ServiceType &&
                other.Description == Description &&
                other.Login == Login &&
                other.PasswordHash == PasswordHash)
            {
                return 0;
            }

            return 1;
        }
    }
}
