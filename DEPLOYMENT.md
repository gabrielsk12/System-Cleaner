# 🚀 Deployment Guide for GitHub

This guide will help you deploy the Windows System Cleaner Pro to your GitHub repository with professional releases.

## 📋 Prerequisites

Before deploying to GitHub, ensure you have:

1. **GitHub Account** - Create one at [github.com](https://github.com) if needed
2. **Git Installed** - Download from [git-scm.com](https://git-scm.com/)
3. **Repository Access** - You should have write access to `https://github.com/gabrielsk12/System-Cleaner`

## 🔧 Setup Instructions

### Step 1: Configure Git (One-time setup)
```bash
# Set your Git identity
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

### Step 2: Connect to GitHub Repository
```bash
# Navigate to your project directory
cd d:\Projects\cleaner

# Initialize git if not already done
git init

# Add the GitHub repository as remote
git remote add origin https://github.com/gabrielsk12/System-Cleaner.git

# Or if remote already exists, update it
git remote set-url origin https://github.com/gabrielsk12/System-Cleaner.git
```

### Step 3: Prepare for Deployment
```bash
# Add all files to staging
git add .

# Commit your changes
git commit -m "feat: Complete Windows System Cleaner Pro implementation

- Advanced system cleaning with 8 categories
- Enhanced file explorer with size analysis
- Dark/Light theme support with system awareness
- Scheduled automatic cleaning with Task Scheduler
- Professional NSIS installer with dependency management
- Modern .NET 8.0 WPF application with MVVM architecture
- Complete documentation and setup guides"

# Push to GitHub
git push -u origin main
```

## 🏗️ Build and Release Process

### Option 1: Local Build and Manual Release

1. **Build the installer locally:**
   ```bash
   # Run the build script as Administrator
   .\build.bat
   ```

2. **Create a release on GitHub:**
   - Go to your repository: `https://github.com/gabrielsk12/System-Cleaner`
   - Click "Releases" → "Create a new release"
   - Tag version: `v1.0.0`
   - Release title: `Windows System Cleaner Pro v1.0.0`
   - Upload the `WindowsCleanerPro_Setup.exe` file
   - Add release notes (see template below)

### Option 2: Automated Build with GitHub Actions

The repository includes GitHub Actions workflow that automatically:
- Builds the application on every push
- Creates releases when you push a tag
- Uploads the installer as a release asset

**To trigger an automated release:**
```bash
# Create and push a version tag
git tag v1.0.0
git push origin v1.0.0
```

## 📝 Release Notes Template

Use this template when creating releases:

```markdown
## Windows System Cleaner Pro v1.0.0

### 🎉 Features
- 🧹 **Advanced System Cleaning** - 8 intelligent cleanup categories
- 📁 **Enhanced File Explorer** - Visual size analysis with percentage bars
- 🎨 **Modern UI** - Dark/Light themes with system awareness
- 📅 **Scheduled Cleaning** - Automated maintenance with Windows Task Scheduler
- 🔧 **Smart Installer** - Professional installer with dependency management

### 📋 System Requirements
- **Operating System:** Windows 10 (1903+) or Windows 11
- **Architecture:** x64 (64-bit)
- **RAM:** 2 GB minimum, 4 GB recommended
- **Storage:** 100 MB for application + temporary space for cleaning
- **Permissions:** Administrator rights (for system cleaning)

### 📥 Installation
1. Download `WindowsCleanerPro_Setup.exe`
2. Right-click and select "Run as administrator"
3. Follow the installation wizard
4. Launch from desktop shortcut or Start Menu

### 🔒 Security
- ✅ Local operation only - no data transmitted externally
- ✅ Administrator verification for system operations
- ✅ Safe deletion with confirmation dialogs
- ✅ Detailed logging for audit trails

### 🛠️ What's New
- Complete rewrite with modern .NET 8.0 WPF
- Professional NSIS installer with Windows integration
- Enhanced performance with async operations
- Improved UI/UX with modern design principles
- Comprehensive documentation and user guides

### 📚 Documentation
- [Setup Guide](SETUP.md) - Installation and usage instructions
- [README](README.md) - Feature overview and technical details

### 🐛 Known Issues
- None currently known

### 💬 Support
For issues, questions, or feature requests, please use the [GitHub Issues](https://github.com/gabrielsk12/System-Cleaner/issues) page.
```

## 🔄 Update Process

When you make updates to the application:

1. **Make your changes** to the code
2. **Test thoroughly** on your local machine
3. **Update version numbers** in:
   - `WindowsCleaner/WindowsCleaner.csproj`
   - `installer.nsi` (VERSIONMAJOR, VERSIONMINOR, VERSIONBUILD)
4. **Commit and push changes:**
   ```bash
   git add .
   git commit -m "feat: Add new feature or fix: Fix specific issue"
   git push origin main
   ```
5. **Create a new release:**
   ```bash
   git tag v1.1.0
   git push origin v1.1.0
   ```

## 📊 Repository Structure

Your GitHub repository will have this structure:
```
System-Cleaner/
├── .github/
│   └── workflows/
│       └── build-release.yml    # Automated build and release
├── WindowsCleaner/              # Main application source
│   ├── Models/
│   ├── ViewModels/
│   ├── Views/
│   ├── Services/
│   ├── Converters/
│   └── WindowsCleaner.csproj
├── build.bat                    # Build script wrapper
├── create_installer.ps1         # PowerShell build system
├── installer.nsi               # NSIS installer script
├── README.md                   # Project overview
├── SETUP.md                    # Setup and usage guide
├── LICENSE                     # MIT License
└── .gitignore                  # Git ignore rules
```

## 🎯 Best Practices

### Commit Messages
Use conventional commit format:
- `feat:` for new features
- `fix:` for bug fixes
- `docs:` for documentation changes
- `style:` for code style changes
- `refactor:` for code refactoring
- `test:` for adding tests
- `chore:` for maintenance tasks

### Version Numbers
Follow semantic versioning (SemVer):
- `MAJOR.MINOR.PATCH` (e.g., 1.0.0)
- MAJOR: Breaking changes
- MINOR: New features (backward compatible)
- PATCH: Bug fixes (backward compatible)

### Security
- Never commit sensitive information (passwords, keys, etc.)
- Use GitHub's security features (Dependabot, security alerts)
- Consider code signing for the installer in production

## 🚀 Go Live Checklist

Before your first public release:

- [ ] All code committed and pushed to GitHub
- [ ] Installer tested on clean Windows systems
- [ ] Documentation reviewed and updated
- [ ] License file included (MIT recommended)
- [ ] Version numbers updated consistently
- [ ] Release notes prepared
- [ ] GitHub repository settings configured
- [ ] Consider enabling GitHub Pages for documentation
- [ ] Set up issue templates for better bug reports

## 🎉 Success!

Once deployed, users can:
1. Visit your GitHub repository
2. Go to the "Releases" section
3. Download the latest `WindowsCleanerPro_Setup.exe`
4. Install and enjoy your professional Windows cleaning tool!

Your application will be professionally distributed with:
- ✅ Professional installer
- ✅ Automatic updates (via GitHub releases)
- ✅ Proper Windows integration
- ✅ Security and safety features
- ✅ Comprehensive documentation
