using SafeBox.Commands;
using SafeBox.EventArguments;
using SafeBox.Extensions;
using SafeBox.Interfaces;
using SafeBox.Models;
using SafeBox.Security;
using System.Security;

namespace SafeBox.ViewModels
{
    public class EditMemberViewModel : ViewModelBase
    {
        #region Private Fields

        private ICryptographer<SecureString> nativeCryptographer;
        private IStorageMember _member;
        private IStorageMember _originalMember;
        private string _newPassword;

        #endregion

        #region Public Properties

        public IStorageMember Member => _member;

        public string NewPassword { get => _newPassword; set => Set(ref _newPassword, value); }

        #endregion

        public delegate void OnEditingMemberFinished(EditingMemberFinishedEventArgs e);
        public event OnEditingMemberFinished EditingFinished;

        #region Commands

        public RelayCommand SaveCommand => new(Save);

        #endregion

        private void Save()
        {
            if (!NewPassword.IsNullOrWhiteSpace())
            {
                ((StorageMember)Member).PasswordHash = nativeCryptographer.Encrypt(NewPassword);
                SecurityHelper.DecomposeString(ref _newPassword);
            }

            EditingFinished?.Invoke(new(HasChanges, _originalMember, _member));
        }

        private bool HasChanges =>
            _originalMember.CompareTo(_member) != 0;

        public void AttachNativeCryptographer(ICryptographer<SecureString> cryptographer) =>
            nativeCryptographer = cryptographer;

        public void AttachStorageMember(IStorageMember storageMember)
        {
            _originalMember = storageMember;
            _member = storageMember.Clone();
        }
    }
}
