using SafeBox.Commands;
using SafeBox.EventArguments;
using SafeBox.Extensions;
using SafeBox.Handlers;
using SafeBox.Infrastructure;
using SafeBox.Interfaces;
using SafeBox.Models;
using SafeBox.Security;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace SafeBox.ViewModels
{
    public class ImportViewModel : ViewModelBase
    {
        #region Private Fields

        private IFileHandler fileHandler;
        private string _location;
        private string _password;
        private bool _isMergeRequested;

        #endregion

        #region Binding Properties

        public string Location { get => _location; set => Set(ref _location, value); }
        public string Password { get => _password; set => Set(ref _password, value); }
        public bool IsMergeRequested { get => _isMergeRequested; set => Set(ref _isMergeRequested, value); }

        #endregion

        public delegate void OnImportFinished(ImportFinishedEventArgs e);
        public event OnImportFinished ImportFinished;
        public event Action RequestClose;

        #region Commands

        public RelayCommand RunImportCommand => new(RunImport);
        public RelayCommand SelectLocationCommand => new(SelectLocation);

        #endregion

        private void RunImport()
        {
            try
            {
                var passwordShaHash = SHACryptographer.Encrypt(Password);
                using var securePassword = SecurityHelper.ToSecureString(passwordShaHash);
                var encryptedData = fileHandler.Read() ?? string.Empty;
                var decryptedData = AesCryptographer.Decrypt(encryptedData, securePassword);

                if (decryptedData.IsNullOrWhiteSpace())
                {
                    ImportFinished?.Invoke(new(false, "Decrypted data is empty or has been corrupted, the import process is stopped.", fileHandler.FileName, null, IsMergeRequested));
                    return;
                }

                var decryptedCollection = decryptedData.JsonDeserializeObject<IEnumerable<StorageMember>>();

                SecurityHelper.DecomposeString(ref decryptedData);
                ImportFinished?.Invoke(new(true, null, fileHandler.FileName, decryptedCollection, IsMergeRequested));
            }
            catch (CryptographicException)
            {
                Logger.Error($"{Constants.ImportLogMark}: An invalid password has been entered.");

                MessageBox.Show($"An error occurred while importing accounts.\nReason: An invalid password has been entered.",
                    "SafeBox Export", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            catch (Exception ex)
            {
                Logger.Error($"{Constants.ImportLogMark}: {ex.Message}\n{ex.StackTrace}");

                if (MessageBox.Show($"An error occurred while importing accounts.\nReason: {ex.Message}",
                    "SafeBox Export", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                {
                    RunImport();
                }
            }

            RequestClose?.Invoke();
        }

        private void SelectLocation()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Selecting the storage backup file...",
                Filter = "SafeBox Storage Backup|*.sbb|All Files|*.*"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Location = dialog.FileName.Replace(Constants.Space, Constants.NonBreakingSpace);
                fileHandler = new FileHandler(Location);
            }
        }
    }
}
