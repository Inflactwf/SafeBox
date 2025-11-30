using SafeBox.Commands;
using SafeBox.Extensions;
using SafeBox.Handlers;
using SafeBox.Infrastructure;
using SafeBox.Interfaces;
using SafeBox.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using SafeBox.Models;

namespace SafeBox.ViewModels;

public class ExportViewModel : ViewModelBase
{
    #region Private Fields

    private IFileHandler _fileHandler;
    private readonly IEnumerable<StorageMember> _collection = [];

    private string _location;
    private string _password = string.Empty;
    private string _repeatedPassword = string.Empty;

    #endregion

    #region Binding Properties

    public string Password { get => _password; set => Set(ref _password, value); }
    public string RepeatedPassword { get => _repeatedPassword; set => Set(ref _repeatedPassword, value); }
    public string Location { get => _location; set => Set(ref _location, value); }

    #endregion

    public event Action RequestClose;

    public ExportViewModel() { }

    public ExportViewModel(IEnumerable<StorageMember> collection)
    {
        _collection = collection;
    }

    #region Commands

    public RelayCommand RunExportCommand => new(RunExport);
    public RelayCommand SelectLocationCommand => new(SelectLocation);

    #endregion

    private bool PerformFieldsCheck()
    {
        if (_password != _repeatedPassword)
        {
            MessageBox.Show("The password and repeat password do not match. Fill all the required fields correctly and try again.",
                "SafeBox Export", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return false;
        }

        return true;
    }

    private void RunExport()
    {
        if (!PerformFieldsCheck())
            return;

        try
        {
            using var passwordKey = SecurityHelper.ToSecureString(SHACryptographer.Encrypt(Password));
            var encryptedData = AesCryptographer.Encrypt(_collection.JsonSerializeObject(), passwordKey);

            if (encryptedData.IsNull())
                throw new Exception("Encrypted data is null or empty.");

            _fileHandler.Write(encryptedData);

            var logMsg = $"Accounts were successfully exported to the file '{_fileHandler.FileName}'.";
                
            Logger.Info($"{Constants.ExportLogMark}: {logMsg}");
            MessageBox.Show(logMsg, "SafeBox Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            Logger.Error($"{Constants.ExportLogMark}: {ex.Message}\n{ex.StackTrace}");

            if (MessageBox.Show($"An error occurred while exporting accounts.\nReason: {ex.Message}",
                    "SafeBox Export", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
            {
                RunExport();
            }
        }

        RequestClose?.Invoke();
    }

    private void SelectLocation()
    {
        var dialog = new FolderBrowserDialog()
        {
            SelectedPath = AppDomain.CurrentDomain.BaseDirectory,
            ShowNewFolderButton = true,
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            Location = dialog.SelectedPath.Replace(Constants.Space, Constants.NonBreakingSpace);
            _fileHandler = new FileHandler(Path.Combine(Location, $"safebox_backup_{DateTime.Now:dd-MM-yyyy}.sbb"));
        }
    }
}