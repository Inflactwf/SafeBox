using SafeBox.Commands;
using SafeBox.Handlers;
using SafeBox.Security;
using SafeBox.Services;
using System;
using System.Security;
using System.Windows;

namespace SafeBox.ViewModels;

public class AuthenticationViewModel : ViewModelBase, IDisposable
{
    #region Private Fields

    private readonly IdentityService _identityService;
    private SecureString _password;
    private bool _isDisposed;

    #endregion

    #region Public Properties

    public SecureString Password { get => _password; set => Set(ref _password, value); }

    public bool IsAuthenticated { get; private set; }

    #endregion

    public event Action RequestClose;

    public AuthenticationViewModel() { }

    public AuthenticationViewModel(IdentityService identityService) =>
        _identityService = identityService;

    #region Commands

    public RelayCommand ValidateCommand => new(Validate);

    #endregion

    private void Validate()
    {
        var shaHash = ShaCryptographer.Encrypt(Password);
        using var key = SecurityHelper.ToSecureString(shaHash);
        var encryptedKey = DpapiCryptographer.Encrypt(key);

        if (_identityService.IsKeyValid(key))
        {
            UpdateKey(encryptedKey);
            return;
        }

        ShowPasswordIncorrectMessage(encryptedKey);
    }

    private void ShowPasswordIncorrectMessage(string encryptedKey)
    {
        switch (MessageBox.Show(
            $"""
             The password you have entered is incorrect and does not match the current account storage.


             If you want to restore your accounts, click «Yes» and enter the correct password.

             If you want to set this as a new password and reset your previous accounts, click «No».

             If you want to stop working with the program, click «Cancel».
             """,
            "SafeBox Identity Service",
            MessageBoxButton.YesNoCancel, MessageBoxImage.Error))
        {
            case MessageBoxResult.No:
            {
                _identityService.ResetIdentity();
                UpdateKey(encryptedKey);
                break;
            }
        }
    }

    private void UpdateKey(string encryptedKey)
    {
        ConfigurationHandler.UpdateSecureKey(encryptedKey);
        IsAuthenticated = true;
        RequestClose?.Invoke();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _password?.Dispose();
                _password = null;
            }

            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}