using SafeBox.Commands;
using SafeBox.EventArguments;
using SafeBox.Extensions;
using SafeBox.Models;
using SafeBox.Security;

namespace SafeBox.ViewModels;

public class EditMemberViewModel : ViewModelBase
{
    #region Private Fields

    private readonly StorageMember _originalMember;
    private string _newPassword;

    #endregion

    #region Public Properties

    public StorageMember Member { get; }

    public string NewPassword { get => _newPassword; set => Set(ref _newPassword, value); }

    #endregion

    public delegate void OnEditingMemberFinished(EditingMemberFinishedEventArgs e);
    public event OnEditingMemberFinished EditingFinished;

    public EditMemberViewModel() { }

    public EditMemberViewModel(StorageMember storageMember)
    {
        _originalMember = storageMember;
        Member = storageMember.Clone();
    }

    #region Commands

    public RelayCommand SaveCommand => new(Save);

    #endregion

    private void Save()
    {
        if (!NewPassword.IsNull())
        {
            using var key = SecurityHelper.TryGetSecureKeyAsSecureStringOrNull();

            Member.PasswordHash = AesCryptographer.Encrypt(NewPassword, key);
            SecurityHelper.DecomposeString(ref _newPassword);
        }

        EditingFinished?.Invoke(new(HasChanges, _originalMember, Member));
    }

    private bool HasChanges => _originalMember != null && Member != null && _originalMember.CompareTo(Member) != 0;
}