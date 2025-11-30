using Newtonsoft.Json;
using SafeBox.Enums;
using SafeBox.ViewModels;
using System;

namespace SafeBox.Models;

public class StorageMember(string resourceName, ServiceType serviceType, Category category, string login, string passwordHash, string description = null)
    : ViewModelBase, IComparable<StorageMember>
{
    #region Private Fields

    private bool _isPasswordVisible;
    private string _displayInsecurePassword;
    private string _resourceName = resourceName;
    private ServiceType _serviceType = serviceType;
    private Category _category = category;
    private string _login = login;
    private string _passwordHash = passwordHash;
    private string _description = description;

    #endregion

    public string ResourceName { get => _resourceName; set => Set(ref _resourceName, value); }

    public ServiceType ServiceType { get => _serviceType; set => Set(ref _serviceType, value); }

    public Category Category { get => _category; set => Set(ref _category, value); }

    public string Description { get => _description; set => Set(ref _description, value == string.Empty ? null : value); }

    public string Login { get => _login; set => Set(ref _login, value); }

    public string PasswordHash { get => _passwordHash; set => Set(ref _passwordHash, value); }

    [JsonIgnore]
    public bool IsPasswordVisible { get => _isPasswordVisible; set => Set(ref _isPasswordVisible, value); }

    [JsonIgnore]
    public string DisplayInsecurePassword { get => _displayInsecurePassword; set => Set(ref _displayInsecurePassword, value); }

    public StorageMember() : this(null, ServiceType.Other, Category.All, null, null) { }

    public virtual StorageMember Clone() => new(ResourceName, ServiceType, Category, Login, PasswordHash, Description);

    public virtual int CompareTo(StorageMember other)
    {
        if (other != null &&
            other.ResourceName == ResourceName &&
            other.ServiceType == ServiceType &&
            other.Category == Category &&
            other.Description == Description &&
            other.Login == Login &&
            other.PasswordHash == PasswordHash)
        {
            return 0;
        }

        return 1;
    }
}