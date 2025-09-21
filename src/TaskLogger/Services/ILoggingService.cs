using System;

namespace TaskLogger.Services
{
    public interface ILoggingService
    {
        void LogDebug(string message, params object[] args);
        void LogInfo(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(string message, params object[] args);
        void LogError(Exception ex, string message, params object[] args);
        void LogFatal(string message, params object[] args);
        void LogFatal(Exception ex, string message, params object[] args);
        string GetLogFilePath();
        string GetRecentLogs(int lines = 100);
    }
}