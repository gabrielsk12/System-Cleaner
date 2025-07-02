# Windows System Cleaner Pro - Complete Setup Guide

## 🚀 Overview
Windows System Cleaner Pro is a professional-grade system optimization tool built with modern C# WPF technology. It provides comprehensive system cleaning, enhanced file management, and advanced scheduling capabilities.

## ✨ Features

### 🧹 Advanced System Cleaning
- **8 Cleanup Categories:**
  - Windows Update files
  - Temporary files
  - Recycle Bin
  - System cache
  - Browser cache
  - Log files
  - Error reports
  - Thumbnail cache

- **Smart Selection:** Choose exactly what to clean
- **Real-time Progress:** Live scanning and cleaning progress
- **Size Analysis:** See how much space each category uses
- **Safe Deletion:** Confirmation dialogs for important operations

### 📁 Enhanced File Explorer
- **Folder Size Analysis:** See exact folder sizes and percentages
- **Visual Size Bars:** Graphical representation of space usage
- **Quick Navigation:** Home, back, refresh buttons
- **Search Functionality:** Find files and folders quickly
- **Quick Actions:** Delete, properties, and more
- **Hierarchical View:** Expand/collapse folder structures

### ⚙️ Advanced Settings
- **Theme Switching:** Light, Dark, and System themes
- **Startup Control:** Launch with Windows option
- **Notification Settings:** Control system notifications
- **Confirmation Dialogs:** Safety settings for file operations
- **Log Management:** Configure maximum log file retention

### 📅 Scheduled Cleaning
- **Automatic Cleaning:** Set up recurring cleanup schedules
- **Flexible Timing:** Daily, Weekly, or Monthly schedules
- **Custom Time:** Choose exact time for cleaning
- **Day Selection:** Pick specific days (weekly) or dates (monthly)
- **Windows Task Scheduler Integration:** Native Windows scheduling

### 🔧 Smart Dependency Installation
- **Automatic Detection:** Checks for required dependencies
- **.NET Runtime:** Installs .NET 8.0 if missing
- **Visual C++ Redistributable:** Ensures compatibility
- **Progress Tracking:** Real-time installation progress

## 📋 System Requirements

### Minimum Requirements
- **Operating System:** Windows 10 (1903) or later / Windows 11
- **Architecture:** x64 (64-bit)
- **RAM:** 2 GB minimum, 4 GB recommended
- **Storage:** 100 MB for application + temporary space for cleaning
- **Permissions:** Administrator rights (for system cleaning)

### Development Requirements (if building from source)
- **.NET 8.0 SDK** or later
- **Visual Studio 2022** or **Visual Studio Code** (optional)
- **Git** (for source control)

## 🛠️ Installation Guide

### Option 1: Easy Installation (Recommended)
1. **Download** the installer from the release page
2. **Right-click** the installer and select "Run as administrator"
3. **Follow** the installation wizard
4. The installer will automatically:
   - Build the application
   - Install dependencies if needed
   - Create desktop and start menu shortcuts
   - Register in Programs and Features
   - Set up uninstaller

### Option 2: Manual Installation
1. **Clone** the repository:
   ```bash
   git clone https://github.com/yourusername/windows-cleaner.git
   cd windows-cleaner
   ```

