using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using WindowsCleaner.ViewModels;
using WindowsCleaner.Views;
using WindowsCleaner.Models;

namespace WindowsCleaner
{
    public partial class App : Application
    {
        // Import DPI awareness functions
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(int value);

        protected override void OnStartup(StartupEventArgs e)
        {
            // Set DPI awareness before UI initialization
            SetDpiAwareness();
            
            base.OnStartup(e);

            // Set up global exception handling
            SetupExceptionHandling();

            // Set up the main window
            try
            {
                var mainWindow = new MainWindow();
                var viewModel = new MainViewModel();
                mainWindow.DataContext = viewModel;
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                HandleCriticalError(ex, "Failed to start application");
            }
        }

        /// <summary>
        /// Sets DPI awareness for the application to prevent blurry UI
        /// </summary>
        private void SetDpiAwareness()
        {
            try
            {
                // Try to set per-monitor DPI awareness (Windows 8.1 and later)
                if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor >= 3)
                {
                    SetProcessDpiAwareness(2); // PROCESS_PER_MONITOR_DPI_AWARE
                }
                else
                {
                    // Fallback to system DPI awareness for older Windows versions
                    SetProcessDPIAware();
                }
            }
            catch
            {
                // If DPI awareness setting fails, continue without it
                // This ensures the app still starts on systems where these APIs are not available
            }
        }

        private void SetupExceptionHandling()
        {
            // Handle WPF exceptions
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            
            // Handle all other exceptions
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            
            // Handle async exceptions
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception, "WPF Dispatcher Exception");
                e.Handled = true;
            }
            catch (Exception ex)
            {
                HandleCriticalError(ex, "Error in exception handler");
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is Exception exception)
                {
                    HandleException(exception, "Unhandled Domain Exception");
                }
            }
            catch (Exception ex)
            {
                HandleCriticalError(ex, "Error in unhandled exception handler");
            }
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception, "Unobserved Task Exception");
                e.SetObserved();
            }
            catch (Exception ex)
            {
                HandleCriticalError(ex, "Error in task exception handler");
            }
        }

        private void HandleException(Exception exception, string context)
        {
            try
            {
                // Create crash info
                var crashInfo = new CrashInfo
                {
                    CrashId = Guid.NewGuid().ToString("N")[..8],
                    Timestamp = DateTime.Now,
                    ExceptionType = exception.GetType().Name,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace ?? "",
                    AdditionalInfo = $"Context: {context}\nInner Exception: {exception.InnerException?.Message}",
                    ApplicationVersion = GetApplicationVersion(),
                    OperatingSystem = Environment.OSVersion.ToString(),
                    IsReported = false
                };

                // Write crash log (simplified without LoggingService dependency)
                var crashLogPath = WriteCrashLog(crashInfo, exception);

                // Show crash report window
                ShowCrashReportWindow(crashInfo, crashLogPath);
            }
            catch (Exception ex)
            {
                HandleCriticalError(ex, "Failed to handle exception properly");
            }
        }

        private void HandleCriticalError(Exception exception, string context)
        {
            try
            {
                var message = $"Critical Error in Windows Cleaner Pro\n\n" +
                             $"Context: {context}\n" +
                             $"Error: {exception.Message}\n\n" +
                             $"The application will now close. Please restart the program.";

                MessageBox.Show(message, "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                // If even MessageBox fails, we're in serious trouble
            }
            finally
            {
                Environment.Exit(1);
            }
        }

        private string WriteCrashLog(CrashInfo crashInfo, Exception exception)
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var crashLogDir = System.IO.Path.Combine(appDataPath, "WindowsCleanerPro", "crashes");
                System.IO.Directory.CreateDirectory(crashLogDir);

                var fileName = $"crash_{DateTime.Now:yyyyMMdd_HHmmss}_{crashInfo.CrashId}.log";
                var filePath = System.IO.Path.Combine(crashLogDir, fileName);

                var logContent = $"=== WINDOWS CLEANER PRO CRASH REPORT ===\n" +
                               $"Crash ID: {crashInfo.CrashId}\n" +
                               $"Timestamp: {crashInfo.Timestamp:yyyy-MM-dd HH:mm:ss}\n" +
                               $"Application Version: {crashInfo.ApplicationVersion}\n" +
                               $"Operating System: {crashInfo.OperatingSystem}\n\n" +
                               $"Exception Type: {crashInfo.ExceptionType}\n" +
                               $"Message: {crashInfo.Message}\n\n" +
                               $"Stack Trace:\n{crashInfo.StackTrace}\n\n" +
                               $"Additional Info:\n{crashInfo.AdditionalInfo}\n\n" +
                               $"=== END OF CRASH REPORT ===";

                System.IO.File.WriteAllText(filePath, logContent);
                return filePath;
            }
            catch
            {
                return ""; // Failed to write log
            }
        }

        private void ShowCrashReportWindow(CrashInfo crashInfo, string logFilePath)
        {
            try
            {
                // Dispatch to UI thread if needed
                if (Current.Dispatcher.CheckAccess())
                {
                    var crashWindow = new Views.CrashReportWindow(crashInfo, logFilePath);
                    crashWindow.ShowDialog();
                }
                else
                {
                    Current.Dispatcher.Invoke(() =>
                    {
                        var crashWindow = new Views.CrashReportWindow(crashInfo, logFilePath);
                        crashWindow.ShowDialog();
                    });
                }
            }
            catch (Exception)
            {
                // If crash window fails, fall back to simple message box
                var message = $"Windows Cleaner Pro has crashed.\n\n" +
                             $"Error: {crashInfo.Message}\n" +
                             $"Crash ID: {crashInfo.CrashId}\n\n" +
                             $"Log file: {logFilePath}";

                MessageBox.Show(message, "Application Crash", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetApplicationVersion()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                return version?.ToString() ?? "1.0.0.0";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
