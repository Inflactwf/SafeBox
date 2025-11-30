namespace SafeBox.EventArguments;

public class SettingsChangedEventArgs(bool hasStorageChanged, bool isStorageResetRequested)
{
    public bool HasStorageChanged { get; } = hasStorageChanged;
    public bool IsStorageResetRequested { get; } = isStorageResetRequested;
}