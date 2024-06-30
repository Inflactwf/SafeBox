using SafeBox.Commands;
using SafeBox.EventArguments;
using SafeBox.Extensions;
using SafeBox.Handlers;
using SafeBox.Interfaces;
using SafeBox.Models;
using SafeBox.Security;
using SafeBox.Services;
using SafeBox.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        public ObservableCollection<StorageMember> StorageCollection { get => _storageCollection; set => Set(ref _storageCollection, value); }

        public string SearchCriteria
        {
            get => _searchCriteria;
            set
            {
                if (!value.IsNullOrWhiteSpace())
                {
                    StorageCollection = new(_backupStorageCollection.Where(x => x.Login
                        .ToLower()
                        .Contains(value.ToLower())));
                }
                else
                {
                    StorageCollection = new(_backupStorageCollection);
                }

                Set(ref _searchCriteria, value);
            }
        }

        #endregion

        #region Commands

        public RelayCommand<string> CopyToClipboardCommand => new(CopyToClipboard);
        public RelayCommand<StorageMember> ShowPasswordCommand => new(async (member) => await ShowPassword(member));
        public RelayCommand<StorageMember> RemoveCommand => new(RemoveMember);
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

        private void CopyToClipboard(string data)
        {
            using var secureString = dpapiCryptographer.Decrypt(data);
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

        private void RemoveMember(StorageMember member)
        {
            if (MessageBox.Show(
                $"Resource Name: {member.ResourceName}\n" +
                $"Login: {member.Login}\n" +
                $"Service Type: {member.ServiceType}\n\n" +
                $"Are you sure you want to delete the record?",
                "SafeBox Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
            {
                return;
            }

            StorageCollection.Remove(member);
            _backupStorageCollection.Remove(member);
            StorageHandler.DeleteEntry(member);
        }

        private void ImportStorage(IEnumerable<StorageMember> collection)
        {
            SearchCriteria = string.Empty;
            StorageCollection = new(collection);
            _backupStorageCollection = new(collection);
        }

        private void ImportStorageMember(StorageMember member)
        {
            StorageCollection.Add(member);
            _backupStorageCollection.Add(member);
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
            if (_backupStorageCollection.Count == 0)
            {
                MessageBox.Show("Nothing to export, the process is canceled.", "SafeBox Export", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            if (!IsCurrentMachineVerified())
            {
                MessageBox.Show("The local machine is not verified, the export process is canceled.",
                    "SafeBox Export", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }    

            var vm = new ExportViewModel();

            vm.AttachExportableCollection(_backupStorageCollection);
            vm.AttachNativeCryptographer(dpapiCryptographer);

            windowService.ShowWindow<ExportWindow>(vm);
        }

        private void RunSettings()
        {
            var vm = new SettingsViewModel();
            vm.SettingsChanged += SettingsViewModel_SettingsChanged;
            windowService.ShowWindow<SettingsWindow>(vm);
        }

        private bool IsCurrentMachineVerified()
        {
            try
            {
                return _backupStorageCollection.All(x =>
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

                MessageBox.Show($"Accounts were successfully imported from the file '{e.FileName}'.",
                    "SafeBox Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(e.Message,
                    "SafeBox Export", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateMemberViewModel_CreatingFinished(CreatingMemberFinishedEventArgs e)
        {
            if (!StorageHandler.IsEntryExists(e.StorageMember))
            {
                StorageHandler.AddEntry(e.StorageMember);
                ImportStorageMember(e.StorageMember);
                SelectedStorageItem = e.StorageMember;
            }
            else
            {
                MessageBox.Show("An exact element is already exists in the storage, the process is canceled.",
                    "SafeBox", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SettingsViewModel_SettingsChanged(SettingsChangedEventArgs e)
        {
            if (e.HasStorageChanged)
            {
                StorageHandler.Refresh();
                LoadStorage();
            }
        }
    }
}
