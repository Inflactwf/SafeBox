using SafeBox.Commands;
using SafeBox.EventArguments;
using SafeBox.Extensions;
using SafeBox.Interfaces;
using SafeBox.Models;
using SafeBox.Security;

namespace SafeBox.ViewModels
{
    public class EditMemberViewModel : ViewModelBase
    {
        #region Private Fields

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

        public EditMemberViewModel() { }

        public EditMemberViewModel(IStorageMember storageMember)
        {
            _originalMember = storageMember;
            _member = storageMember.Clone();
        }

        #region Commands

        public RelayCommand SaveCommand => new(Save);

        #endregion

        private void Save()
        {
            if (!NewPassword.IsNullOrWhiteSpace())
            {
                using var key = SecurityHelper.TryGetSecureKeyAsSecureStringOrNull();

                ((StorageMember)Member).PasswordHash = AesCryptographer.Encrypt(NewPassword, key);
                SecurityHelper.DecomposeString(ref _newPassword);
            }

            EditingFinished?.Invoke(new(HasChanges, _originalMember, _member));
        }

        private bool HasChanges => _originalMember != null && _member != null && _originalMember.CompareTo(_member) != 0;
    }
}
