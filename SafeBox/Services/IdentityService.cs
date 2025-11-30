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

namespace SafeBox.Services;

public sealed class IdentityService(IWindowService windowService, StorageHandler storageHandler,
    ViewSynchronizationService<StorageMember> synchronizationService)
{
    public void Init()
    {
        if (!IsCurrentMachineVerified())
        {
            using var authenticationViewModel = new AuthenticationViewModel(this);
            windowService.ShowWindow<AuthenticationWindow>(authenticationViewModel);

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
            if (!synchronizationService.HasElements)
                return true;

            using var sKey = key ?? SecurityHelper.TryGetSecureKeyAsSecureStringOrNull();

            return synchronizationService.GetSourceCollection().All(x =>
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
        // ReSharper disable once UseCollectionExpression
        storageHandler.OverwriteStorage(Enumerable.Empty<StorageMember>());
        synchronizationService?.Clear();
    }
}