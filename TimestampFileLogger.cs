using DiscordRPC.Logging;
using System;
using System.IO;

namespace CustomRPC
{
    public class TimestampFileLogger : ILogger
    {
        public LogLevel Level { get; set; }

        public string LogsPath { get; set; }

        private string LogExt = ".log";

        private object filelock;

        public TimestampFileLogger(string path)
            : this(path, LogLevel.Trace) { }

        public TimestampFileLogger(string path, LogLevel level)
        {
            Level = level;
            LogsPath = path;
            filelock = new object();

            Directory.CreateDirectory(path);

        }

        private void Log(string logType, LogLevel logLevel, string message, params object[] args)
        {
            if (Level > logLevel) return;

            lock (filelock)
            {
                try
                {
                    File.AppendAllText(Path.Combine(LogsPath, DateTime.Now.ToString("yyyy-MM-dd") + LogExt), "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + logType + ": " + (args.Length > 0 ? string.Format(message, args) : message) + "\r\n");
                }
                catch { }
            }
        }

        public void Trace(string message, params object[] args)
        {
            Log("TRACE", LogLevel.Trace, message, args);
        }

        public void Info(string message, params object[] args)
        {
            Log(" INFO", LogLevel.Info, message, args);
        }

        public void Warning(string message, params object[] args)
        {
            Log(" WARN", LogLevel.Warning, message, args);
        }

        public void Error(string message, params object[] args)
        {
            Log("ERROR", LogLevel.Error, message, args);
        }
    }
}