using SafeBox.Commands;
using SafeBox.EventArguments;
using SafeBox.Interfaces;
using SafeBox.Models;
using System.Security;

namespace SafeBox.ViewModels
{
    public class CreateMemberViewModel : ViewModelBase
    {
        #region Private Fields

        private readonly IStorageMember _member = new StorageMember();
        private ICryptographer<SecureString> nativeCryptographer;

        #endregion

        #region Binding Properties

        public IStorageMember Member => _member;

        #endregion

        #region Commands

        public RelayCommand CreateCommand => new(CreateMember);

        #endregion

        public delegate void OnCreatingMemberFinished(CreatingMemberFinishedEventArgs e);
        public event OnCreatingMemberFinished CreatingFinished;

        public void AttachNativeCryptographer(ICryptographer<SecureString> cryptographer) =>
            nativeCryptographer = cryptographer;

        private void CreateMember()
        {
            EncryptStorageMember();
            CreatingFinished?.Invoke(new(Member));
        }

        private void EncryptStorageMember()
        {
            var storageMember = Member as StorageMember;
            storageMember.PasswordHash = nativeCryptographer.Encrypt(storageMember.PasswordHash);
        }
    }
}
