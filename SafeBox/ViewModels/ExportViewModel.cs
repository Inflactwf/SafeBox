using SafeBox.Commands;
using SafeBox.Extensions;
using SafeBox.Handlers;
using SafeBox.Infrastructure;
using SafeBox.Interfaces;
using SafeBox.Models;
using SafeBox.Security;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows.Forms;

namespace SafeBox.ViewModels
{
    public class ExportViewModel : ViewModelBase
    {
        #region Private Fields

        private readonly ICryptographer<string> aesCryptographer;
        private readonly ICryptographer<string> shaCryptographer;
        private ICryptographer<SecureString> nativeCryptographer;
        private IFileHandler fileHandler;
        private ObservableCollection<IStorageMember> collection = [];

        private string _location;
        private string _password = string.Empty;
        private string _repeatedPassword = string.Empty;

        #endregion

        public ExportViewModel()
        {
            aesCryptographer = new AesCryptographer();
            shaCryptographer = new SHACryptographer();
        }

        #region Binding Properties

        public string Password { get => _password; set => Set(ref _password, value); }
        public string RepeatedPassword { get => _repeatedPassword; set => Set(ref _repeatedPassword, value); }
        public string Location { get => _location; set => Set(ref _location, value); }

        #endregion

        #region Commands

        public RelayCommand RunExportCommand => new(RunExport);
        public RelayCommand SelectLocationCommand => new(SelectLocation);

        #endregion

        public void AttachExportableCollection(ObservableCollection<IStorageMember> collection)
        {
            this.collection.Clear();

            foreach (var member in collection)
                this.collection.Add(member.Clone());
        }

        public void AttachNativeCryptographer(ICryptographer<SecureString> cryptographer) =>
            nativeCryptographer = cryptographer;

        private bool PerformFieldsCheck()
        {
            if (_password != _repeatedPassword)
            {
                MessageBox.Show(Constants.PasswordAndConfirmationAreNotEquals, "SafeBox Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var passwordHash = shaCryptographer.Encrypt(Password);
                ReEncryptExtractingCollection(passwordHash);
                var encryptedData = aesCryptographer.Encrypt(collection.JsonSerializeObject(), passwordHash);
                fileHandler.Write(encryptedData);

                var logMsg = $"Accounts were successfully exported to the file '{fileHandler.FileName}'.";

                Logger.Info($"{Constants.ExportLogMark}: {logMsg}");
                MessageBox.Show(logMsg, "SafeBox Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                var logMsg = $"Unable to export accounts:\n\n{ex.Message}\n{ex.StackTrace}";

                Logger.Error($"{Constants.ExportLogMark}: {logMsg}");

                if (MessageBox.Show(logMsg, "SafeBox Export", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                    RunExport();
            }
        }

        private void ReEncryptExtractingCollection(string hash)
        {
            var unsafeCollection = new ObservableCollection<IStorageMember>();

            foreach (var member in collection)
            {
                using var securePwd = nativeCryptographer.Decrypt(member.PasswordHash);

                if (securePwd == null || securePwd.Length == 0)
                {
                    unsafeCollection.Add(member);
                    continue;
                }

                var decryptedPassword = SecurityHelper.SecureStringToString(securePwd);
                ((StorageMember)member).PasswordHash = aesCryptographer.Encrypt(decryptedPassword, hash);
                SecurityHelper.DecomposeString(ref decryptedPassword);
            }

            if (unsafeCollection.Count > 0)
                collection = new(collection.Except(unsafeCollection));
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
                fileHandler = new FileHandler(Path.Combine(Location, $"safebox_backup_{DateTime.Now:dd-MM-yyyy}.sbb"));
            }
        }
    }
}
