using System.Windows;
using WindowsCleaner.ViewModels;
using WindowsCleaner.Views;

namespace WindowsCleaner
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Set up the main window
            var mainWindow = new MainWindow();
            var viewModel = new MainViewModel();
            mainWindow.DataContext = viewModel;
            mainWindow.Show();
        }
    }
}
