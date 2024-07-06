using ExtendedFileHandler;
using ExtendedFileHandler.EventArguments;
using SafeBox.Infrastructure;
using SafeBox.Interfaces;
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

        public static string GetStoragePath() =>
            storageWorker.DbFileInfo.FullName;

        public static IEnumerable<IStorageMember> GetEntries() =>
            storageWorker.GetEntries();

        public static void AddEntry(IStorageMember entry) =>
            storageWorker.AddEntry((StorageMember)entry);

        public static void ReplaceEntry(IStorageMember oldEntry, IStorageMember newEntry) =>
            storageWorker.ReplaceEntry((StorageMember)oldEntry, (StorageMember)newEntry);

        public static void DeleteEntry(IStorageMember entry) =>
            storageWorker.DeleteEntry((StorageMember)entry);

        public static void OverwriteStorage(IEnumerable<IStorageMember> collection) =>
            storageWorker.ReplaceAll((IEnumerable<StorageMember>)collection);

        public static bool IsEntryExists(IStorageMember entry) =>
            storageWorker.IsEntryExists((StorageMember)entry);
    }
}
