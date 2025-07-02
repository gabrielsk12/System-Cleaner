# 🧹 Windows System Cleaner Pro

A professional-grade Windows system optimization tool built with modern C# WPF technology. Clean your system, manage files intelligently, and schedule automatic maintenance - all with a beautiful, user-friendly interface.

![Windows System Cleaner Pro](https://img.shields.io/badge/Windows-10%2F11-blue?style=for-the-badge&logo=windows)
![.NET](https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge&logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

## ✨ Key Features

### 🧹 **Advanced System Cleaning**
- **8 Smart Cleanup Categories** - Windows Updates, Temp files, Cache, Logs, and more
- **Real-time Progress Tracking** - Watch your system get optimized live
- **Size Analysis** - See exactly how much space you'll reclaim
- **Safe Operation** - Confirmations and protections for critical files

### 📁 **Enhanced File Explorer**
- **Visual Size Analysis** - Percentage bars show folder space usage
- **Hierarchical Navigation** - Expand/collapse folder structures
- **Quick Actions** - Delete, properties, and file management
- **Smart Search** - Find files and folders instantly
- **Drive Overview** - Complete system storage visualization

### ⚙️ **Professional Settings**
- **Theme Engine** - Light, Dark, and System-aware themes
- **Startup Control** - Launch with Windows options
- **Smart Notifications** - Stay informed of operations
- **Safety Controls** - Configurable confirmation dialogs

### 📅 **Intelligent Scheduling**
- **Automated Cleaning** - Set it and forget it maintenance
- **Flexible Timing** - Daily, Weekly, or Monthly schedules
- **Precision Control** - Choose exact times and days
- **Task Scheduler Integration** - Native Windows scheduling

### 🔧 **Auto-Dependency Management**
- **Smart Detection** - Automatically checks for required components
- **.NET Runtime** - Installs .NET 8.0 if missing
- **VC++ Redistributable** - Ensures compatibility
- **Progress Tracking** - Real-time installation updates

## 🚀 Quick Start

### Easy Installation (Recommended)
```batch
# 1. Clone or download the project
git clone https://github.com/yourusername/windows-cleaner.git

# 2. Run installer as Administrator
Right-click build.bat → "Run as administrator"

# 3. Launch from desktop shortcut or Start Menu
```

### What the Installer Does
- ✅ Builds the application automatically
- ✅ Installs .NET 8.0 if needed
- ✅ Creates desktop and Start Menu shortcuts
- ✅ Registers in Programs and Features
- ✅ Sets up uninstaller
- ✅ Configures Windows firewall exceptions

## 📸 Screenshots

### System Cleaner Interface
Clean interface showing 8 cleanup categories with real-time scanning progress and space analysis.

### Enhanced File Explorer
Visual folder size analysis with percentage bars, hierarchical navigation, and quick actions.

### Advanced Settings
Theme switching, scheduled cleaning configuration, and safety settings.

## 🛠️ Technical Specifications

### Built With Modern Technology
- **Framework:** .NET 8.0 WPF
- **Architecture:** MVVM (Model-View-ViewModel)
- **UI:** Modern Windows design with dark/light themes
- **Dependencies:** TaskScheduler, System.Management, WindowsAPICodePack

### System Requirements
- **OS:** Windows 10 (1903+) or Windows 11
- **Architecture:** x64 (64-bit)
- **RAM:** 2 GB minimum, 4 GB recommended
- **Storage:** 100 MB + cleaning space
- **Permissions:** Administrator rights for system cleaning

## 🔒 Security & Privacy

### Safe & Secure
- **Local Operation Only** - No data transmitted externally
- **Administrator Verification** - Secure system-level operations
- **Confirmation Dialogs** - Protection against accidental deletion
- **Detailed Logging** - Complete operation audit trail

### Privacy Focused
- **No Telemetry** - Your data stays on your machine
- **Selective Cleaning** - You control what gets deleted
- **Settings Privacy** - Configuration stored locally
- **Open Source** - Fully transparent operation

## 📋 Feature Comparison

| Feature | Basic Cleaners | Windows Cleaner Pro |
|---------|---------------|-------------------|
| System Cleaning | ✅ | ✅ **8 Categories** |
| File Explorer | ❌ | ✅ **Enhanced with Size Analysis** |
| Dark/Light Themes | ❌ | ✅ **System-Aware Themes** |
| Scheduled Cleaning | ❌ | ✅ **Full Task Scheduler Integration** |
| Dependency Management | ❌ | ✅ **Auto-Install .NET & VC++** |
| Progress Tracking | ⚠️ Basic | ✅ **Real-time with Details** |
| Safety Features | ⚠️ Limited | ✅ **Comprehensive Protection** |
| Professional Installer | ❌ | ✅ **Full Windows Integration** |

## 🎯 Use Cases

### 🏠 **Home Users**
- Regular system maintenance
- Free up storage space
- Improve system performance
- Easy-to-use interface

### 🏢 **IT Professionals**
- Automated system cleaning
- Batch operations
- Detailed logging
- Scheduled maintenance

### 💻 **Power Users**
- Advanced file management
- Custom cleaning schedules
- System optimization
- Performance monitoring

## 📚 Documentation

- **[Complete Setup Guide](SETUP.md)** - Detailed installation and configuration
- **[User Manual](SETUP.md#usage-instructions)** - How to use all features
- **[Troubleshooting](SETUP.md#troubleshooting)** - Common issues and solutions
- **[Technical Details](SETUP.md#technical-architecture)** - Architecture and dependencies

## 🤝 Contributing

We welcome contributions! Please see our contributing guidelines:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

### Development Setup
```bash
# Clone the repository
git clone https://github.com/yourusername/windows-cleaner.git
cd windows-cleaner

# Install .NET 8.0 SDK
# https://dotnet.microsoft.com/download/dotnet/8.0

# Build the project
dotnet build

# Run the application
dotnet run --project WindowsCleaner
```

## 📈 Roadmap

### Version 1.1 (Planned)
- [ ] Automatic update system
- [ ] Plugin architecture
- [ ] Advanced file filtering
- [ ] Network drive support

### Version 1.2 (Future)
- [ ] Multi-language support
- [ ] Cloud backup integration
- [ ] Advanced analytics
- [ ] Custom cleaning scripts

## 🐛 Bug Reports & Support

### Reporting Issues
- **GitHub Issues** - For bugs and feature requests
- **Email Support** - For general questions
- **Documentation** - Check SETUP.md first

### System Logs
- Application logs: `%LOCALAPPDATA%\WindowsCleanerPro\logs\`
- Settings file: `%LOCALAPPDATA%\WindowsCleanerPro\settings.json`
- Windows Event Viewer for system-level issues

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Microsoft for .NET framework and WPF
- TaskScheduler library contributors
- Windows API Code Pack maintainers
- The open-source community

---

## ⭐ Star This Project

If you find Windows System Cleaner Pro useful, please consider giving it a star! It helps others discover this tool and motivates continued development.

**Windows System Cleaner Pro** - Professional system optimization made simple.

*Built with ❤️ for the Windows community*
