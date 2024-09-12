using GongSolutions.Wpf.DragDrop;
using SafeBox.Commands;
using SafeBox.EventArguments;
using SafeBox.Handlers;
using SafeBox.Infrastructure;
using SafeBox.Interfaces;
using SafeBox.Models;
using SafeBox.Security;
using SafeBox.Services;
using SafeBox.Views;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;

namespace SafeBox.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDropTarget
    {
        #region Private Fields

        private readonly ICryptographer<SecureString> dpapiCryptographer;
        private readonly IWindowService windowService;

        #endregion

        public MainWindowViewModel()
        {
            dpapiCryptographer = new DPAPICryptographer();
            windowService = new WindowService();
            SynchronizationService = new();

            LoadStorage();
        }

        #region Public Properties

        public ViewSynchronizationService<IStorageMember> SynchronizationService { get; }

        #endregion

        #region Commands

        public RelayCommand<string> CopyToClipboardCommand => new(CopyToClipboard);
        public RelayCommand<IStorageMember> ShowPasswordCommand => new(async (member) => await ShowPassword(member));
        public RelayCommand RemoveCommand => new(RemoveMember);
        public RelayCommand AddCommand => new(AddMember);
        public RelayCommand EditCommand => new(EditMember);
        public RelayCommand ShowImportCommand => new(RunImport);
        public RelayCommand ShowExportCommand => new(RunExport);
        public RelayCommand ShowSettingsCommand => new(RunSettings);

        #endregion

        private void LoadStorage() => ImportStorage(StorageHandler.GetEntries());

        private void CopyToClipboard(string passwordHash)
        {
            DecryptHashToInsecurePassword(passwordHash, out string insecurePassword);

            try
            {
                ClipboardHandler.CopyTextToClipboard(insecurePassword);
            }
            finally
            {
                SecurityHelper.DecomposeString(ref insecurePassword);
            }
        }

        private async Task ShowPassword(IStorageMember member)
        {
            DecryptHashToInsecurePassword(member.PasswordHash, out string insecurePassword);

            try
            {
                member.DisplayInsecurePassword = string.Intern(insecurePassword);
                member.IsPasswordVisible = true;
                await Task.Delay(Constants.PasswordShowTimeInMilliseconds);
            }
            finally
            {
                member.IsPasswordVisible = false;
                SecurityHelper.DecomposeString(ref insecurePassword);
            }
        }

        private void DecryptHashToInsecurePassword(string passwordHash, out string insecurePassword)
        {
            using var secureString = dpapiCryptographer.Decrypt(passwordHash);

            insecurePassword = secureString == null
                ? "UNKNOWN"
                : SecurityHelper.SecureStringToString(secureString);
        }

        private void AddMember()
        {
            var vm = new CreateMemberViewModel();
            vm.AttachNativeCryptographer(dpapiCryptographer);
            vm.CreatingFinished += CreateMemberViewModel_CreatingFinished;

            windowService.ShowWindow<CreateMemberWindow>(vm);

            vm.CreatingFinished -= CreateMemberViewModel_CreatingFinished;
        }

        private void EditMember()
        {
            if (!IsCurrentMachineVerified())
            {
                Logger.Error($"{Constants.EditLogMark}: {Constants.LocalMachineIsNotVerifiedMessage}");
                MessageBox.Show(Constants.LocalMachineIsNotVerifiedMessage, "SafeBox Export", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            var vm = new EditMemberViewModel();
            vm.AttachStorageMember(SynchronizationService.SelectedItem);
            vm.AttachNativeCryptographer(dpapiCryptographer);
            vm.EditingFinished += EditMemberViewModel_EditingFinished;

            windowService.ShowWindow<EditMemberWindow>(vm);

            vm.EditingFinished -= EditMemberViewModel_EditingFinished;
        }

        private void RemoveMember()
        {
            // We should write to temporary variable because message box removes the focus from selected item of the listbox.
            var storageMember = SynchronizationService.SelectedItem;

            if (MessageBox.Show(
                $"Resource Name: {storageMember.ResourceName}\n" +
                $"Login: {storageMember.Login}\n" +
                $"Service Type: {storageMember.ServiceType}\n\n" +
                $"Are you sure you want to delete the record?",
                "SafeBox Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
            {
                return;
            }

            SynchronizationService.Remove(storageMember);
            StorageHandler.DeleteEntry(storageMember);

            Logger.Info($"{Constants.RemoveLogMark}: Removed storage member '{storageMember.ResourceName}'.");
        }

        private void ImportStorage(IEnumerable<IStorageMember> collection)
        {
            SynchronizationService.SearchCriteria = string.Empty;
            SynchronizationService.Set(collection);
        }

        private void ReplaceStorageMember(IStorageMember oldMember, IStorageMember newMember)
        {
            StorageHandler.ReplaceEntry(oldMember, newMember);
            SynchronizationService.Replace(oldMember, newMember);
        }

        private void ImportStorageMember(IStorageMember member) =>
            SynchronizationService.Add(member);

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
            if (!SynchronizationService.HasElements)
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

            vm.AttachExportableCollection(SynchronizationService.CloneCollection());
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
                return SynchronizationService.SourceCollection.All(x =>
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
                    $"{e.ImportedCollection.Count()} storage members were successfully imported from the file '{e.FileName}'.");

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
                SynchronizationService.SelectedItem = e.StorageMember;

                Logger.Info($"{Constants.CreateLogMark}: Added a new storage member '{e.StorageMember.ResourceName}'.");
            }
            else
            {
                Logger.Error($"{Constants.CreateLogMark}: {Constants.CreateExistingStorageMemberMessage}");
                MessageBox.Show(Constants.CreateExistingStorageMemberMessage, "SafeBox", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditMemberViewModel_EditingFinished(EditingMemberFinishedEventArgs e)
        {
            if (e.HasChanges)
            {
                ReplaceStorageMember(e.SourceMember, e.EditedMember);

                Logger.Error($"{Constants.EditLogMark}: The storage member '{e.SourceMember.ResourceName}' has been successfully edited.");
                MessageBox.Show("The record has been succesfully edited.", "SafeBox", MessageBoxButton.OK, MessageBoxImage.Information);
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

        public void DragEnter(IDropInfo dropInfo) { }

        public void DragOver(IDropInfo dropInfo)
        {
            var targetItem = (IStorageMember)dropInfo.TargetItem;
            if (targetItem != null)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void DragLeave(IDropInfo dropInfo) { }

        public void Drop(IDropInfo dropInfo)
        {
            var sourceMember = dropInfo.Data as IStorageMember;
            var targetMember = dropInfo.TargetItem as IStorageMember;

            if (sourceMember == targetMember)
                return;

            SynchronizationService.Move(sourceMember, targetMember);
            StorageHandler.OverwriteStorage(SynchronizationService.GetExplicitCollectionOfType<StorageMember>());
        }
    }
}
