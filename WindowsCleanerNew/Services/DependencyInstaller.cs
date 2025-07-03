using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.IO.Compression;

namespace WindowsCleaner.Services
{
    public class DependencyInstaller
    {
        private readonly HttpClient _httpClient;

        public DependencyInstaller()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> CheckAndInstallDependenciesAsync(IProgress<string> progress)
        {
            var success = true;

            // Check .NET Runtime
            progress.Report("Checking .NET 8.0 Runtime...");
            if (!IsDotNetInstalled())
            {
                progress.Report("Installing .NET 8.0 Runtime...");
                success &= await InstallDotNetRuntimeAsync(progress);
            }
            else
            {
                progress.Report(".NET 8.0 Runtime is already installed.");
            }

            // Check Visual C++ Redistributable
            progress.Report("Checking Visual C++ Redistributable...");
            if (!IsVCRedistInstalled())
            {
                progress.Report("Installing Visual C++ Redistributable...");
                success &= await InstallVCRedistAsync(progress);
            }
            else
            {
                progress.Report("Visual C++ Redistributable is already installed.");
            }

            return success;
        }

        private bool IsDotNetInstalled()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--list-runtimes",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null) return false;

                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Contains("Microsoft.WindowsDesktop.App 8.0") || 
                       output.Contains("Microsoft.NETCore.App 8.0");
            }
            catch
            {
                return false;
            }
        }

        private bool IsVCRedistInstalled()
        {
            try
            {
                // Check for Visual C++ 2022 Redistributable in registry
                var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\X64");
                
                if (key != null)
                {
                    var version = key.GetValue("Version")?.ToString();
                    key.Close();
                    return !string.IsNullOrEmpty(version);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> InstallDotNetRuntimeAsync(IProgress<string> progress)
        {
            try
            {
                var tempPath = Path.GetTempPath();
                var installerPath = Path.Combine(tempPath, "dotnet-runtime-8.0-win-x64.exe");

                // Download .NET Runtime installer
                progress.Report("Downloading .NET 8.0 Runtime installer...");
                var downloadUrl = "https://download.microsoft.com/download/a/b/c/abc91234-1234-1234-1234-123456789012/dotnet-runtime-8.0.0-win-x64.exe";
                
                // For demo purposes, we'll use a placeholder URL
                // In a real application, you'd need the actual Microsoft download URL
                
                progress.Report("Installing .NET 8.0 Runtime (this may take a few minutes)...");
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = "/quiet /norestart",
                    UseShellExecute = true,
                    Verb = "runas" // Run as administrator
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    
                    // Clean up installer
                    try { File.Delete(installerPath); } catch { }
                    
                    return process.ExitCode == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                progress.Report($"Error installing .NET Runtime: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> InstallVCRedistAsync(IProgress<string> progress)
        {
            try
            {
                var tempPath = Path.GetTempPath();
                var installerPath = Path.Combine(tempPath, "vc_redist.x64.exe");

                // Download VC++ Redistributable
                progress.Report("Downloading Visual C++ Redistributable...");
                var downloadUrl = "https://aka.ms/vs/17/release/vc_redist.x64.exe";
                
                using var response = await _httpClient.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();
                
                await using var fileStream = File.Create(installerPath);
                await response.Content.CopyToAsync(fileStream);

                progress.Report("Installing Visual C++ Redistributable...");
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = "/quiet /norestart",
                    UseShellExecute = true,
                    Verb = "runas" // Run as administrator
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    
                    // Clean up installer
                    try { File.Delete(installerPath); } catch { }
                    
                    return process.ExitCode == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                progress.Report($"Error installing VC++ Redistributable: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateUninstallerAsync()
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appFolder = Path.Combine(appDataPath, "WindowsCleaner");
                Directory.CreateDirectory(appFolder);

                var uninstallerPath = Path.Combine(appFolder, "uninstall.bat");
                var uninstallerContent = @"@echo off
echo Uninstalling Windows Cleaner Pro...

REM Remove from startup
reg delete ""HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"" /v ""WindowsCleanerPro"" /f >nul 2>&1

REM Remove scheduled task
schtasks /delete /tn ""WindowsCleanerPro_AutoClean"" /f >nul 2>&1

REM Remove application files
rmdir /s /q ""%LOCALAPPDATA%\WindowsCleaner"" >nul 2>&1

REM Remove from Programs and Features
reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\WindowsCleanerPro"" /f >nul 2>&1

echo Uninstallation completed.
pause";

                await File.WriteAllTextAsync(uninstallerPath, uninstallerContent);

                // Register in Programs and Features
                var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\WindowsCleanerPro");
                
                key.SetValue("DisplayName", "Windows Cleaner Pro");
                key.SetValue("DisplayVersion", "1.0.0");
                key.SetValue("Publisher", "System Optimizer");
                key.SetValue("UninstallString", uninstallerPath);
                key.SetValue("DisplayIcon", System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName);
                key.SetValue("EstimatedSize", 50000); // 50MB in KB
                key.Close();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating uninstaller: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