2. **Install .NET 8.0 SDK** from [Microsoft's website](https://dotnet.microsoft.com/download/dotnet/8.0)

3. **Run the installer:**
   ```batch
   # Right-click and "Run as administrator"
   build.bat
   ```

## 🎯 Usage Instructions

### First Launch
1. **Start the application** from desktop shortcut or Start Menu
2. **Grant administrator permissions** when prompted
3. **Choose your theme** in Settings (optional)
4. **Configure preferences** in the Settings tab

### System Cleaning
1. **Navigate** to the "System Cleaner" tab
2. **Select categories** you want to clean (all selected by default)
3. **Click "Scan System"** to analyze your system
4. **Review the results** - see files found and space to reclaim
5. **Click "Clean Selected"** to remove files
6. **Monitor progress** in real-time

### File Explorer
1. **Go to** the "File Explorer" tab
2. **Browse** drives and folders
3. **View size information** with visual bars
4. **Use search** to find specific files/folders
5. **Right-click** for context menu options
6. **Delete** unwanted files and folders

### Scheduled Cleaning
1. **Open** the "Settings" tab
2. **Navigate** to "Scheduled Cleaning" section
3. **Enable** automatic cleaning
4. **Choose frequency:** Daily, Weekly, or Monthly
5. **Set time** for cleaning (e.g., 2:00 AM)
6. **Select day** (for weekly/monthly schedules)
7. **Save settings** - Windows Task Scheduler will handle the rest

### Theme Switching
1. **Go to** Settings → Appearance
2. **Choose theme:**
   - **Light:** Clean, bright interface
   - **Dark:** Easy on the eyes, professional look
   - **System:** Follows Windows theme

## 🔒 Security Features

### Safe Operation
- **Administrator verification** for system-level operations
- **Confirmation dialogs** before deleting files
- **Detailed logging** of all operations
- **Rollback protection** for critical system files

### Privacy Protection
- **Local operation only** - no data sent to external servers
- **Selective cleaning** - you control what gets deleted
- **Secure deletion** of temporary and cache files
- **Settings stored locally** in user profile

## 🗑️ Uninstallation

### Automatic Uninstall
1. **Open** Settings → Apps & features
2. **Find** "Windows System Cleaner Pro"
3. **Click** Uninstall
4. **Follow** the prompts

### Manual Uninstall
1. **Run** the uninstaller: `%LOCALAPPDATA%\\WindowsCleanerPro\\uninstall.bat`
2. **Or delete** the installation folder manually

## 🛠️ Troubleshooting

### Common Issues

#### "Access Denied" Errors
- **Solution:** Always run as administrator
- **Reason:** System cleaning requires elevated permissions

#### .NET Runtime Missing
- **Solution:** The app will install it automatically on first run
- **Manual:** Download from [Microsoft .NET](https://dotnet.microsoft.com/download)

#### Schedule Not Working
- **Check:** Windows Task Scheduler service is running
- **Verify:** User has permissions to create scheduled tasks
- **Solution:** Re-save schedule settings

#### Theme Not Changing
- **Restart** the application after theme change
- **Check** Windows display settings for system theme

### Support
- **Documentation:** Check README.md in installation folder
- **Logs:** Review application logs in `%LOCALAPPDATA%\\WindowsCleanerPro\\logs`
- **Issues:** Report bugs on the project repository

## 🔄 Updates
- **Automatic checking:** Planned for future versions
- **Manual updates:** Download new version and run installer
- **Settings preservation:** Your preferences are kept during updates

## 📊 Performance Tips

### Optimal Usage
- **Run weekly** scheduled cleaning for best performance
- **Use file explorer** to identify large, unnecessary files
- **Enable notifications** to stay informed of cleanup results
- **Regular maintenance:** Check settings monthly

### System Optimization
- **Combine** with Windows Disk Cleanup for maximum effect
- **Monitor** available disk space regularly
- **Consider** moving large files to external storage
- **Defragment** traditional hard drives after major cleanups

## 🏗️ Technical Architecture

### Built With
- **Framework:** .NET 8.0 WPF
- **Pattern:** MVVM (Model-View-ViewModel)
- **Dependencies:** 
  - TaskScheduler (Windows Task Scheduler integration)
  - System.Management (WMI queries)
  - Microsoft.WindowsAPICodePack (Shell integration)
  - System.Text.Json (Settings persistence)

### Project Structure
```
WindowsCleaner/
├── Models/           # Data models and enums
├── ViewModels/       # MVVM view models
├── Views/           # WPF user interfaces
├── Services/        # Business logic services
├── Converters/      # Data binding converters
└── App.xaml        # Application resources and styles
```

---

**Windows System Cleaner Pro** - Professional system optimization made simple.

*For more information, visit the project repository or contact support.*
