using SafeBox.Commands;
using SafeBox.Enums;
using SafeBox.EventArguments;
using SafeBox.Interfaces;
using SafeBox.Models;
using SafeBox.Security;
using System.Security;

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

        #endregion

        #region Binding Properties

        public string ResourceName { get => _resourceName; set => Set(ref _resourceName, value); }
        public ServiceType SelectedServiceType { get => _selectedServiceType; set => Set(ref _selectedServiceType, value); }
        public string Login { get => _login; set => Set(ref _login, value); }
        public string Password { get => _password; set => Set(ref _password, value); }

        #endregion

        #region Commands

        public RelayCommand CreateCommand => new(CreateMember);

        #endregion

        public delegate void OnCreatingMemberFinished(CreatingMemberFinishedEventArgs e);
        public event OnCreatingMemberFinished CreatingFinished;

        public void AttachNativeCryptographer(ICryptographer<SecureString> cryptographer) =>
            nativeCryptographer = cryptographer;

        private void CreateMember() =>
            CreatingFinished?.Invoke(new(GetEncryptedStorageMember()));

        private IStorageMember GetEncryptedStorageMember()
        {
            var storageMember = new StorageMember(ResourceName, SelectedServiceType, Login, null)
            {
                PasswordHash = nativeCryptographer.Encrypt(Password)
            };

            SecurityHelper.DecomposeString(ref _password);

            return storageMember;
        }
    }
}
