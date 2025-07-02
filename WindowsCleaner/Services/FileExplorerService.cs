using System.IO;
using WindowsCleaner.Models;

namespace WindowsCleaner.Services
{
    public class FileExplorerService
    {
        public async Task<List<FileSystemItem>> GetDrivesAsync()
        {
            return await Task.Run(() =>
            {
                var drives = new List<FileSystemItem>();
                
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady)
                    {
                        var driveItem = new FileSystemItem
                        {
                            Name = $"{drive.Name} ({drive.DriveType})",
                            FullPath = drive.Name,
                            ItemType = FileSystemItemType.Drive,
                            Size = drive.TotalSize - drive.AvailableFreeSpace
                        };

                        drives.Add(driveItem);
                    }
                }

                return drives;
            });
        }

        public async Task<List<FileSystemItem>> GetDirectoryContentsAsync(string path, bool includeFiles = true)
        {
            return await Task.Run(() =>
            {
                var items = new List<FileSystemItem>();

                try
                {
                    var directoryInfo = new DirectoryInfo(path);
                    
                    // Add subdirectories
                    foreach (var dir in directoryInfo.GetDirectories())
                    {
                        try
                        {
                            var size = CalculateDirectorySize(dir.FullName);
                            var item = new FileSystemItem
                            {
                                Name = dir.Name,
                                FullPath = dir.FullName,
                                ItemType = FileSystemItemType.Folder,
                                LastModified = dir.LastWriteTime,
                                Size = size
                            };
                            items.Add(item);
                        }
                        catch
                        {
                            // Skip directories we can't access
                        }
                    }

                    // Add files if requested
                    if (includeFiles)
                    {
                        foreach (var file in directoryInfo.GetFiles())
                        {
                            try
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
                                items.Add(item);
                            }
                            catch
                            {
                                // Skip files we can't access
                            }
                        }
                    }

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
                catch
                {
                    return new List<FileSystemItem>();
                }
            });
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
    }
}
