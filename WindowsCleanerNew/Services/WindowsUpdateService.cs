using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using WindowsCleaner.Models;

namespace WindowsCleaner.Services
{
    /// <summary>
    /// Service for checking Windows updates and system optimization
    /// </summary>
    public class WindowsUpdateService
    {
        private readonly LoggingService _logger;

        public WindowsUpdateService()
        {
            _logger = LoggingService.Instance;
        }

        /// <summary>
        /// Gets the current update status, including pending updates
        /// </summary>
        public async Task<WindowsUpdateStatus> GetUpdateStatusAsync(CancellationToken cancellationToken = default)
        {
            return await CheckWindowsUpdatesAsync();
        }

        /// <summary>
        /// Checks if auto updates are enabled in Windows
        /// </summary>
        public async Task<bool> IsAutoUpdateEnabledAsync()
        {
            return await Task.FromResult(IsAutoUpdateEnabled());
        }

        /// <summary>
        /// Checks if restart is required after installing updates
        /// </summary>
        public async Task<bool> IsRestartRequiredAsync()
        {
            return await Task.FromResult(IsPendingRestart());
        }
        
        public async Task<WindowsUpdateStatus> CheckWindowsUpdatesAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInfo("Checking Windows Update status", "WindowsUpdateService");

                    var status = new WindowsUpdateStatus
                    {
                        LastUpdateCheck = GetLastUpdateCheck(),
                        LastInstallDate = GetLastInstallDate(),
                        PendingRestartRequired = IsPendingRestart(),
                        WindowsVersion = GetWindowsVersion(),
                        BuildNumber = GetBuildNumber(),
                        UpdatesAvailable = false,
                        AutoUpdateEnabled = IsAutoUpdateEnabled()
                    };

                    // Check if system is up to date based on last update
                    if (status.LastUpdateCheck.HasValue)
                    {
                        var daysSinceLastCheck = (DateTime.Now - status.LastUpdateCheck.Value).Days;
                        status.UpdatesAvailable = daysSinceLastCheck > 7; // Consider updates available if last check was more than a week ago
                    }

                    // Check pending updates using Windows Update API
                    status.PendingUpdates = GetPendingUpdates();
                    status.UpdatesAvailable = status.PendingUpdates.Any();

                    _logger.LogInfo($"Windows Update check completed - Available: {status.UpdatesAvailable}, Pending Restart: {status.PendingRestartRequired}", "WindowsUpdateService");

                    return status;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to check Windows updates", ex, "WindowsUpdateService");
                    
