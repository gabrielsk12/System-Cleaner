using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WindowsCleaner.Models;

namespace WindowsCleaner.Views
{
    public partial class CrashReportWindow : Window
    {
        private readonly CrashInfo _crashInfo;
        private readonly string _logFilePath;

        public CrashReportWindow(CrashInfo crashInfo, string logFilePath)
        {
            InitializeComponent();
            _crashInfo = crashInfo;
            _logFilePath = logFilePath;
            
            SetupCrashDetails();
        }

        private void SetupCrashDetails()
        {
            // Set crash information
            CrashIdText.Text = _crashInfo.CrashId;
            TimestampText.Text = _crashInfo.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            ErrorTypeText.Text = _crashInfo.ExceptionType;
            LogFileText.Text = Path.GetFileName(_logFilePath);

            // Set error details
            var errorDetails = $"Error Message:\n{_crashInfo.Message}\n\n";
            
            if (!string.IsNullOrEmpty(_crashInfo.StackTrace))
            {
                errorDetails += $"Stack Trace:\n{_crashInfo.StackTrace}\n\n";
            }
            
            if (!string.IsNullOrEmpty(_crashInfo.AdditionalInfo))
            {
                errorDetails += $"Additional Information:\n{_crashInfo.AdditionalInfo}";
            }

            ErrorDetailsText.Text = errorDetails;
        }

        private void ViewLogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(_logFilePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "notepad.exe",
                        Arguments = $"\"{_logFilePath}\"",
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show(
                        "Log file not found. It may have been moved or deleted.",
                        "File Not Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open log file: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CopyDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var details = $"Windows Cleaner Pro Crash Report\n" +
                             $"================================\n" +
                             $"Crash ID: {_crashInfo.CrashId}\n" +
                             $"Timestamp: {_crashInfo.Timestamp:yyyy-MM-dd HH:mm:ss}\n" +
                             $"Application Version: {_crashInfo.ApplicationVersion}\n" +
                             $"Operating System: {_crashInfo.OperatingSystem}\n" +
                             $"Error Type: {_crashInfo.ExceptionType}\n\n" +
                             $"Error Message:\n{_crashInfo.Message}\n\n" +
                             $"Stack Trace:\n{_crashInfo.StackTrace}\n\n" +
                             $"Additional Information:\n{_crashInfo.AdditionalInfo}";

                Clipboard.SetText(details);
                
                MessageBox.Show(
                    "Crash details copied to clipboard successfully!",
                    "Copied",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to copy details: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SendReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SendReportCheckBox.IsChecked == true)
                {
                    // In a real application, you would send the crash report to your servers
                    // For now, we'll just mark it as reported
                    _crashInfo.IsReported = true;
                    
                    MessageBox.Show(
                        "Thank you for helping improve Windows Cleaner Pro!\n\n" +
                        "Your crash report has been prepared. In the full version, " +
                        "this would be automatically sent to our developers.",
                        "Report Prepared",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                HandleApplicationRestart();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An error occurred while processing the crash report: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                CloseWindow();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            HandleApplicationRestart();
        }

        private void HandleApplicationRestart()
        {
            try
            {
                if (RestartAppCheckBox.IsChecked == true)
                {
                    // Get the current application path
                    var applicationPath = Environment.ProcessPath;
                    
                    if (!string.IsNullOrEmpty(applicationPath) && File.Exists(applicationPath))
                    {
                        // Start a new instance of the application
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = applicationPath,
                            UseShellExecute = true,
                            WorkingDirectory = Path.GetDirectoryName(applicationPath)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to restart application: {ex.Message}\n\n" +
                    "You can manually restart Windows Cleaner Pro from the Start Menu.",
                    "Restart Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            finally
            {
                CloseWindow();
            }
        }

        private void CloseWindow()
        {
            try
            {
                // Save crash report status if needed
                // In a real application, you might want to save this to settings
                
                DialogResult = true;
                Close();
                
                // Exit the current application instance
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                // Last resort - force close
                Debug.WriteLine($"Error closing crash report window: {ex.Message}");
                Environment.Exit(1);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Make sure the window is visible and in front
            this.Topmost = true;
            this.Activate();
            this.Focus();
        }
    }
}
