using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WindowsCleaner.Models;
using WindowsCleaner.Services;

namespace WindowsCleaner.ViewModels
{
    public class FileExplorerViewModel : BaseViewModel
    {
        private readonly FileExplorerService _fileService;
        private string _currentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private ObservableCollection<FileSystemItemInfo> _items = new();
        private bool _isLoading;
        private string _statusMessage = string.Empty;

        public FileExplorerViewModel()
        {
            _fileService = new FileExplorerService();
            Items = new ObservableCollection<FileSystemItemInfo>();
            
            NavigateCommand = new RelayCommand<string>(async (path) => await NavigateToAsync(path));
            DeleteCommand = new RelayCommand<FileSystemItemInfo>(async (item) => await DeleteItemAsync(item));
            RefreshCommand = new RelayCommand(async () => await LoadCurrentDirectoryAsync());
            OpenCommand = new RelayCommand<FileSystemItemInfo>(OpenItem);
            
            // Load initial directory
            _ = LoadCurrentDirectoryAsync();
        }

        public string CurrentPath
        {
            get => _currentPath;
            set
            {
                if (SetProperty(ref _currentPath, value))
                {
                    _ = LoadCurrentDirectoryAsync();
                }
            }
        }

        public ObservableCollection<FileSystemItemInfo> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand NavigateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand OpenCommand { get; }

        private async Task NavigateToAsync(string? path)
        {
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                if (Directory.Exists(path))
                {
                    CurrentPath = path;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Cannot navigate to {path}: {ex.Message}";
            }
        }

        private async Task LoadCurrentDirectoryAsync()
        {
            IsLoading = true;
            StatusMessage = $"Loading {CurrentPath}...";
            
            try
            {
                Items.Clear();
                var items = await _fileService.GetDirectoryContentsAsync(CurrentPath);
                
                foreach (var item in items)
                {
                    Items.Add(item);
                }
                
                StatusMessage = $"Loaded {Items.Count} item(s)";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading directory: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteItemAsync(FileSystemItemInfo? item)
        {
            if (item == null) return;

            IsLoading = true;
            StatusMessage = $"Deleting {item.Name}...";
            
            try
            {
                await _fileService.DeleteItemAsync(item.FullPath, item.IsDirectory);
                Items.Remove(item);
                StatusMessage = $"Deleted {item.Name}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting {item.Name}: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenItem(FileSystemItemInfo? item)
        {
            if (item == null) return;

            try
            {
                if (item.IsDirectory)
                {
                    CurrentPath = item.FullPath;
                }
                else
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = item.FullPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Cannot open {item.Name}: {ex.Message}";
            }
        }
    }
}
