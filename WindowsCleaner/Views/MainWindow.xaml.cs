using System.Windows;
using WindowsCleaner.ViewModels;

namespace WindowsCleaner.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set up data contexts for each tab
            if (DataContext is MainViewModel mainViewModel)
            {
                // Find tab content controls and set their data contexts
                var fileExplorerTab = FindName("FileExplorerView") as Views.FileExplorerView;
                if (fileExplorerTab != null)
                {
                    fileExplorerTab.DataContext = mainViewModel.FileExplorerViewModel;
                }

                var settingsTab = FindName("SettingsView") as Views.SettingsView;
                if (settingsTab != null)
                {
                    settingsTab.DataContext = mainViewModel.SettingsViewModel;
                }
            }

            // Check if running as administrator
            if (!IsRunningAsAdministrator())
            {
                var result = MessageBox.Show(
                    "This application requires administrator privileges to clean system files effectively.\n\n" +
                    "Would you like to restart as administrator?",
                    "Administrator Rights Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    RestartAsAdministrator();
                    return;
                }
            }
        }

        private static bool IsRunningAsAdministrator()
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }

        private static void RestartAsAdministrator()
        {
            try
            {
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName,
                    Verb = "runas"
                };

                System.Diagnostics.Process.Start(processInfo);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to restart as administrator: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