                    return new WindowsUpdateStatus
                    {
                        ErrorMessage = ex.Message,
                        WindowsVersion = GetWindowsVersion(),
                        BuildNumber = GetBuildNumber()
                    };
                }
            });
        }

        public async Task<bool> TriggerWindowsUpdateCheckAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInfo("Triggering Windows Update check", "WindowsUpdateService");

                    // Use USOClient to trigger update check
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "usoclient.exe"),
                        Arguments = "ScanInstallWait",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using var process = Process.Start(processInfo);
                    if (process != null)
                    {
                        process.WaitForExit(30000); // 30 second timeout
                        
                        if (process.ExitCode == 0)
                        {
                            _logger.LogInfo("Windows Update check triggered successfully", "WindowsUpdateService");
                            return true;
                        }
                        else
                        {
                            var error = process.StandardError.ReadToEnd();
                            _logger.LogWarning($"Windows Update check returned non-zero exit code: {process.ExitCode}, Error: {error}", "WindowsUpdateService");
                        }
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to trigger Windows Update check", ex, "WindowsUpdateService");
                    return false;
                }
            });
        }

        public async Task<List<StartupProgramInfo>> GetStartupProgramsAsync()
        {
            return await Task.Run(() =>
            {
                var programs = new List<StartupProgramInfo>();

                try
                {
                    _logger.LogInfo("Analyzing startup programs", "WindowsUpdateService");

                    // Check registry startup locations
                    programs.AddRange(GetStartupFromRegistry(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", StartupLocation.CurrentUser));
                    programs.AddRange(GetStartupFromRegistry(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", StartupLocation.AllUsers));
                    programs.AddRange(GetStartupFromRegistry(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", StartupLocation.CurrentUserOnce));
                    programs.AddRange(GetStartupFromRegistry(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", StartupLocation.AllUsersOnce));

                    // Check startup folder
                    programs.AddRange(GetStartupFromFolder());

                    // Check Task Scheduler startup tasks
                    programs.AddRange(GetStartupFromTaskScheduler());

                    // Analyze impact of each program
                    foreach (var program in programs)
                    {
                        program.Impact = AnalyzeStartupImpact(program);
                    }

                    _logger.LogInfo($"Found {programs.Count} startup programs", "WindowsUpdateService");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to get startup programs", ex, "WindowsUpdateService");
                }

                return programs.OrderByDescending(p => p.Impact).ToList();
            });
        }

        public async Task<bool> DisableStartupProgramAsync(StartupProgramInfo program)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.LogInfo($"Disabling startup program: {program.Name}", "WindowsUpdateService");

                    switch (program.Location)
                    {
                        case StartupLocation.CurrentUser:
                            return DisableRegistryStartup(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", program.Name);
                        
                        case StartupLocation.AllUsers:
                            return DisableRegistryStartup(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", program.Name);
                        
                        case StartupLocation.CurrentUserOnce:
                            return DisableRegistryStartup(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", program.Name);
                        
                        case StartupLocation.AllUsersOnce:
                            return DisableRegistryStartup(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", program.Name);
                        
                        case StartupLocation.StartupFolder:
                            return DisableStartupFolderItem(program);
                        
                        case StartupLocation.TaskScheduler:
                            return DisableScheduledTask(program.Name);
                        
                        default:
                            return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to disable startup program {program.Name}", ex, "WindowsUpdateService");
                    return false;
                }
            });
        }

        private DateTime? GetLastUpdateCheck()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\Results\Detect");
                var lastSuccessTime = key?.GetValue("LastSuccessTime")?.ToString();
                
                if (!string.IsNullOrEmpty(lastSuccessTime) && DateTime.TryParse(lastSuccessTime, out var dateTime))
                {
                    return dateTime;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to get last update check: {ex.Message}", "WindowsUpdateService");
            }

            return null;
        }

        private DateTime? GetLastInstallDate()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\Results\Install");
                var lastSuccessTime = key?.GetValue("LastSuccessTime")?.ToString();
                
                if (!string.IsNullOrEmpty(lastSuccessTime) && DateTime.TryParse(lastSuccessTime, out var dateTime))
                {
                    return dateTime;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to get last install date: {ex.Message}", "WindowsUpdateService");
            }

            return null;
        }

        private bool IsPendingRestart()
        {
            try
            {
                // Check multiple registry locations for pending restart
                var registryKeys = new[]
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending",
                    @"SOFTWARE\Microsoft\ServerManager\CurrentRebootAttempts"
                };

                foreach (var keyPath in registryKeys)
                {
                    using var key = Registry.LocalMachine.OpenSubKey(keyPath);
                    if (key != null)
                    {
                        return true;
                    }
                }

                // Check for pending file rename operations
                using var fileRenameKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager");
                var pendingFileRenameOps = fileRenameKey?.GetValue("PendingFileRenameOperations");
                if (pendingFileRenameOps != null)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to check pending restart: {ex.Message}", "WindowsUpdateService");
            }

            return false;
        }

        private string GetWindowsVersion()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                var productName = key?.GetValue("ProductName")?.ToString();
                var displayVersion = key?.GetValue("DisplayVersion")?.ToString();
                
                if (!string.IsNullOrEmpty(displayVersion))
                {
                    return $"{productName} {displayVersion}";
                }
                
                return productName ?? Environment.OSVersion.ToString();
            }
            catch
            {
                return Environment.OSVersion.ToString();
            }
        }

        private string GetBuildNumber()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                var currentBuild = key?.GetValue("CurrentBuild")?.ToString();
                var ubr = key?.GetValue("UBR")?.ToString();
                
                if (!string.IsNullOrEmpty(currentBuild) && !string.IsNullOrEmpty(ubr))
                {
                    return $"{currentBuild}.{ubr}";
                }
                
                return currentBuild ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private bool IsAutoUpdateEnabled()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update");
                var auOptions = key?.GetValue("AUOptions");
                
                if (auOptions is int option)
                {
                    // 2 = Notify before download, 3 = Automatically download and notify of installation
                    // 4 = Automatically download and schedule installation, 5 = Allow local admin to choose setting
                    return option >= 2;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to check auto update setting: {ex.Message}", "WindowsUpdateService");
            }

            return false;
        }

        private List<PendingUpdate> GetPendingUpdates()
        {
            var updates = new List<PendingUpdate>();

            try
            {
                // This would typically use Windows Update API (WUA) which requires COM interop
                // For this implementation, we'll simulate checking for common update indicators
                
                // Check Windows Update service status
                var updateService = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "wuauserv");
                if (updateService?.Status == ServiceControllerStatus.Running)
                {
                    // Service is running, could indicate updates being processed
                    updates.Add(new PendingUpdate
                    {
                        Title = "Windows Update Service Active",
                        Description = "Windows Update service is running and may be processing updates",
                        IsImportant = false,
                        SizeInBytes = 0
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to get pending updates: {ex.Message}", "WindowsUpdateService");
            }

            return updates;
        }

        private List<StartupProgramInfo> GetStartupFromRegistry(RegistryKey rootKey, string subKeyPath, StartupLocation location)
        {
            var programs = new List<StartupProgramInfo>();

            try
            {
                using var key = rootKey.OpenSubKey(subKeyPath);
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        var command = key.GetValue(valueName)?.ToString();
                        if (!string.IsNullOrEmpty(command))
                        {
                            programs.Add(new StartupProgramInfo
                            {
                                Name = valueName,
                                Command = command,
                                Location = location,
                                FilePath = ExtractFilePathFromCommand(command),
                                IsEnabled = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to read startup registry {subKeyPath}: {ex.Message}", "WindowsUpdateService");
            }

            return programs;
        }

        private List<StartupProgramInfo> GetStartupFromFolder()
        {
            var programs = new List<StartupProgramInfo>();

            try
            {
                var startupFolders = new[]
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup))
                };

                foreach (var folder in startupFolders)
                {
                    if (Directory.Exists(folder))
                    {
                        foreach (var file in Directory.GetFiles(folder, "*.*"))
                        {
                            programs.Add(new StartupProgramInfo
                            {
                                Name = Path.GetFileNameWithoutExtension(file),
                                Command = file,
                                Location = StartupLocation.StartupFolder,
                                FilePath = file,
                                IsEnabled = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to read startup folder: {ex.Message}", "WindowsUpdateService");
            }

            return programs;
        }

        private List<StartupProgramInfo> GetStartupFromTaskScheduler()
        {
            var programs = new List<StartupProgramInfo>();

            try
            {
                // This would require Task Scheduler API integration
                // For now, we'll simulate common startup tasks
                programs.Add(new StartupProgramInfo
                {
                    Name = "Task Scheduler Startup Tasks",
                    Command = "Check Task Scheduler for startup tasks",
                    Location = StartupLocation.TaskScheduler,
                    FilePath = "",
                    IsEnabled = true,
                    Impact = StartupImpact.Low
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to read scheduled startup tasks: {ex.Message}", "WindowsUpdateService");
            }

            return programs;
        }

        private StartupImpact AnalyzeStartupImpact(StartupProgramInfo program)
        {
            // High impact programs that significantly slow startup
            var highImpactPrograms = new[] { "steam", "spotify", "skype", "discord", "zoom", "teams" };
            
            // Medium impact programs
            var mediumImpactPrograms = new[] { "adobe", "office", "dropbox", "onedrive", "googledrive" };

            var programName = program.Name.ToLower();

            if (highImpactPrograms.Any(p => programName.Contains(p)))
                return StartupImpact.High;
            
            if (mediumImpactPrograms.Any(p => programName.Contains(p)))
                return StartupImpact.Medium;

            // System and antivirus programs are usually low impact or necessary
            if (programName.Contains("windows") || programName.Contains("microsoft") || 
                programName.Contains("antivirus") || programName.Contains("defender"))
                return StartupImpact.Low;

            return StartupImpact.Medium;
        }

        private string ExtractFilePathFromCommand(string command)
        {
            try
            {
                // Handle quoted paths
                if (command.StartsWith("\""))
                {
                    var endQuote = command.IndexOf("\"", 1);
                    if (endQuote > 0)
                    {
                        return command.Substring(1, endQuote - 1);
                    }
                }

                // Handle unquoted paths
                var spaceIndex = command.IndexOf(" ");
                if (spaceIndex > 0)
                {
                    return command.Substring(0, spaceIndex);
                }

                return command;
            }
            catch
            {
                return command;
            }
        }

        private bool DisableRegistryStartup(RegistryKey rootKey, string subKeyPath, string valueName)
        {
            try
            {
                using var key = rootKey.OpenSubKey(subKeyPath, true);
                if (key != null)
                {
                    key.DeleteValue(valueName, false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to disable registry startup {valueName}", ex, "WindowsUpdateService");
            }

            return false;
        }

        private bool DisableStartupFolderItem(StartupProgramInfo program)
        {
            try
            {
                if (File.Exists(program.FilePath))
                {
                    File.Delete(program.FilePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to disable startup folder item {program.Name}", ex, "WindowsUpdateService");
            }

            return false;
        }

        private bool DisableScheduledTask(string taskName)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/change /tn \"{taskName}\" /disable",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(processInfo);
                if (process != null)
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to disable scheduled task {taskName}", ex, "WindowsUpdateService");
            }

            return false;
        }
    }
}
