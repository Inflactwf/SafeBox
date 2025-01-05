using SafeBox.Commands;
using SafeBox.EventArguments;
using SafeBox.Interfaces;
using SafeBox.Models;
using SafeBox.Security;

namespace SafeBox.ViewModels
{
    public class CreateMemberViewModel : ViewModelBase
    {
        #region Private Fields

        private IStorageMember _member = new StorageMember();

        #endregion

        #region Binding Properties

        public IStorageMember Member => _member;

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
            var storageMember = Member as StorageMember;

            using var key = SecurityHelper.TryGetSecureKeyAsSecureStringOrNull();
            storageMember.PasswordHash = AesCryptographer.Encrypt(storageMember.PasswordHash, key);
        }
    }
}
