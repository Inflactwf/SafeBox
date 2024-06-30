namespace SafeBox.Interfaces
{
    internal interface IFileHandler
    {
        string Read();
        void Write(string text);
        public string FileName { get; }
        public string FullFileName { get; }
    }
}
