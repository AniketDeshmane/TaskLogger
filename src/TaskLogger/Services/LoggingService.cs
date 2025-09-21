using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace TaskLogger.Services
{
    public class LoggingService : ILoggingService
    {
        private static readonly object _lockObject = new object();
        private readonly string _logDirectory;
        private readonly string _logFilePath;
        private static LoggingService? _instance;

        public static LoggingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LoggingService();
                }
                return _instance;
            }
        }

        public LoggingService()
        {
            // Create logs directory in user's AppData
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _logDirectory = Path.Combine(appDataPath, "TaskLogger", "Logs");
            
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            // Create log file with date
            var logFileName = $"TaskLogger_{DateTime.Now:yyyy-MM-dd}.log";
            _logFilePath = Path.Combine(_logDirectory, logFileName);

            // Log startup
            LogInfo("===========================================");
            LogInfo("TaskLogger Application Starting");
            LogInfo($"Version: {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}");
            LogInfo($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            LogInfo($"OS: {Environment.OSVersion}");
            LogInfo($".NET Version: {Environment.Version}");
            LogInfo($"Log File: {_logFilePath}");
            LogInfo("===========================================");
        }

        public void LogDebug(string message, params object[] args)
        {
            WriteLog("DEBUG", message, args);
        }

        public void LogInfo(string message, params object[] args)
        {
            WriteLog("INFO", message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            WriteLog("WARN", message, args);
        }

        public void LogError(string message, params object[] args)
        {
            WriteLog("ERROR", message, args);
        }

        public void LogError(Exception ex, string message, params object[] args)
        {
            var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
            WriteLog("ERROR", $"{formattedMessage} - Exception: {ex.GetType().Name}: {ex.Message}");
            WriteLog("ERROR", $"StackTrace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                WriteLog("ERROR", $"Inner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                WriteLog("ERROR", $"Inner StackTrace: {ex.InnerException.StackTrace}");
            }
        }

        public void LogFatal(string message, params object[] args)
        {
            WriteLog("FATAL", message, args);
        }

        public void LogFatal(Exception ex, string message, params object[] args)
        {
            var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
            WriteLog("FATAL", $"{formattedMessage} - Exception: {ex.GetType().Name}: {ex.Message}");
            WriteLog("FATAL", $"StackTrace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                WriteLog("FATAL", $"Inner Exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                WriteLog("FATAL", $"Inner StackTrace: {ex.InnerException.StackTrace}");
            }
        }

        public string GetLogFilePath()
        {
            return _logFilePath;
        }

        public string GetRecentLogs(int lines = 100)
        {
            try
            {
                lock (_lockObject)
                {
                    if (File.Exists(_logFilePath))
                    {
                        var allLines = File.ReadAllLines(_logFilePath);
                        var recentLines = allLines.Skip(Math.Max(0, allLines.Length - lines)).ToArray();
                        return string.Join(Environment.NewLine, recentLines);
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error reading log file: {ex.Message}";
            }

            return "No logs available.";
        }

        private void WriteLog(string level, string message, params object[] args)
        {
            try
            {
                var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level,-5}] [{Thread.CurrentThread.ManagedThreadId,3}] {formattedMessage}";

                lock (_lockObject)
                {
                    // Also write to console for debugging
                    Console.WriteLine(logEntry);
                    
                    // Write to file
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // If we can't log, at least try to write to console
                Console.WriteLine($"LOGGING ERROR: {ex.Message}");
            }
        }

        // Clean up old log files (keep last 30 days)
        public void CleanupOldLogs()
        {
            try
            {
                LogInfo("Starting log cleanup...");
                var cutoffDate = DateTime.Now.AddDays(-30);
                var logFiles = Directory.GetFiles(_logDirectory, "TaskLogger_*.log");
                
                foreach (var file in logFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        LogInfo($"Deleted old log file: {fileInfo.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Error during log cleanup");
            }
        }
    }
}