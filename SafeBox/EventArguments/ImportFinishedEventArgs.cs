using SafeBox.Models;
using System.Collections.Generic;

namespace SafeBox.EventArguments;

public class ImportFinishedEventArgs(bool isSuccess, string message, string fileName, IEnumerable<StorageMember> importedCollection, bool isMergeRequested)
{
    public bool IsSuccess { get; } = isSuccess;
    public string Message { get; } = message;
    public string FileName { get; } = fileName;
    public IEnumerable<StorageMember> ImportedCollection { get; } = importedCollection;
    public bool IsMergeRequested { get; } = isMergeRequested;
}