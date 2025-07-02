# Installation Guide

This guide will walk you through installing **Windows System Cleaner Pro** on your system.

## 📋 System Requirements

### Minimum Requirements
- **Operating System**: Windows 10 (version 1903) or Windows 11
- **Architecture**: x64 (64-bit processor)
- **RAM**: 2 GB available memory
- **Storage**: 100 MB free disk space (plus temporary space for cleaning)
- **Permissions**: Administrator rights for installation and system cleaning

### Recommended Requirements
- **RAM**: 4 GB or more
- **Storage**: 500 MB free space for optimal operation
- **Display**: 1920x1080 resolution or higher

## 🚀 Installation Methods

### Method 1: Automatic Installer (Recommended)

1. **Download the Installer**
   - Visit the [Releases page](https://github.com/gabrielsk12/System-Cleaner/releases/latest)
   - Download `WindowsCleanerPro_Setup.exe`

2. **Run the Installer**
   - Right-click `WindowsCleanerPro_Setup.exe`
   - Select **"Run as administrator"**
   - If Windows SmartScreen appears, click **"More info"** → **"Run anyway"**

3. **Follow Installation Wizard**
   - Welcome screen: Click **"Next"**
   - License agreement: Read and click **"I Agree"**
   - Components: Choose installation options (all recommended)
   - Install location: Default is recommended (`C:\Program Files\Windows System Cleaner Pro`)
   - Installation: Wait for files to copy
   - Finish: Check **"Run Windows System Cleaner Pro"** and click **"Finish"**

### Method 2: Silent Installation

For system administrators deploying to multiple machines:

```cmd
WindowsCleanerPro_Setup.exe /S /D=C:\Program Files\Windows System Cleaner Pro
```

Parameters:
- `/S` - Silent installation (no user interface)
- `/D=path` - Installation directory (must be last parameter)

## ⚙️ Installation Components

### Core Application (Required)
- Main executable (`WindowsCleaner.exe`)
- Required libraries and dependencies
- Documentation files (README, LICENSE, SETUP guide)
- Application data directory creation

### Shortcuts (Optional)
- **Desktop Shortcut**: Quick access from desktop
- **Start Menu Shortcuts**: Application, uninstaller, and user guide
- **Quick Launch Shortcut**: Taskbar quick access

### Automatic Dependencies
The installer automatically detects and installs:
- **.NET 8.0 Runtime**: If not present, downloads and installs
- **Visual C++ Redistributable**: Ensures compatibility
- **Windows Updates**: May prompt for system updates if needed

## 🔧 Post-Installation Setup

### First Launch
1. **Start the application** from desktop shortcut or Start Menu
2. **Grant administrator permissions** when Windows UAC prompts
3. **Choose your preferred theme** (Light/Dark/System)
4. **Configure initial settings** in the Settings tab

### Optional Configuration
- **Startup with Windows**: Enable in Settings → General
- **Notification preferences**: Configure in Settings → Notifications
- **Scheduled cleaning**: Set up automated maintenance in Settings → Schedule

## 🛡️ Security Considerations

### Administrator Privileges
- Required for system-level file access
- Cleaning system directories and registry
- Installing/removing Windows components
- Managing scheduled tasks

### Windows Defender/Antivirus
- The application is digitally signed (in production versions)
- May trigger false positives due to system-level access
- Add to antivirus exclusions if needed:
  - `C:\Program Files\Windows System Cleaner Pro\`
  - `%LOCALAPPDATA%\WindowsCleanerPro\`

### Firewall Configuration
No special firewall configuration needed - the application:
- Does not require internet access for core functionality
- Only connects for dependency downloads during installation
- All data remains on your local system

## 🚨 Troubleshooting Installation

### Common Issues

#### "This app can't run on your PC"
- **Cause**: 32-bit Windows or older Windows version
- **Solution**: Requires 64-bit Windows 10 (1903+) or Windows 11

#### "Windows protected your PC"
- **Cause**: Windows SmartScreen blocking unknown publisher
- **Solution**: Click "More info" → "Run anyway"

#### ".NET Runtime not found"
- **Cause**: Missing .NET 8.0 runtime
- **Solution**: Installer should handle this automatically. If not, download from [Microsoft .NET](https://dotnet.microsoft.com/download/dotnet/8.0)

#### "Access denied" during installation
- **Cause**: Insufficient privileges
- **Solution**: Right-click installer → "Run as administrator"

#### Installation hangs or freezes
- **Cause**: Antivirus interference or system issues
- **Solution**: 
  1. Temporarily disable antivirus
  2. Restart computer and try again
  3. Run Windows Update first

### Error Codes

| Code | Description | Solution |
|------|-------------|----------|
| 1603 | Fatal error during installation | Run as administrator, check disk space |
| 1618 | Another installation in progress | Wait or restart computer |
| 1633 | Platform not supported | Requires 64-bit Windows |
| 2 | File not found | Re-download installer |

## 🗑️ Uninstallation

### Automatic Uninstall
1. **Windows Settings**:
   - Settings → Apps → Apps & features
   - Find "Windows System Cleaner Pro"
   - Click → Uninstall

2. **Control Panel**:
   - Control Panel → Programs → Programs and Features
   - Select "Windows System Cleaner Pro"
   - Click "Uninstall"

3. **Start Menu**:
   - Start Menu → Windows System Cleaner Pro → Uninstall

### What Gets Removed
- Application files and folders
- Desktop and Start Menu shortcuts
- Registry entries
- Scheduled tasks (if created)

### What Stays (Optional Removal)
- User settings and logs in `%LOCALAPPDATA%\WindowsCleanerPro\`
- Custom scheduled tasks (can be manually removed)

The uninstaller will ask if you want to remove personal data and settings.

## 🔄 Updating

### Automatic Updates (Future Feature)
- Built-in update checker (planned)
- Notification of new versions
- One-click update process

### Manual Updates
1. Download latest installer
2. Run new installer (will detect existing installation)
3. Choose to upgrade existing installation
4. Settings and preferences are preserved

## 📞 Installation Support

If you encounter issues:

1. **Check System Requirements**: Ensure your system meets minimum requirements
2. **Review Troubleshooting**: Common solutions above
3. **Check GitHub Issues**: [Search existing issues](https://github.com/gabrielsk12/System-Cleaner/issues)
4. **Create New Issue**: [Report installation problems](https://github.com/gabrielsk12/System-Cleaner/issues/new)

Include in your report:
- Windows version and build
- Error messages (exact text)
- Installation log (if available)
- Antivirus software being used

---

**Next Steps**: Once installed, check out the [Quick Start Tutorial](Quick-Start-Tutorial) to begin using the application!
