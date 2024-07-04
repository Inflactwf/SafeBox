using ExtendedFileHandler;
using ExtendedFileHandler.EventArguments;
using SafeBox.Infrastructure;
using SafeBox.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace SafeBox.Handlers
{
    public static class StorageHandler
    {
        private static DbWorker<StorageMember> storageWorker;

        static StorageHandler()
        {
            Refresh();
        }

        public static void Refresh()
        {
            if (storageWorker != null)
                storageWorker.OnError -= StorageWorker_LogMessageReceived;

            storageWorker = new DbWorker<StorageMember>(new(StaticResources.StorageFullPath), CultureInfo.InvariantCulture);
            storageWorker.OnError += StorageWorker_LogMessageReceived;
        }

        private static void StorageWorker_LogMessageReceived(ErrorMessageEventArgs e)
        {
            Logger.Error($"{Constants.StorageHandlerLogMark}: {e.Message}\n{e.StackTrace}");
            MessageBox.Show($"{e.Message}\nPlease, try again later.", "Extended File Handler", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static string GetStoragePath() => storageWorker.DbFileInfo.FullName;

        public static IEnumerable<StorageMember> GetEntries() => storageWorker.GetEntries();

        public static void AddEntry(StorageMember entry) => storageWorker.AddEntry(entry);

        public static void ReplaceEntry(StorageMember oldEntry, StorageMember newEntry) => storageWorker.ReplaceEntry(oldEntry, newEntry);

        public static void DeleteEntry(StorageMember entry) => storageWorker.DeleteEntry(entry);

        public static void OverwriteStorage(IEnumerable<StorageMember> collection) => storageWorker.ReplaceAll(collection);

        public static bool IsEntryExists(StorageMember entry) => storageWorker.IsEntryExists(entry);
    }
}
