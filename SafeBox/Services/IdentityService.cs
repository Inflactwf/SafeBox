using SafeBox.Extensions;
using SafeBox.Handlers;
using SafeBox.Infrastructure;
using SafeBox.Interfaces;
using SafeBox.Models;
using SafeBox.Security;
using SafeBox.ViewModels;
using SafeBox.Views;
using System.Linq;
using System.Security;
using System.Windows;

namespace SafeBox.Services
{
    public sealed class IdentityService(IWindowService windowService, ViewSynchronizationService<IStorageMember> synchronizationService)
    {
        #region Private Fields

        private readonly IWindowService _windowService = windowService;
        private readonly ViewSynchronizationService<IStorageMember> _synchronizationService = synchronizationService;

        #endregion

        public void Init()
        {
            if (!IsCurrentMachineVerified())
            {
                using var authenticationViewModel = new AuthenticationViewModel(this);
                _windowService.ShowWindow<AuthenticationWindow>(authenticationViewModel);

                if (!authenticationViewModel.IsAuthenticated)
                {
                    Logger.Fatal($"{Constants.GeneralLogMark}: authentication failed, the program exited.");
                    Application.Current.Shutdown();
                }
            }
        }

        public bool IsCurrentMachineVerified()
        {
            using var secureKey = SecurityHelper.TryGetSecureKeyAsSecureStringOrNull();
            return !secureKey.IsNull();
        }

        public bool IsKeyValid(SecureString key = null)
        {
            try
            {
                if (!_synchronizationService.HasElements)
                    return true;

                using var sKey = key ?? SecurityHelper.TryGetSecureKeyAsSecureStringOrNull();

                return _synchronizationService.SourceCollection.All(x =>
                {
                    using var passwordContainer = AesCryptographer.DecryptToSecureString(x.PasswordHash, sKey);
                    return !passwordContainer.IsNull();
                });
            }
            catch
            {
                return false;
            }
        }

        public void ResetIdentity()
        {
            StorageHandler.OverwriteStorage(Enumerable.Empty<StorageMember>());
            _synchronizationService.Clear();
        }
    }
}
