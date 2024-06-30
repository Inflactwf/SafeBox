using SafeBox.Models;
using System.Collections.ObjectModel;

namespace SafeBox.EventArguments
{
    public class ImportFinishedEventArgs(bool isSuccess, string message, string fileName, ObservableCollection<StorageMember> importedCollection)
    {
        public bool IsSuccess { get; } = isSuccess;
        public string Message { get; } = message;
        public string FileName { get; } = fileName;
        public ObservableCollection<StorageMember> ImportedCollection { get; } = importedCollection;
    }
}
