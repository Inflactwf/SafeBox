using SafeBox.Commands;
using SafeBox.EventArguments;
using SafeBox.Extensions;
using SafeBox.Handlers;
using SafeBox.Infrastructure;
using SafeBox.Interfaces;
using SafeBox.Models;
using SafeBox.Security;
using SafeBox.Services;
using SafeBox.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using Constants = SafeBox.Infrastructure.Constants;

namespace SafeBox.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Private Fields

        private readonly ICryptographer<SecureString> dpapiCryptographer;
        private readonly IWindowService windowService;

        private ObservableCollection<StorageMember> _backupStorageCollection = [];
        private ObservableCollection<StorageMember> _storageCollection = [];
        
        private string _searchCriteria = string.Empty;
        private StorageMember _selectedStorageItem;

        #endregion

        public MainWindowViewModel()
        {
            dpapiCryptographer = new DPAPICryptographer();
            windowService = new WindowService();

            LoadStorage();
        }

        #region Public Properties

        public StorageMember SelectedStorageItem { get => _selectedStorageItem; set => Set(ref _selectedStorageItem, value); }
        public ObservableCollection<StorageMember> BackupStorageCollection { get => _backupStorageCollection; set => Set(ref _backupStorageCollection, value); }
        public ObservableCollection<StorageMember> StorageCollection { get => _storageCollection; set => Set(ref _storageCollection, value); }

        public string SearchCriteria
        {
            get => _searchCriteria;
            set
            {
                if (!value.IsNullOrWhiteSpace())
                {
                    StorageCollection = new(BackupStorageCollection.Where(x => x.Login
                        .ToLower()
                        .Contains(value.ToLower())));
                }
                else
                {
                    StorageCollection = new(BackupStorageCollection);
                }

                Set(ref _searchCriteria, value);
            }
        }

        #endregion

        #region Commands

        public RelayCommand<string> CopyToClipboardCommand => new(CopyToClipboard);
        public RelayCommand<StorageMember> ShowPasswordCommand => new(async (member) => await ShowPassword(member));
        public RelayCommand RemoveCommand => new(RemoveMember);
        public RelayCommand AddCommand => new(AddMember);
        public RelayCommand ShowImportCommand => new(RunImport);
        public RelayCommand ShowExportCommand => new(RunExport);
        public RelayCommand ShowSettingsCommand => new(RunSettings);

        #endregion

        private void LoadStorage()
        {
            var entries = StorageHandler.GetEntries();
            ImportStorage(entries);
        }

        private void CopyToClipboard(string passwordHash)
        {
            using var secureString = dpapiCryptographer.Decrypt(passwordHash);
            var insecureString = secureString == null
                ? "UNKNOWN"
                : SecurityHelper.SecureStringToString(secureString);

            try
            {
                ClipboardHandler.CopyTextToClipboard(insecureString);
            }
            finally
            {
                SecurityHelper.DecomposeString(ref insecureString);
            }
        }

        private async Task ShowPassword(StorageMember member)
        {
            using var secureString = dpapiCryptographer.Decrypt(member.PasswordHash);
            var insecureString = secureString == null 
                ? "UNKNOWN"
                : SecurityHelper.SecureStringToString(secureString);

            try
            {
                member.DisplayInsecurePassword = string.Intern(insecureString);
                member.IsPasswordVisible = true;
                await Task.Delay(Constants.PasswordShowTimeInMilliseconds);
            }
            finally
            {
                member.IsPasswordVisible = false;
                SecurityHelper.DecomposeString(ref insecureString);
            }
        }

        private void AddMember()
        {
            var vm = new CreateMemberViewModel();
            vm.AttachNativeCryptographer(dpapiCryptographer);
            vm.CreatingFinished += CreateMemberViewModel_CreatingFinished;

            windowService.ShowWindow<CreateMemberWindow>(vm);

            vm.CreatingFinished -= CreateMemberViewModel_CreatingFinished;
        }

        private void RemoveMember()
        {
            // We should write to temporary variable because message box removes the focus from the selected item of listbox.
            var storageMember = SelectedStorageItem; 

            if (MessageBox.Show(
                $"Resource Name: {storageMember.ResourceName}\n" +
                $"Login: {storageMember.Login}\n" +
                $"Service Type: {storageMember.ServiceType}\n\n" +
                $"Are you sure you want to delete the record?",
                "SafeBox Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
            {
                return;
            }

            StorageCollection.Remove(storageMember);
            BackupStorageCollection.Remove(storageMember);
            StorageHandler.DeleteEntry(storageMember);

            Logger.Info($"{Constants.RemoveLogMark}: Removed storage member '{storageMember.ResourceName}'.");
        }

        private void ImportStorage(IEnumerable<StorageMember> collection)
        {
            SearchCriteria = string.Empty;
            StorageCollection = new(collection);
            BackupStorageCollection = new(collection);
        }

        private void ImportStorageMember(StorageMember member)
        {
            StorageCollection.Add(member);
            BackupStorageCollection.Add(member);
        }

        private void RunImport()
        {
            var vm = new ImportViewModel();
            vm.AttachNativeCryptographer(dpapiCryptographer);
            vm.ImportFinished += ImportViewModel_ImportFinished;

            windowService.ShowWindow<ImportWindow>(vm);

            vm.ImportFinished -= ImportViewModel_ImportFinished;
        }

        private void RunExport()
        {
            if (BackupStorageCollection.Count == 0)
            {
                Logger.Error($"{Constants.ExportLogMark}: {Constants.ExportEmptyStorageCollectionMessage}");
                MessageBox.Show(Constants.ExportEmptyStorageCollectionMessage, "SafeBox Export", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            if (!IsCurrentMachineVerified())
            {
                Logger.Error($"{Constants.ExportLogMark}: {Constants.LocalMachineIsNotVerifiedMessage}");
                MessageBox.Show(Constants.LocalMachineIsNotVerifiedMessage, "SafeBox Export", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }    

            var vm = new ExportViewModel();

            vm.AttachExportableCollection(BackupStorageCollection);
            vm.AttachNativeCryptographer(dpapiCryptographer);

            windowService.ShowWindow<ExportWindow>(vm);
        }

        private void RunSettings()
        {
            var vm = new SettingsViewModel();
            vm.SettingsChanged += SettingsViewModel_SettingsChanged;
            windowService.ShowWindow<SettingsWindow>(vm);
            vm.SettingsChanged -= SettingsViewModel_SettingsChanged;
        }

        private bool IsCurrentMachineVerified()
        {
            try
            {
                return BackupStorageCollection.All(x =>
                {
                    using var secPwd = dpapiCryptographer.Decrypt(x.PasswordHash);
                    return secPwd != null && secPwd.Length > 0;
                });
            }
            catch
            {
                return false;
            }
        }

        private void ImportViewModel_ImportFinished(ImportFinishedEventArgs e)
        {
            if (e.IsSuccess)
            {
                StorageHandler.OverwriteStorage(e.ImportedCollection);
                ImportStorage(e.ImportedCollection);

                Logger.Info($"{Constants.ImportLogMark}: " +
                    $"{BackupStorageCollection.Count} storage members were successfully imported from the file '{e.FileName}'.");

                MessageBox.Show($"Accounts were successfully imported from the file '{e.FileName}'.",
                    "SafeBox Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Logger.Error($"{Constants.ImportLogMark}: {e.Message}");
                MessageBox.Show(e.Message, "SafeBox Export", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateMemberViewModel_CreatingFinished(CreatingMemberFinishedEventArgs e)
        {
            if (!StorageHandler.IsEntryExists(e.StorageMember))
            {
                StorageHandler.AddEntry(e.StorageMember);
                ImportStorageMember(e.StorageMember);
                SelectedStorageItem = e.StorageMember;

                Logger.Info($"{Constants.CreateLogMark}: Added a new storage member '{e.StorageMember.ResourceName}'.");
            }
            else
            {
                Logger.Error($"{Constants.CreateLogMark}: {Constants.CreateExistingStorageMemberMessage}");
                MessageBox.Show(Constants.CreateExistingStorageMemberMessage, "SafeBox", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SettingsViewModel_SettingsChanged(SettingsChangedEventArgs e)
        {
            if (e.HasStorageChanged)
            {
                StorageHandler.Refresh();
                LoadStorage();

                Logger.Info($"{Constants.SettingsLogMark}: The storage path has been changed '{StorageHandler.GetStoragePath()}'");
            }
        }
    }
}
