using System.Collections.ObjectModel;
using System.Windows.Input;
using WindowsCleaner.Models;
using WindowsCleaner.Services;

namespace WindowsCleaner.ViewModels
{
    public class FileExplorerViewModel : BaseViewModel
    {
        private readonly FileExplorerService _fileExplorerService;
        private bool _isLoading;
        private FileSystemItem? _selectedItem;
        private string _currentPath = "";
        private string _searchFilter = "";
        private ObservableCollection<FileSystemItem> _breadcrumbs;

        public FileExplorerViewModel()
        {
            _fileExplorerService = new FileExplorerService();
            Items = new ObservableCollection<FileSystemItem>();
            SelectedItems = new ObservableCollection<FileSystemItem>();
            _breadcrumbs = new ObservableCollection<FileSystemItem>();

            LoadDrivesCommand = new RelayCommand(async () => await LoadDrivesAsync());
            NavigateCommand = new RelayCommand<FileSystemItem>(async (item) => await NavigateToAsync(item));
            RefreshCommand = new RelayCommand(async () => await RefreshCurrentPathAsync());
            BackCommand = new RelayCommand(NavigateBack, () => _breadcrumbs.Count > 1);
            DeleteSelectedCommand = new RelayCommand(async () => await DeleteSelectedItemsAsync(), 
                () => SelectedItems.Any());
            FindLargestFoldersCommand = new RelayCommand(async () => await FindLargestFoldersAsync());
            FindFilesByTypeCommand = new RelayCommand<string>(async (type) => await FindFilesByTypeAsync(type));

            // Load drives on startup
            _ = LoadDrivesAsync();
        }

        public ObservableCollection<FileSystemItem> Items { get; }
        public ObservableCollection<FileSystemItem> SelectedItems { get; }
        public ObservableCollection<FileSystemItem> Breadcrumbs => _breadcrumbs;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public FileSystemItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                if (value != null && !SelectedItems.Contains(value))
                {
                    SelectedItems.Add(value);
                }
            }
        }

        public string CurrentPath
        {
            get => _currentPath;
            set => SetProperty(ref _currentPath, value);
        }

        public string SearchFilter
        {
            get => _searchFilter;
            set
            {
                SetProperty(ref _searchFilter, value);
                ApplyFilter();
            }
        }

        public ICommand LoadDrivesCommand { get; }
        public ICommand NavigateCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand DeleteSelectedCommand { get; }
        public ICommand FindLargestFoldersCommand { get; }
        public ICommand FindFilesByTypeCommand { get; }

        private async Task LoadDrivesAsync()
        {
            IsLoading = true;
            try
            {
                var drives = await _fileExplorerService.GetDrivesAsync();
                Items.Clear();
                _breadcrumbs.Clear();

                foreach (var drive in drives)
                {
                    Items.Add(drive);
                }

                CurrentPath = "Computer";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task NavigateToAsync(FileSystemItem? item)
        {
            if (item == null) return;

            IsLoading = true;
            try
            {
                if (item.ItemType == FileSystemItemType.Folder || item.ItemType == FileSystemItemType.Drive)
                {
                    var contents = await _fileExplorerService.GetDirectoryContentsAsync(item.FullPath);
                    Items.Clear();

                    foreach (var content in contents)
                    {
                        Items.Add(content);
                    }

                    CurrentPath = item.FullPath;
                    
                    // Update breadcrumbs
                    if (!_breadcrumbs.Contains(item))
                    {
                        _breadcrumbs.Add(item);
                    }

                    ((RelayCommand)BackCommand).RaiseCanExecuteChanged();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshCurrentPathAsync()
        {
            if (string.IsNullOrEmpty(CurrentPath) || CurrentPath == "Computer")
            {
                await LoadDrivesAsync();
            }
            else
            {
                var currentItem = new FileSystemItem 
                { 
                    FullPath = CurrentPath, 
                    ItemType = FileSystemItemType.Folder 
                };
                await NavigateToAsync(currentItem);
            }
        }

        private void NavigateBack()
        {
            if (_breadcrumbs.Count > 1)
            {
                _breadcrumbs.RemoveAt(_breadcrumbs.Count - 1);
                var previousItem = _breadcrumbs.LastOrDefault();
                
                if (previousItem != null)
                {
                    _ = NavigateToAsync(previousItem);
                }
                else
                {
                    _ = LoadDrivesAsync();
                }
            }
        }

        private async Task DeleteSelectedItemsAsync()
        {
            if (!SelectedItems.Any()) return;

            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete {SelectedItems.Count} selected items?\n\n" +
                "This action cannot be undone.",
                "Confirm Deletion",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes) return;

            IsLoading = true;
            try
            {
                var progress = new Progress<string>(status => 
                {
                    // Update status in UI
                    System.Diagnostics.Debug.WriteLine(status);
                });

                var itemsToDelete = SelectedItems.ToList();
                await _fileExplorerService.DeleteItemsAsync(itemsToDelete, progress);

                // Remove deleted items from UI
                foreach (var item in itemsToDelete)
                {
                    Items.Remove(item);
                }
                
                SelectedItems.Clear();
                ((RelayCommand)DeleteSelectedCommand).RaiseCanExecuteChanged();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task FindLargestFoldersAsync()
        {
            if (string.IsNullOrEmpty(CurrentPath) || CurrentPath == "Computer") return;

            IsLoading = true;
            try
            {
                var largestFolders = await _fileExplorerService.FindLargestFoldersAsync(CurrentPath);
                Items.Clear();

                foreach (var folder in largestFolders)
                {
                    Items.Add(folder);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task FindFilesByTypeAsync(string? fileType)
        {
            if (string.IsNullOrEmpty(CurrentPath) || CurrentPath == "Computer" || string.IsNullOrEmpty(fileType)) 
                return;

            IsLoading = true;
            try
            {
                var extensions = fileType.ToLower() switch
                {
                    "images" => new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" },
                    "videos" => new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv" },
                    "audio" => new[] { ".mp3", ".wav", ".flac", ".aac", ".ogg" },
                    "documents" => new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx" },
                    "archives" => new[] { ".zip", ".rar", ".7z", ".tar", ".gz" },
                    "executables" => new[] { ".exe", ".msi", ".dll", ".sys" },
                    _ => new[] { ".*" }
                };

                var files = await _fileExplorerService.FindFilesByTypeAsync(CurrentPath, extensions);
                Items.Clear();

                foreach (var file in files)
                {
                    Items.Add(file);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilter()
        {
            // Simple filter implementation
            if (string.IsNullOrEmpty(SearchFilter))
            {
                foreach (var item in Items)
                {
                    // Show all items
                }
            }
            else
            {
                // Filter items by name
                foreach (var item in Items)
                {
                    // Hide/show based on filter
                }
            }
        }
    }
}
