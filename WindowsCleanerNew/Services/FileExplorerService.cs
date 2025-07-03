using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using WindowsCleaner.Models;

namespace WindowsCleaner.Services
{
    public class FileExplorerService
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentDictionary<string, long> _sizeCache;

        public FileExplorerService()
        {
            _semaphore = new SemaphoreSlim(Environment.ProcessorCount, Environment.ProcessorCount);
            _sizeCache = new ConcurrentDictionary<string, long>();
        }

        public async Task<List<FileSystemItem>> GetDrivesAsync()
        {
            return await Task.Run(async () =>
            {
                var drives = new List<FileSystemItem>();
                
                try
                {
                    System.Diagnostics.Debug.WriteLine("Starting drive enumeration");

                    var driveTasks = DriveInfo.GetDrives()
                        .Where(drive => drive.IsReady)
                        .Select(async drive =>
                        {
                            try
                            {
                                await _semaphore.WaitAsync();
                                
                                var usedSpace = await Task.Run(() =>
                                {
                                    try
                                    {
                                        return drive.TotalSize - drive.AvailableFreeSpace;
                                    }
                                    catch
                                    {
                                        return 0L;
                                    }
                                });

                                return new FileSystemItem
                                {
                                    Name = $"{drive.Name} ({drive.DriveType})",
                                    FullPath = drive.Name,
                                    ItemType = FileSystemItemType.Drive,
                                    Size = usedSpace,
                                    LastModified = DateTime.Now
                                };
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Failed to process drive {drive.Name}: {ex.Message}");
                                return null;
                            }
                            finally
                            {
                                _semaphore.Release();
                            }
                        });

                    var results = await Task.WhenAll(driveTasks);
                    drives.AddRange(results.Where(d => d != null)!);

                    System.Diagnostics.Debug.WriteLine($"Found {drives.Count} drives");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to enumerate drives: {ex.Message}");
                }

                return drives;
            });
        }

