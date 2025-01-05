namespace SafeBox.Infrastructure
{
    internal static class Constants
    {
        public const string NonBreakingSpace = "\u00A0";
        public const string Space = " ";
        public const int PasswordShowTimeInMilliseconds = 3000;
        public const string SecureKeyParameterName = "SecureKey";
        public const string StoragePathParameterName = "StorageFullPath";

        public const string GeneralLogMark = "GENERAL";
        public const string CreateLogMark = "CREATE";
        public const string RemoveLogMark = "REMOVE";
        public const string ImportLogMark = "IMPORT";
        public const string ExportLogMark = "EXPORT";
        public const string EditLogMark = "EDIT";
        public const string DeleteLogMark = "DELETE";
        public const string SettingsLogMark = "SETTINGS";
        public const string StorageHandlerLogMark = "STORAGEHANDLER";
        public const string DpapiLogMark = "DPAPI";
        public const string AesLogMark = "AES";

        public const string LocalMachineIsNotVerifiedMessage = "The local machine is not verified, the export process is canceled.";
        public const string ExportEmptyStorageCollectionMessage = "Nothing to export, the process is canceled.";
        public const string CreateExistingStorageMemberMessage = "An exact element is already exists in the storage, the process is canceled.";
    }
}
