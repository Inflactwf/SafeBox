using Newtonsoft.Json;
using SafeBox.Enums;
using SafeBox.ViewModels;
using System;

namespace SafeBox.Models;

public class StorageMember(string resourceName, ServiceType serviceType, Category category, string login, string passwordHash, string description = null)
    : ViewModelBase, IComparable<StorageMember>
{
    public string ResourceName { get; set => Set(ref field, value); } = resourceName;

    public ServiceType ServiceType { get; set => Set(ref field, value); } = serviceType;

    public Category Category { get; set => Set(ref field, value); } = category;

    public string Description { get; set => Set(ref field, value == string.Empty ? null : value); } = description;

    public string Login { get; set => Set(ref field, value); } = login;

    public string PasswordHash { get; set => Set(ref field, value); } = passwordHash;

    [JsonIgnore]
    public bool IsPasswordVisible { get; set => Set(ref field, value); }

    [JsonIgnore]
    public string DisplayInsecurePassword { get; set => Set(ref field, value); }

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

    public override string ToString() =>
        $"[{ServiceType}] {ResourceName} - {Login}";
}