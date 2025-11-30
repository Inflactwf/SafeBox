using GongSolutions.Wpf.DragDrop;
using SafeBox.Commands;
using SafeBox.Enums;
using SafeBox.EventArguments;
using SafeBox.Extensions;
using SafeBox.Handlers;
using SafeBox.Infrastructure;
using SafeBox.Interfaces;
using SafeBox.Models;
using SafeBox.Security;
using SafeBox.Services;
using SafeBox.Views;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SafeBox.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDropTarget
{
    #region Private Fields

    private readonly IWindowService _windowService;
    private readonly IdentityService _identityService;
    private readonly StorageHandler _storageHandler;
    private bool _isSidePanelExpanded;

    #endregion

    #region Public Properties

    public ViewSynchronizationService<StorageMember> SynchronizationService { get; }
    public bool IsSidePanelExpanded { get => _isSidePanelExpanded; set => Set(ref _isSidePanelExpanded, value); }

    #endregion

    #region Commands

    public RelayCommand<string> CopyToClipboardCommand => new(CopyToClipboard);
    public RelayCommand<StorageMember> ShowPasswordCommand => new(async member => await ShowPassword(member));
    public RelayCommand OpenCommand => new(OpenResourceNameAsLink);
    public RelayCommand RemoveCommand => new(RemoveMember);
    public RelayCommand AddCommand => new(AddMember);
    public RelayCommand EditCommand => new(EditMember);
    public RelayCommand ShowImportCommand => new(RunImport);
    public RelayCommand ShowExportCommand => new(RunExport);
    public RelayCommand ShowSettingsCommand => new(RunSettings);
    public RelayCommand<string> ChangePanelStateCommand => new(ChangePanelState);
    public RelayCommand<string> ChangeCategoryCommand => new(ChangeCategory);

    #endregion

    public MainWindowViewModel()
    {
        _windowService = new WindowService();
        _storageHandler = new StorageHandler();
        SynchronizationService = new(_storageHandler.GetEntries());
        _identityService = new(_windowService, _storageHandler, SynchronizationService);
        _identityService.Init();
    }

    private void ChangeCategory(string categoryId)
    {
        if (int.TryParse(categoryId, out var category))
            SynchronizationService.Category = (Category)category;
    }

    private void ChangePanelState(string state) => IsSidePanelExpanded = bool.Parse(state);

    private static void CopyToClipboard(string passwordHash)
    {
        TryDecryptHashToInsecurePassword(passwordHash, out var insecurePassword);

        try
        {
            ClipboardHandler.CopyTextToClipboard(insecurePassword);
        }
        finally
        {
            SecurityHelper.DecomposeString(ref insecurePassword);
        }
    }

    private static async Task ShowPassword(StorageMember member)
    {
        TryDecryptHashToInsecurePassword(member.PasswordHash, out var insecurePassword);

        try
        {
            member.DisplayInsecurePassword = insecurePassword.IsNull()
                ? "UNKNOWN"
                : string.Intern(insecurePassword);

            member.IsPasswordVisible = true;
            await Task.Delay(Constants.PasswordShowTimeInMilliseconds);
        }
        finally
        {
            member.IsPasswordVisible = false;
            SecurityHelper.DecomposeString(ref insecurePassword);
        }
    }

    private static void TryDecryptHashToInsecurePassword(string passwordHash, out string insecurePassword)
    {
        using var key = SecurityHelper.TryGetSecureKeyAsSecureStringOrNull();
        using var securePassword = AesCryptographer.DecryptToSecureString(passwordHash, key);

        insecurePassword = securePassword.IsNull()
            ? string.Empty
            : SecurityHelper.SecureStringToString(securePassword);
    }

    private void OpenResourceNameAsLink() =>
        Process.Start(SynchronizationService.SelectedItem.ResourceName.EnsureUrlHasProtocol());

    private void AddMember()
    {
        var vm = new CreateMemberViewModel();
        vm.CreatingFinished += CreateMemberViewModel_CreatingFinished;
        _windowService.ShowWindow<CreateMemberWindow>(vm);
        vm.CreatingFinished -= CreateMemberViewModel_CreatingFinished;
    }

    private void EditMember()
    {
        if (!_identityService.IsCurrentMachineVerified())
        {
            Logger.Error($"{Constants.EditLogMark}: {Constants.LocalMachineIsNotVerifiedMessage}");
            MessageBox.Show(Constants.LocalMachineIsNotVerifiedMessage, "SafeBox Export", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var vm = new EditMemberViewModel(SynchronizationService.SelectedItem);
        vm.EditingFinished += EditMemberViewModel_EditingFinished;
        _windowService.ShowWindow<EditMemberWindow>(vm);
        vm.EditingFinished -= EditMemberViewModel_EditingFinished;
    }

    private void RemoveMember()
    {
        // We should write to temporary variable because message box removes the focus from selected item of the listbox.
        var storageMember = SynchronizationService.SelectedItem;

        if (MessageBox.Show($"{storageMember}\n\n" +
                $"Are you sure you want to delete the record?",
                "SafeBox Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.Cancel)
        {
            return;
        }

        SynchronizationService.Remove(storageMember);
        _storageHandler.DeleteEntry(storageMember);

        Logger.Info($"{Constants.RemoveLogMark}: Removed storage member '{storageMember.ResourceName}'.");
    }

    private void RunImport()
    {
        var vm = new ImportViewModel();
        vm.ImportFinished += ImportViewModel_ImportFinished;
        _windowService.ShowWindow<ImportWindow>(vm);
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

        if (!_identityService.IsCurrentMachineVerified())
        {
            Logger.Error($"{Constants.ExportLogMark}: {Constants.LocalMachineIsNotVerifiedMessage}");
            MessageBox.Show(Constants.LocalMachineIsNotVerifiedMessage, "SafeBox Export", MessageBoxButton.OK, MessageBoxImage.Error);

            return;
        }

        var vm = new ExportViewModel(SynchronizationService.CloneCollection());
        _windowService.ShowWindow<ExportWindow>(vm);
    }

    private void RunSettings()
    {
        var vm = new SettingsViewModel();
        vm.SettingsChanged += SettingsViewModel_SettingsChanged;
        _windowService.ShowWindow<SettingsWindow>(vm);
        vm.SettingsChanged -= SettingsViewModel_SettingsChanged;
    }

    private void ImportViewModel_ImportFinished(ImportFinishedEventArgs e)
    {
        if (e.IsSuccess)
        {
            if (e.IsMergeRequested)
            {
                _storageHandler.AddEntries(e.ImportedCollection);
                SynchronizationService.AddRange(e.ImportedCollection);
            }
            else
            {
                _storageHandler.OverwriteStorage(e.ImportedCollection);
                SynchronizationService.Set(e.ImportedCollection);
            }

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
        if (e.StorageMember == null)
            return;

        if (!_storageHandler.IsEntryExists(e.StorageMember))
        {
            _storageHandler.AddEntry(e.StorageMember);
            SynchronizationService.Add(e.StorageMember);
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
        if (!e.HasChanges)
            return;

        _storageHandler.ReplaceEntry(e.SourceMember, e.EditedMember);
        SynchronizationService.Replace(e.SourceMember, e.EditedMember);

        Logger.Error($"{Constants.EditLogMark}: The storage member '{e.SourceMember.ResourceName}' has been successfully edited.");
        MessageBox.Show("The record has been successfully edited.", "SafeBox", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SettingsViewModel_SettingsChanged(SettingsChangedEventArgs e)
    {
        if (e.HasStorageChanged)
        {
            _storageHandler.Refresh();
            SynchronizationService.Set(_storageHandler.GetEntries());

            Logger.Info($"{Constants.SettingsLogMark}: The storage path has been changed '{_storageHandler.GetStoragePath()}'");
        }
        else if (e.IsStorageResetRequested)
        {
            _identityService.ResetIdentity();

            Logger.Info($"{Constants.SettingsLogMark}: The storage path has been changed '{_storageHandler.GetStoragePath()}'");
            MessageBox.Show("All your accounts were successfully cleared!", "SafeBox Notification", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public void DragEnter(IDropInfo dropInfo) { }

    public void DragOver(IDropInfo dropInfo)
    {
        var targetItem = (StorageMember)dropInfo.TargetItem;

        if (targetItem != null)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            dropInfo.Effects = DragDropEffects.Move;
        }
    }

    public void DragLeave(IDropInfo dropInfo) { }

    public void Drop(IDropInfo dropInfo)
    {
        var sourceMember = dropInfo.Data as StorageMember;
        var targetMember = dropInfo.TargetItem as StorageMember;

        if (sourceMember == targetMember)
            return;

        SynchronizationService.Move(sourceMember, targetMember);
        _storageHandler.OverwriteStorage(SynchronizationService.GetSourceCollection());
    }
}