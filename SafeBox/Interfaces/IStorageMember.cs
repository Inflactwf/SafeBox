using Newtonsoft.Json;
using SafeBox.Enums;
using System;

namespace SafeBox.Interfaces
{
    public interface IStorageMember : IComparable<IStorageMember>
    {
        string ResourceName { get; }
        ServiceType ServiceType { get; }
        string Description { get; }
        string Login { get; }
        string PasswordHash { get; }

        [JsonIgnore]
        bool IsPasswordVisible { get; set; }

        [JsonIgnore]
        string DisplayInsecurePassword { get; set; }

        IStorageMember Clone();
    }
}
