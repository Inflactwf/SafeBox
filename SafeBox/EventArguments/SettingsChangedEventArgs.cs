namespace SafeBox.EventArguments
{
    public class SettingsChangedEventArgs(bool hasStorageChanged)
    {
        public bool HasStorageChanged { get; } = hasStorageChanged;
    }
}
