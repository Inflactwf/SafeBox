using SafeBox.Extensions;
using SafeBox.Interfaces;
using System.IO;

namespace SafeBox.Handlers
{
    internal class FileHandler : IFileHandler
    {
        private readonly FileInfo _fileInfo;

        internal FileHandler(string fullFileName)
        {
            if (!fullFileName.IsNullOrWhiteSpace())
                _fileInfo = new FileInfo(fullFileName);
        }

        public string FileName => _fileInfo?.Name;

        public string FullFileName => _fileInfo?.FullName;

        public bool IsExists
        {
            get
            {
                if (_fileInfo == null)
                    return false;

                _fileInfo.Refresh();

                return _fileInfo.Exists;
            }
        }

        public string Read()
        {
            if (!IsExists)
                return string.Empty;

            using var reader = _fileInfo.OpenRead();
            using var sr = new StreamReader(reader);

            return sr.ReadToEnd();
        }

        public void Write(string text)
        {
            if (_fileInfo == null)
                return;

            _fileInfo.Refresh();

            using var fs = _fileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            using var sw = new StreamWriter(fs);
            sw.AutoFlush = true;

            sw.Write(text);
            sw.Close();
        }
    }
}
