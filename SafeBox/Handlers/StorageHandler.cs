using ExtendedFileHandler;
using ExtendedFileHandler.EventArguments;
using SafeBox.Infrastructure;
using SafeBox.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
        }

        public static string GetStoragePath() => storageWorker.DbFileInfo.FullName;

        public static IEnumerable<StorageMember> GetEntries() => storageWorker.GetEntries();

        public static StorageMember GetEntry(string login) => storageWorker.GetEntry(x => x.Login == login);

        public static void SaveEntry(StorageMember entry) => storageWorker.SaveEntry(entry);

        public static void AddEntry(StorageMember entry) => storageWorker.AddEntry(entry);

        public static void EditEntry(StorageMember entry, Action<StorageMember> action) => storageWorker.EditEntry(entry, action);

        public static void DeleteEntry(StorageMember entry) => storageWorker.DeleteEntry(entry);

        public static void OverwriteStorage(IEnumerable<StorageMember> collection) => storageWorker.ReplaceAll(collection);

        public static bool IsEntryExists(StorageMember entry) => storageWorker.IsEntryExists(entry);
    }
}