        public async Task<List<FileSystemItem>> GetDirectoryContentsAsync(string path, bool includeFiles = true)
        {
            return await Task.Run(async () =>
            {
                var items = new List<FileSystemItem>();

                try
                {
                    if (!Directory.Exists(path))
                    {
                        System.Diagnostics.Debug.WriteLine($"Directory does not exist: {path}");
                        return items;
                    }

                    var directoryInfo = new DirectoryInfo(path);
                    var tasks = new List<Task<FileSystemItem?>>();
                    
                    // Process subdirectories with parallel execution
                    try
                    {
                        var directories = directoryInfo.GetDirectories();
                        foreach (var dir in directories)
                        {
                            tasks.Add(ProcessDirectoryAsync(dir));
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        System.Diagnostics.Debug.WriteLine($"Access denied to directory: {path}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error getting directories in {path}: {ex.Message}");
                    }

                    // Process files if requested
                    if (includeFiles)
                    {
                        try
                        {
                            var files = directoryInfo.GetFiles();
                            foreach (var file in files)
                            {
                                tasks.Add(ProcessFileAsync(file));
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            System.Diagnostics.Debug.WriteLine($"Access denied to files in: {path}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error getting files in {path}: {ex.Message}");
                        }
                    }

                    // Wait for all tasks to complete
                    var results = await Task.WhenAll(tasks);
                    items.AddRange(results.Where(item => item != null)!);

                    // Calculate size percentages
                    var totalSize = items.Sum(i => i.Size);
                    if (totalSize > 0)
                    {
                        foreach (var item in items)
                        {
                            item.SizePercentage = (double)item.Size / totalSize * 100;
                        }
                    }

                    // Sort by size descending
                    return items.OrderByDescending(i => i.Size).ToList();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to get directory contents for {path}: {ex.Message}");
                    return new List<FileSystemItem>();
                }
            });
        }

        private async Task<FileSystemItem?> ProcessDirectoryAsync(DirectoryInfo dir)
        {
            try
            {
                await _semaphore.WaitAsync();

                var size = await CalculateDirectorySizeAsync(dir.FullName);
                return new FileSystemItem
                {
                    Name = dir.Name,
                    FullPath = dir.FullName,
                    ItemType = FileSystemItemType.Folder,
                    LastModified = dir.LastWriteTime,
                    Size = size
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to process directory {dir.Name}: {ex.Message}");
                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<FileSystemItem?> ProcessFileAsync(FileInfo file)
        {
            try
            {
                return await Task.Run(() =>
                {
                    return new FileSystemItem
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        ItemType = FileSystemItemType.File,
                        LastModified = file.LastWriteTime,
                        Size = file.Length,
                        Extension = file.Extension
                    };
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to process file {file.Name}: {ex.Message}");
                return null;
            }
        }

        private async Task<long> CalculateDirectorySizeAsync(string directoryPath)
        {
            // Check cache first
            if (_sizeCache.TryGetValue(directoryPath, out long cachedSize))
            {
                return cachedSize;
            }

            try
            {
                var size = await Task.Run(() =>
                {
                    try
                    {
                        var directory = new DirectoryInfo(directoryPath);
                        return directory.EnumerateFiles("*", SearchOption.AllDirectories)
                            .Where(file => !IsSystemFile(file))
                            .Sum(file => 
                            {
                                try 
                                { 
                                    return file.Length; 
                                } 
                                catch 
                                { 
                                    return 0L; 
                                }
                            });
                    }
                    catch
                    {
                        return 0L;
                    }
                });

                // Cache the result
                _sizeCache.TryAdd(directoryPath, size);
                return size;
            }
            catch
            {
                return 0L;
            }
        }

        private bool IsSystemFile(FileInfo file)
        {
            try
            {
                return file.Attributes.HasFlag(FileAttributes.System) ||
                       file.Attributes.HasFlag(FileAttributes.Hidden) ||
                       file.Name.StartsWith("$") ||
                       file.Name.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase) ||
                       file.Name.Equals("thumbs.db", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<FileSystemItem>> FindLargestFoldersAsync(string rootPath, int maxResults = 20)
        {
            return await Task.Run(() =>
            {
                var folders = new List<FileSystemItem>();
                
                try
                {
                    ScanDirectoryRecursive(rootPath, folders, 0, 3); // Max depth of 3
                    
                    return folders
                        .OrderByDescending(f => f.Size)
                        .Take(maxResults)
                        .ToList();
                }
                catch
                {
                    return new List<FileSystemItem>();
                }
            });
        }

        public async Task<List<FileSystemItem>> FindFilesByTypeAsync(string rootPath, string[] extensions)
        {
            return await Task.Run(() =>
            {
                var files = new List<FileSystemItem>();
                
                try
                {
                    FindFilesRecursive(rootPath, files, extensions, 0, 3); // Max depth of 3
                    
                    return files
                        .OrderByDescending(f => f.Size)
                        .ToList();
                }
                catch
                {
                    return new List<FileSystemItem>();
                }
            });
        }

        public async Task<bool> DeleteItemsAsync(List<FileSystemItem> items, IProgress<string> progress)
        {
            return await Task.Run(() =>
            {
                var totalItems = items.Count;
                var processedItems = 0;
                var errors = 0;

                foreach (var item in items)
                {
                    try
                    {
                        progress?.Report($"Deleting {item.Name}... ({processedItems + 1}/{totalItems})");

                        if (item.ItemType == FileSystemItemType.File)
                        {
                            if (File.Exists(item.FullPath))
                            {
                                File.Delete(item.FullPath);
                            }
                        }
                        else if (item.ItemType == FileSystemItemType.Folder)
                        {
                            if (Directory.Exists(item.FullPath))
                            {
                                Directory.Delete(item.FullPath, true);
                            }
                        }
                    }
                    catch
                    {
                        errors++;
                    }

                    processedItems++;
                }

                progress?.Report($"Completed. {processedItems - errors} items deleted, {errors} errors.");
                return errors == 0;
            });
        }

        private void ScanDirectoryRecursive(string path, List<FileSystemItem> folders, int currentDepth, int maxDepth)
        {
            if (currentDepth >= maxDepth) return;

            try
            {
                var directoryInfo = new DirectoryInfo(path);
                
                foreach (var dir in directoryInfo.GetDirectories())
                {
                    try
                    {
                        var size = CalculateDirectorySize(dir.FullName);
                        if (size > 10 * 1024 * 1024) // Only include folders > 10MB
                        {
                            var item = new FileSystemItem
                            {
                                Name = dir.Name,
                                FullPath = dir.FullName,
                                ItemType = FileSystemItemType.Folder,
                                LastModified = dir.LastWriteTime,
                                Size = size
                            };
                            folders.Add(item);
                        }

                        ScanDirectoryRecursive(dir.FullName, folders, currentDepth + 1, maxDepth);
                    }
                    catch
                    {
                        // Skip directories we can't access
                    }
                }
            }
            catch
            {
                // Skip if we can't access the directory
            }
        }

        private void FindFilesRecursive(string path, List<FileSystemItem> files, string[] extensions, int currentDepth, int maxDepth)
        {
            if (currentDepth >= maxDepth) return;

            try
            {
                var directoryInfo = new DirectoryInfo(path);
                
                // Add matching files
                foreach (var file in directoryInfo.GetFiles())
                {
                    try
                    {
                        if (extensions.Contains(file.Extension.ToLower()) && file.Length > 1024 * 1024) // > 1MB
                        {
                            var item = new FileSystemItem
                            {
                                Name = file.Name,
                                FullPath = file.FullName,
                                ItemType = FileSystemItemType.File,
                                LastModified = file.LastWriteTime,
                                Size = file.Length,
                                Extension = file.Extension
                            };
                            files.Add(item);
                        }
                    }
                    catch
                    {
                        // Skip files we can't access
                    }
                }

                // Recurse into subdirectories
                foreach (var dir in directoryInfo.GetDirectories())
                {
                    try
                    {
                        FindFilesRecursive(dir.FullName, files, extensions, currentDepth + 1, maxDepth);
                    }
                    catch
                    {
                        // Skip directories we can't access
                    }
                }
            }
            catch
            {
                // Skip if we can't access the directory
            }
        }

        private long CalculateDirectorySize(string path)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(path);
                long size = 0;

                // Add file sizes
                foreach (var file in directoryInfo.GetFiles())
                {
                    try
                    {
                        size += file.Length;
                    }
                    catch
                    {
                        // Skip files we can't access
                    }
                }

                // Add subdirectory sizes (non-recursive for performance)
                foreach (var dir in directoryInfo.GetDirectories())
                {
                    try
                    {
                        foreach (var file in dir.GetFiles())
                        {
                            try
                            {
                                size += file.Length;
                            }
                            catch
                            {
                                // Skip files we can't access
                            }
                        }
                    }
                    catch
                    {
                        // Skip directories we can't access
                    }
                }

                return size;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<List<FileSystemItemInfo>> GetDirectoryContentsAsync(string directoryPath)
        {
            return await Task.Run(() =>
            {
                var items = new List<FileSystemItemInfo>();
                
                try
                {
                    var directory = new DirectoryInfo(directoryPath);
                    
                    // Add directories
                    foreach (var dir in directory.GetDirectories())
                    {
                        try
                        {
                            items.Add(new FileSystemItemInfo
                            {
                                Name = dir.Name,
                                FullPath = dir.FullName,
                                IsDirectory = true,
                                Size = 0, // Calculate size separately if needed
                                LastModified = dir.LastWriteTime,
                                Extension = string.Empty
                            });
                        }
                        catch
                        {
                            // Skip inaccessible directories
                        }
                    }
                    
                    // Add files
                    foreach (var file in directory.GetFiles())
                    {
                        try
                        {
                            items.Add(new FileSystemItemInfo
                            {
                                Name = file.Name,
                                FullPath = file.FullName,
                                IsDirectory = false,
                                Size = file.Length,
                                LastModified = file.LastWriteTime,
                                Extension = file.Extension
                            });
                        }
                        catch
                        {
                            // Skip inaccessible files
                        }
                    }
                }
                catch
                {
                    // Return empty list if directory is inaccessible
                }
                
                return items;
            });
        }
        
        public async Task DeleteItemAsync(string path, bool isDirectory)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (isDirectory)
                    {
                        Directory.Delete(path, true);
                    }
                    else
                    {
                        File.Delete(path);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Cannot delete {path}: {ex.Message}", ex);
                }
            });
        }
    }
}
