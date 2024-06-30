using SafeBox.Commands;
using SafeBox.EventArguments;
using SafeBox.Handlers;
using SafeBox.Infrastructure;
using System.Windows.Forms;

namespace SafeBox.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Private Fields

        private string _storageLocation = StaticResources.StorageFullPath;

        #endregion

        public delegate void OnSettingsChanged(SettingsChangedEventArgs e);
        public event OnSettingsChanged SettingsChanged;

        #region Commands

        public RelayCommand ChangeStorageLocationCommand => new(SelectStorage);
        public RelayCommand SaveCommand => new(Save);

        #endregion

        public string StorageLocation { get => _storageLocation; set => Set(ref _storageLocation, value); }

        private void SelectStorage()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Selecting the new storage file...",
                Filter = "SafeBox Storage|*.json|All Files|*.*"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
                StorageLocation = dialog.FileName.Replace(Constants.Space, Constants.NonBreakingSpace);
        }

        private void Save()
        {
            if (StaticResources.StorageFullPath != StorageLocation)
            {
                ConfigurationHandler.AddOrUpdate(Constants.StoragePathParameterName, StorageLocation);
                SettingsChanged?.Invoke(new(true));
            }
        }
    }
}
