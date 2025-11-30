using ExtendedFileHandler;
using ExtendedFileHandler.EventArguments;
using SafeBox.Infrastructure;
using SafeBox.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace SafeBox.Handlers;

public class StorageHandler
{
    private DbWorker<StorageMember> _storageWorker;

    public StorageHandler()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (_storageWorker != null)
            _storageWorker.OnError -= StorageWorker_LogMessageReceived;

        _storageWorker = new DbWorker<StorageMember>(new(ConfigurationHandler.StorageFullPath), CultureInfo.InvariantCulture);
        _storageWorker.OnError += StorageWorker_LogMessageReceived;
    }

    private static void StorageWorker_LogMessageReceived(ErrorMessageEventArgs e)
    {
        Logger.Error($"{Constants.StorageHandlerLogMark}: {e.Message}\n{e.StackTrace}");
        MessageBox.Show($"{e.Message}\nPlease, try again later.", "Extended File Handler", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public string GetStoragePath() =>
        _storageWorker.DbFileInfo.FullName;

    public IEnumerable<StorageMember> GetEntries() =>
        _storageWorker.GetEntries();

    public void AddEntry(StorageMember entry) =>
        _storageWorker.AddEntry(entry);

    public void AddEntries(IEnumerable<StorageMember> entries) =>
        _storageWorker.AddEntries(entries);

    public void ReplaceEntry(StorageMember oldEntry, StorageMember newEntry) =>
        _storageWorker.ReplaceEntry(oldEntry, newEntry);

    public void DeleteEntry(StorageMember entry) =>
        _storageWorker.DeleteEntry(entry);

    public void OverwriteStorage(IEnumerable<StorageMember> collection) =>
        _storageWorker.ReplaceAll(collection);

    public bool IsEntryExists(StorageMember entry) =>
        _storageWorker.IsEntryExists(entry);
}