using System.IO;
using System.Management;
using Microsoft.Win32;
using WindowsCleaner.Models;

namespace WindowsCleaner.Services
{
    public class CleanerService
    {
        private readonly List<string> _tempDirectories;
        private readonly List<string> _cacheDirectories;
        private readonly List<string> _logDirectories;

        public CleanerService()
        {
            _tempDirectories = new List<string>
            {
                Path.GetTempPath(),
                Environment.GetFolderPath(Environment.SpecialFolder.InternetCache),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp")
            };

            _cacheDirectories = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "INetCache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Default", "Cache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mozilla", "Firefox", "Profiles")
            };

            _logDirectories = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Logs"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "LogFiles"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft", "Windows", "WER")
            };
        }

        public async Task ScanForFilesAsync(List<CleanupCategory> categories, IProgress<ScanProgress> progress)
        {
            var totalCategories = categories.Count;
            var currentCategory = 0;

            foreach (var category in categories)
            {
                var scanProgress = new ScanProgress
                {
                    Category = category.Category,
                    CurrentOperation = $"Scanning {category.Name}...",
                    ProgressPercentage = (currentCategory * 100.0) / totalCategories
                };

                progress.Report(scanProgress);

                try
                {
                    var result = await ScanCategoryAsync(category.Category);
                    scanProgress.FilesFound = result.filesFound;
                    scanProgress.SizeInBytes = result.sizeInBytes;
                    progress.Report(scanProgress);
                }
                catch (Exception ex)
                {
                    // Log the error but continue with other categories
                    System.Diagnostics.Debug.WriteLine($"Error scanning {category.Name}: {ex.Message}");
                }

                currentCategory++;
            }

            // Report completion
            progress.Report(new ScanProgress
            {
                CurrentOperation = "Scan completed",
                ProgressPercentage = 100
            });
        }

        public async Task<CleanupResult> CleanFilesAsync(List<CleanupCategory> categories, IProgress<CleanProgress> progress)
        {
            var result = new CleanupResult();
            var totalCategories = categories.Count;
            var currentCategory = 0;

            foreach (var category in categories)
            {
                var cleanProgress = new CleanProgress
                {
                    CurrentOperation = $"Cleaning {category.Name}...",
                    ProgressPercentage = (currentCategory * 100.0) / totalCategories
                };

                progress.Report(cleanProgress);

                try
                {
                    var categoryResult = await CleanCategoryAsync(category.Category);
                    result.TotalFiles += categoryResult.filesDeleted;
                    result.TotalSize += categoryResult.sizeDeleted;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error cleaning {category.Name}: {ex.Message}");
                }

                currentCategory++;
            }

            // Report completion
            progress.Report(new CleanProgress
            {
                CurrentOperation = "Cleanup completed",
                ProgressPercentage = 100
            });

            return result;
        }

        private async Task<(int filesFound, long sizeInBytes)> ScanCategoryAsync(CleanupType category)
        {
            return await Task.Run(() =>
            {
                var filesFound = 0;
                long sizeInBytes = 0;

                try
                {
                    switch (category)
                    {
                        case CleanupType.WindowsUpdate:
                            (filesFound, sizeInBytes) = ScanWindowsUpdateFiles();
                            break;

                        case CleanupType.TemporaryFiles:
                            (filesFound, sizeInBytes) = ScanTemporaryFiles();
                            break;

                        case CleanupType.RecycleBin:
                            (filesFound, sizeInBytes) = ScanRecycleBin();
                            break;

                        case CleanupType.SystemCache:
                            (filesFound, sizeInBytes) = ScanSystemCache();
                            break;

                        case CleanupType.BrowserCache:
                            (filesFound, sizeInBytes) = ScanBrowserCache();
                            break;

                        case CleanupType.LogFiles:
                            (filesFound, sizeInBytes) = ScanLogFiles();
                            break;

                        case CleanupType.ErrorReports:
                            (filesFound, sizeInBytes) = ScanErrorReports();
                            break;

                        case CleanupType.ThumbnailCache:
                            (filesFound, sizeInBytes) = ScanThumbnailCache();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error scanning {category}: {ex.Message}");
                }

                return (filesFound, sizeInBytes);
            });
        }

        private async Task<(int filesDeleted, long sizeDeleted)> CleanCategoryAsync(CleanupType category)
        {
            return await Task.Run(() =>
            {
                var filesDeleted = 0;
                long sizeDeleted = 0;

                try
                {
                    switch (category)
                    {
                        case CleanupType.WindowsUpdate:
                            (filesDeleted, sizeDeleted) = CleanWindowsUpdateFiles();
                            break;

                        case CleanupType.TemporaryFiles:
                            (filesDeleted, sizeDeleted) = CleanTemporaryFiles();
                            break;

                        case CleanupType.RecycleBin:
                            (filesDeleted, sizeDeleted) = CleanRecycleBin();
                            break;

                        case CleanupType.SystemCache:
                            (filesDeleted, sizeDeleted) = CleanSystemCache();
                            break;

                        case CleanupType.BrowserCache:
                            (filesDeleted, sizeDeleted) = CleanBrowserCache();
                            break;

                        case CleanupType.LogFiles:
                            (filesDeleted, sizeDeleted) = CleanLogFiles();
                            break;

                        case CleanupType.ErrorReports:
                            (filesDeleted, sizeDeleted) = CleanErrorReports();
                            break;

                        case CleanupType.ThumbnailCache:
                            (filesDeleted, sizeDeleted) = CleanThumbnailCache();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error cleaning {category}: {ex.Message}");
                }

                return (filesDeleted, sizeDeleted);
            });
        }

        private (int filesFound, long sizeInBytes) ScanWindowsUpdateFiles()
        {
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "catroot2"),
                @"C:\Windows\WinSxS\Backup",
                @"C:\Windows\WinSxS\ManifestCache"
            };

            return ScanDirectories(paths, new[] { "*.cab", "*.msu", "*.tmp", "*.log" });
        }

        private (int filesDeleted, long sizeDeleted) CleanWindowsUpdateFiles()
        {
            // For safety, we'll only clean the download cache, not critical system files
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download")
            };

            return CleanDirectories(paths, new[] { "*.*" });
        }

        private (int filesFound, long sizeInBytes) ScanTemporaryFiles()
        {
            return ScanDirectories(_tempDirectories.ToArray(), new[] { "*.*" });
        }

        private (int filesDeleted, long sizeDeleted) CleanTemporaryFiles()
        {
            return CleanDirectories(_tempDirectories.ToArray(), new[] { "*.*" });
        }

        private (int filesFound, long sizeInBytes) ScanRecycleBin()
        {
            var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed);
            var paths = drives.Select(d => Path.Combine(d.Name, "$Recycle.Bin")).ToArray();
            return ScanDirectories(paths, new[] { "*.*" });
        }

        private (int filesDeleted, long sizeDeleted) CleanRecycleBin()
        {
            var filesDeleted = 0;
            long sizeDeleted = 0;

            try
            {
                // Use Shell32 to empty recycle bin properly
                var shell32 = Type.GetTypeFromProgID("Shell.Application");
                if (shell32 != null)
                {
                    dynamic shell = Activator.CreateInstance(shell32);
                    shell.Namespace(10).Items().InvokeVerb("Delete");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cleaning recycle bin: {ex.Message}");
            }

            return (filesDeleted, sizeDeleted);
        }

        private (int filesFound, long sizeInBytes) ScanSystemCache()
        {
            return ScanDirectories(_cacheDirectories.ToArray(), new[] { "*.*" });
        }

        private (int filesDeleted, long sizeDeleted) CleanSystemCache()
        {
            var safePaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch")
            };
            return CleanDirectories(safePaths, new[] { "*.pf" });
        }

        private (int filesFound, long sizeInBytes) ScanBrowserCache()
        {
            var browserPaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Default", "Cache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data", "Default", "Cache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mozilla", "Firefox", "Profiles")
            };

            return ScanDirectories(browserPaths, new[] { "*.*" });
        }

        private (int filesDeleted, long sizeDeleted) CleanBrowserCache()
        {
            var browserPaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "User Data", "Default", "Cache"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Edge", "User Data", "Default", "Cache")
            };

            return CleanDirectories(browserPaths, new[] { "*.*" });
        }

        private (int filesFound, long sizeInBytes) ScanLogFiles()
        {
            return ScanDirectories(_logDirectories.ToArray(), new[] { "*.log", "*.txt", "*.etl" });
        }

        private (int filesDeleted, long sizeDeleted) CleanLogFiles()
        {
            var safePaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Logs", "WindowsUpdate")
            };
            return CleanDirectories(safePaths, new[] { "*.log" });
        }

        private (int filesFound, long sizeInBytes) ScanErrorReports()
        {
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft", "Windows", "WER"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "WER")
            };

            return ScanDirectories(paths, new[] { "*.*" });
        }

        private (int filesDeleted, long sizeDeleted) CleanErrorReports()
        {
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Microsoft", "Windows", "WER"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "WER")
            };

            return CleanDirectories(paths, new[] { "*.*" });
        }

        private (int filesFound, long sizeInBytes) ScanThumbnailCache()
        {
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Explorer")
            };

            return ScanDirectories(paths, new[] { "thumbcache_*.db", "iconcache_*.db" });
        }

        private (int filesDeleted, long sizeDeleted) CleanThumbnailCache()
        {
            var paths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Explorer")
            };

            return CleanDirectories(paths, new[] { "thumbcache_*.db", "iconcache_*.db" });
        }

        private (int filesFound, long sizeInBytes) ScanDirectories(string[] directories, string[] patterns)
        {
            var filesFound = 0;
            long sizeInBytes = 0;

            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory)) continue;

                try
                {
                    foreach (var pattern in patterns)
                    {
                        var files = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            try
                            {
                                var fileInfo = new FileInfo(file);
                                if (fileInfo.Exists)
                                {
                                    filesFound++;
                                    sizeInBytes += fileInfo.Length;
                                }
                            }
                            catch
                            {
                                // Skip files we can't access
                            }
                        }
                    }
                }
                catch
                {
                    // Skip directories we can't access
                }
            }

            return (filesFound, sizeInBytes);
        }

        private (int filesDeleted, long sizeDeleted) CleanDirectories(string[] directories, string[] patterns)
        {
            var filesDeleted = 0;
            long sizeDeleted = 0;

            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory)) continue;

                try
                {
                    foreach (var pattern in patterns)
                    {
                        var files = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            try
                            {
                                var fileInfo = new FileInfo(file);
                                if (fileInfo.Exists && !IsFileInUse(file))
                                {
                                    var size = fileInfo.Length;
                                    File.Delete(file);
                                    filesDeleted++;
                                    sizeDeleted += size;
                                }
                            }
                            catch
                            {
                                // Skip files we can't delete
                            }
                        }
                    }

                    // Try to remove empty directories
                    RemoveEmptyDirectories(directory);
                }
                catch
                {
                    // Skip directories we can't access
                }
            }

            return (filesDeleted, sizeDeleted);
        }

        private static bool IsFileInUse(string filePath)
        {
            try
            {
                using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
            catch
            {
                return true;
            }
        }

        private static void RemoveEmptyDirectories(string path)
        {
            try
            {
                foreach (var directory in Directory.GetDirectories(path))
                {
                    RemoveEmptyDirectories(directory);
                    if (!Directory.EnumerateFileSystemEntries(directory).Any())
                    {
                        Directory.Delete(directory);
                    }
                }
            }
            catch
            {
                // Ignore errors when removing directories
            }
        }
    }
}
