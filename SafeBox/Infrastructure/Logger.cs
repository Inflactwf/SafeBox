using NLog;
using NLog.Config;
using NLog.Targets;

namespace SafeBox.Infrastructure
{
    internal static class Logger
    {
        private static readonly NLog.Logger logger;

        static Logger()
        {
            var fileTarget = new FileTarget
            {
                Name = "SafeBox",
                FileName = "SafeBox.log",
                MaxArchiveFiles = 30,
                MaxArchiveDays = 30,
                ArchiveNumbering = ArchiveNumberingMode.Date,
                ArchiveFileName = @"Logs\SafeBox_{#}.log",
                ArchiveDateFormat = "dd-MM-yyyy",
                ArchiveEvery = FileArchivePeriod.Day,
                AutoFlush = true,
                KeepFileOpen = true
            };

            var config = new LoggingConfiguration();
            config.AddTarget(fileTarget);
            config.LoggingRules.Add(new("*", LogLevel.Trace, fileTarget));
            LogManager.Configuration = config;

            logger = LogManager.GetCurrentClassLogger();
        }

        internal static void Trace(string message) =>
            logger.Log(LogLevel.Trace, message);

        internal static void Off(string message) =>
            logger.Log(LogLevel.Off, message);

        internal static void Info(string message) =>
            logger.Log(LogLevel.Info, message);

        internal static void Debug(string message) =>
            logger.Log(LogLevel.Debug, message);

        internal static void Warn(string message) =>
            logger.Log(LogLevel.Warn, message);

        internal static void Error(string message) =>
            logger.Log(LogLevel.Error, message);

        internal static void Fatal(string message) =>
            logger.Log(LogLevel.Fatal, message);

        internal static void Shutdown() =>
            LogManager.Shutdown();
    }
}
