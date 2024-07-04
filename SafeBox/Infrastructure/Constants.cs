namespace SafeBox.Infrastructure
{
    internal static class Constants
    {
        public const string NonBreakingSpace = "\u00A0";
        public const string Space = " ";
        public const int PasswordShowTimeInMilliseconds = 3000;
        public const string StoragePathParameterName = "StorageFullPath";

        public const string CreateLogMark = "CREATE";
        public const string RemoveLogMark = "REMOVE";
        public const string ImportLogMark = "IMPORT";
        public const string ExportLogMark = "EXPORT";
        public const string EditLogMark = "EDIT";
        public const string DeleteLogMark = "DELETE";
        public const string SettingsLogMark = "SETTINGS";
        public const string StorageHandlerLogMark = "STORAGEHANDLER";

        public const string LocalMachineIsNotVerifiedMessage = "The local machine is not verified, the export process is canceled.";
        public const string ExportEmptyStorageCollectionMessage = "Nothing to export, the process is canceled.";
        public const string CreateExistingStorageMemberMessage = "An exact element is already exists in the storage, the process is canceled.";
        public const string DecryptedDataIsEmptyMessage = "The decrypted data is empty, nothing to import.";
        public const string FieldsValidationFailedMessage = "One of the fields is not set or invalid. Fill all the required fields correctly and try again later.";
    }
}
