using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace WindowsCleaner.Services
{
    /// <summary>
    /// Enhanced logging service with crash reporting and popup notifications
    /// </summary>
    public class LoggingService
    {
        private static LoggingService? _instance;
        private readonly string _logDirectory;
        private readonly string _crashLogPath;
        private readonly object _lockObject = new object();

        public static LoggingService Instance => _instance ??= new LoggingService();

        public event EventHandler<CrashEventArgs>? CrashDetected;

        private LoggingService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _logDirectory = Path.Combine(appDataPath, "WindowsCleanerPro", "logs");
            _crashLogPath = Path.Combine(_logDirectory, "crashes");
            
            Directory.CreateDirectory(_logDirectory);
            Directory.CreateDirectory(_crashLogPath);
        }

        public void LogInfo(string message, string? category = null)
        {
            WriteLog(LogLevel.Info, message, category);
        }

        public void LogWarning(string message, string? category = null)
        {
            WriteLog(LogLevel.Warning, message, category);
        }

        public void LogError(string message, Exception? exception = null, string? category = null)
        {
            WriteLog(LogLevel.Error, message, category, exception);
        }

        public void LogCrash(Exception exception, string? additionalInfo = null)
        {
            var crashId = Guid.NewGuid().ToString("N")[..8];
            var crashFileName = $"crash_{DateTime.Now:yyyyMMdd_HHmmss}_{crashId}.log";
            var crashFilePath = Path.Combine(_crashLogPath, crashFileName);

            try
            {
                var crashReport = CreateCrashReport(exception, additionalInfo, crashId);
                
                lock (_lockObject)
                {
                    File.WriteAllText(crashFilePath, crashReport, Encoding.UTF8);
                }

                // Trigger crash detection event
                CrashDetected?.Invoke(this, new CrashEventArgs(crashId, crashFilePath, exception));
            }
            catch (Exception ex)
            {
                // Last resort logging
                Debug.WriteLine($"Failed to write crash log: {ex.Message}");
            }
        }

        private void WriteLog(LogLevel level, string message, string? category = null, Exception? exception = null)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logFileName = $"app_{DateTime.Now:yyyyMMdd}.log";
                var logFilePath = Path.Combine(_logDirectory, logFileName);

                var logEntry = new StringBuilder();
                logEntry.AppendLine($"[{timestamp}] [{level}] {category ?? "General"}: {message}");
                
                if (exception != null)
                {
                    logEntry.AppendLine($"Exception: {exception.GetType().Name}");
                    logEntry.AppendLine($"Message: {exception.Message}");
                    logEntry.AppendLine($"StackTrace: {exception.StackTrace}");
                    
                    if (exception.InnerException != null)
                    {
                        logEntry.AppendLine($"InnerException: {exception.InnerException.GetType().Name}");
                        logEntry.AppendLine($"InnerMessage: {exception.InnerException.Message}");
                    }
                }

                lock (_lockObject)
                {
                    File.AppendAllText(logFilePath, logEntry.ToString(), Encoding.UTF8);
                }

                // Also write to debug output
                Debug.WriteLine($"[{level}] {category ?? "General"}: {message}");
            }
            catch (Exception ex)
            {
                // Fallback to debug output
                Debug.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        private string CreateCrashReport(Exception exception, string? additionalInfo, string crashId)
        {
            var report = new StringBuilder();
            
            report.AppendLine("=== WINDOWS CLEANER PRO CRASH REPORT ===");
            report.AppendLine($"Crash ID: {crashId}");
            report.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss} UTC{DateTime.Now:zzz}");
            report.AppendLine($"Application Version: {GetApplicationVersion()}");
            report.AppendLine();

            // System Information
            report.AppendLine("=== SYSTEM INFORMATION ===");
            report.AppendLine($"OS: {Environment.OSVersion}");
            report.AppendLine($"CLR Version: {Environment.Version}");
            report.AppendLine($"Machine Name: {Environment.MachineName}");
            report.AppendLine($"User: {Environment.UserName}");
            report.AppendLine($"Working Set: {Environment.WorkingSet / 1024 / 1024:N0} MB");
            report.AppendLine();

            // Exception Details
            report.AppendLine("=== EXCEPTION DETAILS ===");
            var currentException = exception;
            var level = 0;
            
            while (currentException != null)
            {
                var indent = new string(' ', level * 2);
                report.AppendLine($"{indent}Exception Type: {currentException.GetType().FullName}");
                report.AppendLine($"{indent}Message: {currentException.Message}");
                report.AppendLine($"{indent}Source: {currentException.Source}");
                
                if (!string.IsNullOrEmpty(currentException.StackTrace))
                {
                    report.AppendLine($"{indent}Stack Trace:");
                    foreach (var line in currentException.StackTrace.Split('\n'))
                    {
                        report.AppendLine($"{indent}  {line.Trim()}");
                    }
                }

                currentException = currentException.InnerException;
                level++;
                
                if (currentException != null)
                {
                    report.AppendLine($"{indent}--- Inner Exception ---");
                }
            }

            // Additional Information
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                report.AppendLine();
                report.AppendLine("=== ADDITIONAL INFORMATION ===");
                report.AppendLine(additionalInfo);
            }

            // Current Threads
            try
            {
                report.AppendLine();
                report.AppendLine("=== THREAD INFORMATION ===");
                var process = Process.GetCurrentProcess();
                report.AppendLine($"Process ID: {process.Id}");
                report.AppendLine($"Thread Count: {process.Threads.Count}");
                
                foreach (ProcessThread thread in process.Threads)
                {
                    report.AppendLine($"Thread {thread.Id}: State={thread.ThreadState}, Priority={thread.PriorityLevel}");
                }
            }
            catch (Exception ex)
            {
                report.AppendLine($"Failed to get thread information: {ex.Message}");
            }

            report.AppendLine();
            report.AppendLine("=== END OF CRASH REPORT ===");

            return report.ToString();
        }

        private string GetApplicationVersion()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                return version?.ToString() ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        public async Task<List<string>> GetRecentLogsAsync(int maxFiles = 10)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var logFiles = Directory.GetFiles(_logDirectory, "app_*.log")
                        .OrderByDescending(f => new FileInfo(f).CreationTime)
                        .Take(maxFiles)
                        .ToList();

                    return logFiles;
                }
                catch
                {
                    return new List<string>();
                }
            });
        }

        public async Task<List<string>> GetCrashReportsAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var crashFiles = Directory.GetFiles(_crashLogPath, "crash_*.log")
                        .OrderByDescending(f => new FileInfo(f).CreationTime)
                        .ToList();

                    return crashFiles;
                }
                catch
                {
                    return new List<string>();
                }
            });
        }

        public void CleanupOldLogs(int daysToKeep = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                
                // Clean regular logs
                foreach (var logFile in Directory.GetFiles(_logDirectory, "app_*.log"))
                {
                    if (File.GetCreationTime(logFile) < cutoffDate)
                    {
                        File.Delete(logFile);
                    }
                }

                // Clean crash logs (keep longer - 90 days)
                var crashCutoffDate = DateTime.Now.AddDays(-90);
                foreach (var crashFile in Directory.GetFiles(_crashLogPath, "crash_*.log"))
                {
                    if (File.GetCreationTime(crashFile) < crashCutoffDate)
                    {
                        File.Delete(crashFile);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Failed to cleanup old logs", ex, "LoggingService");
            }
        }
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public class CrashEventArgs : EventArgs
    {
        public string CrashId { get; }
        public string LogFilePath { get; }
        public Exception Exception { get; }

        public CrashEventArgs(string crashId, string logFilePath, Exception exception)
        {
            CrashId = crashId;
            LogFilePath = logFilePath;
            Exception = exception;
        }
    }
}
