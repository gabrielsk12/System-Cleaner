using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Win32;
using WindowsCleaner.Models;

namespace WindowsCleaner.Services
{
    /// <summary>
    /// Service for checking and updating system drivers
    /// </summary>
    public class DriverService
    {
        private readonly LoggingService _logger;
        private readonly HttpClient _httpClient;

        public DriverService()
        {
            _logger = LoggingService.Instance;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WindowsCleanerPro/1.0");
        }

        // Add methods expected by DriverUpdatesViewModel
        public async Task<List<DriverInfo>> GetInstalledDriversAsync()
        {
            return await GetSystemDriversAsync();
        }
        
        public async Task<DriverUpdateInfo?> CheckForUpdatesAsync(DriverInfo driver)
        {
            return await CheckDriverUpdatesAsync(driver);
        }

        public async Task<bool> UpdateDriverAsync(DriverInfo driver)
        {
            var updateInfo = new DriverUpdateInfo
            {
                DriverInfo = driver,
                HasUpdate = true,
                UpdateAvailable = true,
                NewVersion = driver.LatestVersion
            };
            
            return await UpdateDriverAsync(updateInfo);
        }
        
        public async Task<List<DriverInfo>> GetSystemDriversAsync()
        {
            return await Task.Run(() =>
            {
                var drivers = new List<DriverInfo>();

                try
                {
                    _logger.LogInfo("Starting driver enumeration", "DriverService");

                    using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver");
                    using var collection = searcher.Get();

                    foreach (ManagementObject driver in collection)
                    {
                        try
                        {
                            var driverInfo = new DriverInfo
                            {
                                DeviceName = driver["DeviceName"]?.ToString() ?? "Unknown Device",
                                DriverVersion = driver["DriverVersion"]?.ToString() ?? "Unknown",
                                DriverDate = ParseDriverDate(driver["DriverDate"]?.ToString()),
                                InfName = driver["InfName"]?.ToString() ?? "",
                                HardwareID = driver["HardwareID"]?.ToString() ?? "",
                                CompatID = driver["CompatID"]?.ToString() ?? "",
                                Manufacturer = driver["Manufacturer"]?.ToString() ?? "Unknown",
                                IsSigned = ParseBoolean(driver["IsSigned"]?.ToString()),
                                Location = driver["Location"]?.ToString() ?? ""
                            };

                            // Skip system and Microsoft drivers that don't need updates
                            if (ShouldCheckDriver(driverInfo))
                            {
                                drivers.Add(driverInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Failed to process driver: {ex.Message}", "DriverService");
                        }
                    }

                    _logger.LogInfo($"Found {drivers.Count} drivers to check", "DriverService");
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to enumerate drivers", ex, "DriverService");
                }

                return drivers.OrderBy(d => d.DeviceName).ToList();
            });
        }

        public async Task<DriverUpdateInfo> CheckDriverUpdatesAsync(DriverInfo driver)
        {
            try
            {
                // For demonstration, we'll use Windows Update API approach
                // In a real implementation, you would integrate with manufacturer APIs
                
                var updateInfo = new DriverUpdateInfo
                {
                    DriverInfo = driver,
                    HasUpdate = false,
                    UpdateAvailable = false,
                    NewVersion = driver.DriverVersion,
                    UpdateSource = "Windows Update",
                    DownloadUrl = "",
                    ReleaseNotes = ""
                };

                // Check if driver is older than 2 years (simplified update check)
                if (driver.DriverDate.HasValue && driver.DriverDate.Value < DateTime.Now.AddYears(-2))
                {
                    updateInfo.HasUpdate = true;
                    updateInfo.UpdateAvailable = true;
                    updateInfo.NewVersion = "Newer version available";
                    updateInfo.ReleaseNotes = "Check manufacturer website for latest driver";
                }

                // For critical hardware, always suggest checking for updates
                if (IsCriticalHardware(driver))
                {
                    updateInfo.UpdateAvailable = true;
                    updateInfo.ReleaseNotes = "Critical hardware - recommend checking for updates";
                }

                return updateInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to check updates for driver {driver.DeviceName}", ex, "DriverService");
                
                return new DriverUpdateInfo
                {
                    DriverInfo = driver,
                    HasUpdate = false,
                    UpdateAvailable = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<List<DriverUpdateInfo>> CheckAllDriverUpdatesAsync(
            IProgress<DriverCheckProgress>? progress = null, 
            CancellationToken cancellationToken = default)
        {
            var drivers = await GetSystemDriversAsync();
            var updateInfos = new List<DriverUpdateInfo>();
            var total = drivers.Count;

            for (int i = 0; i < drivers.Count; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                
                var driver = drivers[i];
                
                progress?.Report(new DriverCheckProgress
                {
                    CurrentDriver = driver.DeviceName,
                    Progress = (i + 1) * 100 / total,
                    CompletedCount = i,
                    TotalCount = total
                });

                var updateInfo = await CheckDriverUpdatesAsync(driver);
                updateInfos.Add(updateInfo);

                // Small delay to prevent overwhelming the system
                try
                {
                    await Task.Delay(100, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Pass through cancellation
                    throw;
                }
            }

            return updateInfos;
        }

        public async Task<bool> UpdateDriverAsync(DriverUpdateInfo updateInfo)
        {
            try
            {
                _logger.LogInfo($"Attempting to update driver: {updateInfo.DriverInfo.DeviceName}", "DriverService");

                // Use Windows Update PowerShell module or pnputil
                var processInfo = new ProcessStartInfo
                {
                    FileName = "pnputil.exe",
                    Arguments = $"/scan-devices",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    
                    if (process.ExitCode == 0)
                    {
                        _logger.LogInfo($"Driver update initiated for: {updateInfo.DriverInfo.DeviceName}", "DriverService");
                        return true;
                    }
                    else
                    {
                        var error = await process.StandardError.ReadToEndAsync();
                        _logger.LogError($"Driver update failed: {error}", null, "DriverService");
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update driver {updateInfo.DriverInfo.DeviceName}", ex, "DriverService");
                return false;
            }
        }

        public async Task<List<string>> GetDriverUpdateLogAsync()
        {
            return await Task.Run(() =>
            {
                var logs = new List<string>();

                try
                {
                    // Read Windows Update logs for driver updates
                    var windowsLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Logs", "WindowsUpdate");
                    
                    if (Directory.Exists(windowsLogPath))
                    {
                        var logFiles = Directory.GetFiles(windowsLogPath, "*.etl")
                            .OrderByDescending(f => new FileInfo(f).CreationTime)
                            .Take(5);

                        foreach (var logFile in logFiles)
                        {
                            try
                            {
                                // Parse ETL files would require Windows Event Log APIs
                                // For now, just list the files
                                logs.Add($"Log file: {Path.GetFileName(logFile)} - {new FileInfo(logFile).CreationTime}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Failed to read log file {logFile}: {ex.Message}", "DriverService");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to get driver update logs", ex, "DriverService");
                }

                return logs;
            });
        }

        private DateTime? ParseDriverDate(string? dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return null;

            try
            {
                // WMI date format: 20230815000000.000000+***
                if (dateString.Length >= 14)
                {
                    var year = int.Parse(dateString.Substring(0, 4));
                    var month = int.Parse(dateString.Substring(4, 2));
                    var day = int.Parse(dateString.Substring(6, 2));
                    
                    return new DateTime(year, month, day);
                }
            }
            catch
            {
                // Ignore parsing errors
            }

            return null;
        }

        private bool ParseBoolean(string? value)
        {
            return !string.IsNullOrEmpty(value) && 
                   (value.Equals("True", StringComparison.OrdinalIgnoreCase) || value == "1");
        }

        private bool ShouldCheckDriver(DriverInfo driver)
        {
            // Skip Microsoft and generic drivers
            if (driver.Manufacturer.Contains("Microsoft", StringComparison.OrdinalIgnoreCase) ||
                driver.DeviceName.Contains("Generic", StringComparison.OrdinalIgnoreCase) ||
                driver.DeviceName.Contains("Standard", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Include important hardware types
            var importantTypes = new[] { "Display", "Audio", "Network", "Storage", "Graphics", "Video", "Ethernet", "WiFi", "Bluetooth" };
            return importantTypes.Any(type => driver.DeviceName.Contains(type, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsCriticalHardware(DriverInfo driver)
        {
            var criticalTypes = new[] { "Display", "Graphics", "Video", "Network", "Ethernet", "Storage", "Disk" };
            return criticalTypes.Any(type => driver.DeviceName.Contains(type, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class DriverCheckProgress
    {
        public string CurrentDriver { get; set; } = "";
        public int Progress { get; set; }
        public int CompletedCount { get; set; }
        public int TotalCount { get; set; }
    }
}
