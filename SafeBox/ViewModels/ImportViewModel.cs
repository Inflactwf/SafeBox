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
using System.Collections.ObjectModel;
using System.Security;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace SafeBox.ViewModels
{
    public class ImportViewModel : ViewModelBase
    {
        #region Private Fields

        private readonly ICryptographer<string> aesCryptographer;
        private readonly ICryptographer<string> shaCryptographer;
        private ICryptographer<SecureString> nativeCryptographer;
        private IFileHandler fileHandler;

        private string _location;
        private string _password;

        #endregion

        public delegate void OnImportFinished(ImportFinishedEventArgs e);
        public event OnImportFinished ImportFinished;

        public ImportViewModel()
        {
            aesCryptographer = new AesCryptographer();
            shaCryptographer = new SHACryptographer();
        }

        #region Binding Properties

        public string Location { get => _location; set => Set(ref _location, value); }
        public string Password { get => _password; set => Set(ref _password, value); }

        #endregion

        #region Commands

        public RelayCommand RunImportCommand => new(RunImport);
        public RelayCommand SelectLocationCommand => new(SelectLocation);

        #endregion

        public void AttachNativeCryptographer(ICryptographer<SecureString> cryptographer) =>
            nativeCryptographer = cryptographer;

        private void RunImport()
        {
            try
            {
                var encryptedPwd = shaCryptographer.Encrypt(Password);
                var encryptedData = fileHandler.Read() ?? string.Empty;
                var decryptedData = aesCryptographer.Decrypt(encryptedData, encryptedPwd);

                if (decryptedData.IsNullOrWhiteSpace())
                {
                    ImportFinished?.Invoke(new(false, Constants.DecryptedDataIsEmptyMessage, fileHandler.FileName, null));
                    return;
                }

                var decryptedCollection = decryptedData.JsonDeserializeObject<ObservableCollection<StorageMember>>();

                ReEncryptExtractingCollection(decryptedCollection, encryptedPwd);
                ImportFinished?.Invoke(new(true, null, fileHandler.FileName, decryptedCollection));
            }
            catch (CryptographicException)
            {
                Logger.Error($"{Constants.ImportLogMark}: An invalid password has been entered.");

                MessageBox.Show(
                    "Unable to import accounts:\n\nAn invalid password has been entered.",
                    "SafeBox Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                Logger.Error($"{Constants.ImportLogMark}: {ex.Message}\n{ex.StackTrace}");

                if (MessageBox.Show(
                    $"Unable to import accounts:\n\n{ex.Message}\n{ex.StackTrace}",
                    "SafeBox Export", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                {
                    RunImport();
                }
            }
        }

        private void ReEncryptExtractingCollection(IEnumerable<StorageMember> collection, string hash)
        {
            foreach (var member in collection)
            {
                var password = aesCryptographer.Decrypt(member.PasswordHash, hash);
                member.ReplacePasswordHash(nativeCryptographer.Encrypt(password));
                SecurityHelper.DecomposeString(ref password);
            }
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
