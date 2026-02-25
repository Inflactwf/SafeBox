using SafeBox.Commands;
using SafeBox.EventArguments;
using SafeBox.Models;
using SafeBox.Security;

namespace SafeBox.ViewModels;

public class CreateMemberViewModel : ViewModelBase
{
    #region Binding Properties

    public StorageMember Member { get; } = new(); //TODO: Finish this when we got a different credential types

    #endregion

    #region Commands

    public RelayCommand CreateCommand => new(CreateMember);

    #endregion

    public delegate void OnCreatingMemberFinished(CreatingMemberFinishedEventArgs e);
    public event OnCreatingMemberFinished CreatingFinished;

    private void CreateMember()
    {
        EncryptStorageMember();
        CreatingFinished?.Invoke(new(Member));
    }

    private void EncryptStorageMember()
    {
        using var key = SecurityHelper.TryGetSecureKeyAsSecureStringOrNull();
        Member.PasswordHash = AesCryptographer.Encrypt(Member.PasswordHash, key);
    }
}