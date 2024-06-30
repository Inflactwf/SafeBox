using SafeBox.Commands;
using SafeBox.Enums;
using SafeBox.EventArguments;
using SafeBox.Extensions;
using SafeBox.Interfaces;
using SafeBox.Models;
using SafeBox.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Windows;

namespace SafeBox.ViewModels
{
    public class CreateMemberViewModel : ViewModelBase
    {
        #region Private Fields

        private ICryptographer<SecureString> nativeCryptographer;
        private string _resourceName;
        private ServiceType _selectedServiceType;
        private string _login;
        private string _password;
        private bool _isContinueButtonEnabled;

        #endregion

        #region Binding Properties

        public string ResourceName
        {
            get => _resourceName;
            set
            {
                Set(ref _resourceName, value);
                PerformFieldsCheck();
            }
        }

        public ServiceType SelectedServiceType { get => _selectedServiceType; set => Set(ref _selectedServiceType, value); }

        public string Login
        {
            get => _login;
            set
            {
                Set(ref _login, value);
                PerformFieldsCheck();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                Set(ref _password, value);
                PerformFieldsCheck();
            }
        }

        public List<ServiceType> ServiceTypes { get; set; } =
            Enum.GetValues(typeof(ServiceType))
            .OfType<ServiceType>()
            .ToList();

        public bool IsContinueButtonEnabled { get => _isContinueButtonEnabled; private set => Set(ref _isContinueButtonEnabled, value); }

        #endregion

        #region Commands

        public RelayCommand CreateCommand => new(CreateMember);

        #endregion

        public delegate void OnCreatingMemberFinished(CreatingMemberFinishedEventArgs e);
        public event OnCreatingMemberFinished CreatingFinished;

        public void AttachNativeCryptographer(ICryptographer<SecureString> cryptographer) =>
            nativeCryptographer = cryptographer;

        private void CreateMember()
        {
            if (!PerformFieldsCheck())
            {
                MessageBox.Show("One of the fields is not set or invalid. Fill all the required fields correctly and try again later.",
                    "SafeBox", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            CreatingFinished?.Invoke(new(GetEncryptedStorageMember()));
        }

        private StorageMember GetEncryptedStorageMember()
        {
            var storageMember = new StorageMember(ResourceName, SelectedServiceType, Login, null);
            storageMember.ReplacePasswordHash(nativeCryptographer.Encrypt(Password));

            SecurityHelper.DecomposeString(ref _password);

            return storageMember;
        }

        private bool PerformFieldsCheck() =>
            IsContinueButtonEnabled =
                !_resourceName.IsNullOrWhiteSpace() &&
                !_login.IsNullOrWhiteSpace() &&
                !_password.IsNullOrWhiteSpace();
    }
}
