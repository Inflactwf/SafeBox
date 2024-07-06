using SafeBox.Interfaces;
using System.Collections.Generic;

namespace SafeBox.EventArguments
{
    public class ImportFinishedEventArgs(bool isSuccess, string message, string fileName, IEnumerable<IStorageMember> importedCollection)
    {
        public bool IsSuccess { get; } = isSuccess;
        public string Message { get; } = message;
        public string FileName { get; } = fileName;
        public IEnumerable<IStorageMember> ImportedCollection { get; } = importedCollection;
    }
}
