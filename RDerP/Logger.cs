using System;
using System.Diagnostics;

namespace RDerP
{
    public static class Logger
    {
        private const string SOURCE = "RDerP";

        private static void WriteLog(string message, EventLogEntryType logType)
        {
            EventLog.WriteEntry(SOURCE, message, logType);
        }

        public static void LogInfo(string message)
        {
            WriteLog(message, EventLogEntryType.Information);
        }

        public static void LogWarn(string message, Exception ex = null)
        {
            WriteLog(ex != null ? $"{message}: {ex}" : message, EventLogEntryType.Warning);
        }

        public static void LogError(string message, Exception ex = null)
        {
            WriteLog(ex != null ? $"{message}: {ex}" : message, EventLogEntryType.Error);
        }

    }
}
