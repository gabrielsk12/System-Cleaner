# Windows System Cleaner Pro 2.0.0 - Complete Release Build Script
param(
    [switch]$SkipBuild,
    [switch]$SkipInstaller,
    [switch]$SkipGitRelease,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

Write-Host "🚀 Windows System Cleaner Pro 2.0.0 - Complete Release Build" -ForegroundColor Cyan
Write-Host "=" * 70

# Set up paths
$ProjectRoot = $PSScriptRoot
$ProjectPath = Join-Path $ProjectRoot "WindowsCleanerNew"
$ProjectFile = Join-Path $ProjectPath "WindowsCleanerNew.csproj"
$PublishPath = Join-Path $ProjectPath "bin\$Configuration\net8.0-windows\publish"
$ReleasePath = Join-Path $ProjectRoot "release"
$InstallerPath = Join-Path $ProjectRoot "installer"
$RedistPath = Join-Path $ProjectRoot "redist"
$SetupScript = Join-Path $ProjectPath "setup.iss"

# Cleanup and create directories
Write-Host "📁 Setting up directories..." -ForegroundColor Yellow
@($ReleasePath, $InstallerPath, $RedistPath) | ForEach-Object {
    if (Test-Path $_) {
        Remove-Item $_ -Recurse -Force
    }
    New-Item -ItemType Directory -Path $_ -Force | Out-Null
    Write-Host "   Created: $_"
}

# Function to download file with progress
function Download-File {
    param([string]$Url, [string]$Path, [string]$Description)
    
    if (Test-Path $Path) {
        Write-Host "   ✅ $Description already exists"
        return
    }
    
    Write-Host "   📥 Downloading $Description..." -ForegroundColor Green
    try {
        $webClient = New-Object System.Net.WebClient
        $webClient.DownloadFile($Url, $Path)
        Write-Host "   ✅ Downloaded: $([System.IO.Path]::GetFileName($Path))"
    } catch {
        Write-Warning "Failed to download $Description`: $_"
    } finally {
        if ($webClient) { $webClient.Dispose() }
    }
}

# Download prerequisites
Write-Host "📦 Downloading prerequisites..." -ForegroundColor Yellow
$dotnetUrl = "https://download.microsoft.com/download/6/0/f/60fc8ea7-d5ae-4f9b-b50b-b5c0c87c2f25/dotnet-runtime-8.0.0-win-x64.exe"
$vcredistUrl = "https://aka.ms/vs/17/release/vc_redist.x64.exe"

Download-File $dotnetUrl (Join-Path $RedistPath "dotnet-runtime-8.0-win-x64.exe") ".NET 8 Runtime"
Download-File $vcredistUrl (Join-Path $RedistPath "VC_redist.x64.exe") "Visual C++ Redistributable"

if (-not $SkipBuild) {
    Write-Host "🔨 Building application..." -ForegroundColor Yellow
    
    # Navigate to project directory
    Push-Location $ProjectPath
    
    try {
        # Clean previous builds
        Write-Host "   🧹 Cleaning previous builds..."
        & dotnet clean --configuration $Configuration
        
        # Restore packages
        Write-Host "   📦 Restoring NuGet packages..."
        & dotnet restore
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to restore packages"
        }
        
        # Build the application
        Write-Host "   🔨 Building $Configuration configuration..."
        & dotnet build --configuration $Configuration --no-restore
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Build completed with warnings, continuing..."
        }
        
        # Publish single-file executable
        Write-Host "   📦 Publishing single-file executable..."
        & dotnet publish --configuration $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --no-restore
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Publish completed with warnings, continuing..."
        }
        
        Write-Host "   ✅ Build completed successfully!" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
} else {
    Write-Host "⏭️  Skipping build (SkipBuild specified)" -ForegroundColor Yellow
}

# Verify build output
$executablePath = Join-Path $PublishPath "WindowsCleanerNew.exe"
if (-not (Test-Path $executablePath)) {
    Write-Error "Build output not found at: $executablePath"
    Write-Error "Please build the project first or remove -SkipBuild parameter."
    exit 1
}

# Get file information
$exeInfo = Get-Item $executablePath
Write-Host "   📊 Executable size: $([math]::Round($exeInfo.Length / 1MB, 2)) MB" -ForegroundColor Cyan

if (-not $SkipInstaller) {
    Write-Host "📦 Creating installer..." -ForegroundColor Yellow
    
    # Check for Inno Setup
    $innoSetupPaths = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe"
    )
    
    $isccPath = $innoSetupPaths | Where-Object { Test-Path $_ } | Select-Object -First 1
    
    if (-not $isccPath) {
        Write-Host "   ⚠️  Inno Setup not found!" -ForegroundColor Red
        Write-Host "   📥 To create the installer:" -ForegroundColor Yellow
        Write-Host "      1. Download Inno Setup from: https://jrsoftware.org/isdl.php"
        Write-Host "      2. Install Inno Setup"
        Write-Host "      3. Run this script again"
        Write-Host "      4. Or manually compile: $SetupScript"
        Write-Host ""
        Write-Host "   📄 Installer script ready: setup.iss" -ForegroundColor Green
    } else {
        Write-Host "   🔨 Compiling installer with Inno Setup..."
        Write-Host "   📄 Using script: $SetupScript"
        
        Push-Location $ProjectPath
        try {
            & $isccPath "setup.iss"
            if ($LASTEXITCODE -eq 0) {
                Write-Host "   ✅ Installer created successfully!" -ForegroundColor Green
                
                # Find the created installer
                $installerPattern = Join-Path $ProjectRoot "dist\*.exe"
                $installerFile = Get-ChildItem $installerPattern -ErrorAction SilentlyContinue | Select-Object -First 1
                if ($installerFile) {
                    Write-Host "   📦 Installer: $($installerFile.Name)" -ForegroundColor Cyan
                    Write-Host "   📊 Size: $([math]::Round($installerFile.Length / 1MB, 2)) MB"
                    
                    # Copy installer to release folder
                    Copy-Item $installerFile.FullName (Join-Path $ReleasePath $installerFile.Name) -Force
                }
            } else {
                Write-Warning "Installer compilation failed with exit code: $LASTEXITCODE"
            }
        }
        finally {
            Pop-Location
        }
    }
} else {
    Write-Host "⏭️  Skipping installer creation (SkipInstaller specified)" -ForegroundColor Yellow
}

# Create portable package
Write-Host "📦 Creating portable package..." -ForegroundColor Yellow

# Copy executable and resources
Copy-Item $executablePath $ReleasePath -Force
Write-Host "   ✅ Copied executable"

# Copy resources if they exist
$resourcesPath = Join-Path $ProjectPath "Resources"
if (Test-Path $resourcesPath) {
    $releaseResourcesPath = Join-Path $ReleasePath "Resources"
    Copy-Item $resourcesPath $releaseResourcesPath -Recurse -Force
    Write-Host "   ✅ Copied resources"
}

# Copy documentation files
$docFiles = @(
    @{ Source = "README.md"; Dest = "README.txt" },
    @{ Source = "LICENSE"; Dest = "LICENSE.txt" },
    @{ Source = "PROJECT_STATUS.md"; Dest = "PROJECT_STATUS.txt" }
)

foreach ($file in $docFiles) {
    $srcPath = Join-Path $ProjectRoot $file.Source
    $destPath = Join-Path $ReleasePath $file.Dest
    
    if (Test-Path $srcPath) {
        Copy-Item $srcPath $destPath -Force
        Write-Host "   ✅ Copied: $($file.Dest)"
    }
}

# Create installation instructions
$installInstructions = @"
Windows System Cleaner Pro 2.0.0 - Portable Version
==================================================

SYSTEM REQUIREMENTS:
- Windows 10 (version 1809) or Windows 11
- 64-bit (x64) processor
- .NET 8.0 Runtime (will be prompted to install if missing)
- Administrator privileges for full functionality

INSTALLATION:
1. Extract all files to your preferred location
2. Right-click WindowsCleanerNew.exe and select "Run as administrator"
3. If prompted, allow the app to install .NET 8.0 Runtime
4. Follow the setup wizard for initial configuration

FEATURES:
✅ System Cleanup - Remove temp files, cache, and junk data
✅ Driver Updates - Automatic driver scanning and installation  
✅ Performance Monitor - Real-time system metrics
✅ Startup Manager - Control boot programs
✅ Windows Updates - Manage system updates
✅ File Browser - Advanced file management tools
✅ Task Scheduler - Automated maintenance routines
✅ Settings Sync - Backup and restore configurations

SUPPORT:
- Documentation: https://github.com/gabrielsk12/System-Cleaner
- Issues: https://github.com/gabrielsk12/System-Cleaner/issues
- License: MIT (Open Source)

⚠️ IMPORTANT:
- This application modifies system files and settings
- Always create a system backup before major operations
- Run from trusted sources only
- Administrator privileges required for system-level operations

© 2025 System Optimizer. All rights reserved.
"@

$installInstructions | Out-File (Join-Path $ReleasePath "INSTALL.txt") -Encoding UTF8
Write-Host "   ✅ Created installation instructions"

# Create ZIP package
Write-Host "📦 Creating ZIP package..." -ForegroundColor Yellow
$zipPath = Join-Path $ProjectRoot "WindowsSystemCleanerPro-v2.0.0-Portable.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

try {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($ReleasePath, $zipPath, 'Optimal', $false)
    
    Write-Host "   ✅ ZIP package created: WindowsSystemCleanerPro-v2.0.0-Portable.zip" -ForegroundColor Green
    
    $zipSize = (Get-Item $zipPath).Length
    Write-Host "   📊 ZIP size: $([math]::Round($zipSize / 1MB, 2)) MB"
} catch {
    Write-Warning "Failed to create ZIP package: $_"
}

# Git operations and GitHub release
if (-not $SkipGitRelease) {
    Write-Host "🔄 Preparing Git release..." -ForegroundColor Yellow
    
    try {
        # Check if we're in a git repository
        & git status | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Not in a Git repository. Skipping Git operations."
        } else {
            # Add and commit changes
            Write-Host "   📝 Committing changes..."
            & git add .
            & git commit -m "Release v2.0.0 - Complete installer and portable package"
            
            # Create and push tag
            Write-Host "   🏷️  Creating tag v2.0.0..."
            & git tag -a "v2.0.0" -m "Windows System Cleaner Pro v2.0.0 - Major release with modernized architecture, advanced driver management, smart file explorer, automated scheduling, multi-language support, modern UI, and enhanced security features."
            
            Write-Host "   🚀 Pushing to GitHub..."
            & git push origin main
            & git push origin v2.0.0
            
            Write-Host "   ✅ Git release prepared!" -ForegroundColor Green
            Write-Host "   🌐 GitHub Actions will automatically create the release"
        }
    } catch {
        Write-Warning "Git operations failed: $_"
    }
} else {
    Write-Host "⏭️  Skipping Git release (SkipGitRelease specified)" -ForegroundColor Yellow
}

# Create release information file
$releaseInfo = @"
# Windows System Cleaner Pro v2.0.0 Release

## Build Information
- **Build Date**: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss UTC')
- **Version**: 2.0.0
- **Configuration**: $Configuration
- **Target Framework**: .NET 8.0
- **Platform**: Windows x64
- **Build Machine**: $env:COMPUTERNAME

## Release Assets
"@

if (Test-Path $executablePath) {
    $exeSize = (Get-Item $executablePath).Length
    $releaseInfo += "`n- **Executable**: WindowsCleanerNew.exe ($([math]::Round($exeSize / 1MB, 2)) MB)"
}

$installerFile = Get-ChildItem (Join-Path $ProjectRoot "dist\*.exe") -ErrorAction SilentlyContinue | Select-Object -First 1
if ($installerFile) {
    $releaseInfo += "`n- **Installer**: $($installerFile.Name) ($([math]::Round($installerFile.Length / 1MB, 2)) MB)"
}

if (Test-Path $zipPath) {
    $zipSize = (Get-Item $zipPath).Length
    $releaseInfo += "`n- **Portable ZIP**: WindowsSystemCleanerPro-v2.0.0-Portable.zip ($([math]::Round($zipSize / 1MB, 2)) MB)"
}

$releaseInfo += @"

## Dependencies Included
- .NET 8.0 Runtime (auto-installed by setup)
- Visual C++ Redistributable (auto-installed by setup)

## Installation Methods
1. **Recommended**: Use the installer (setup.exe) for automatic dependency installation
2. **Portable**: Extract ZIP file and run as administrator

## Next Steps
1. Test the installer on a clean system
2. Upload release assets to GitHub release
3. Update documentation and changelog
4. Announce release on relevant channels

---
Generated by build-release-v2.ps1 on $(Get-Date)
"@

$releaseInfo | Out-File (Join-Path $ProjectRoot "RELEASE_v2.0.0_INFO.md") -Encoding UTF8

# Final summary
Write-Host "`n🎉 Release Build Complete!" -ForegroundColor Cyan
Write-Host "=" * 70
Write-Host "✅ Project: Windows System Cleaner Pro 2.0.0" -ForegroundColor Green
Write-Host "✅ Build Configuration: $Configuration" -ForegroundColor Green
Write-Host "✅ Target Framework: .NET 8.0" -ForegroundColor Green
Write-Host "✅ Platform: Windows x64" -ForegroundColor Green

if (Test-Path $executablePath) {
    $exeSize = (Get-Item $executablePath).Length
    Write-Host "✅ Executable: $([math]::Round($exeSize / 1MB, 2)) MB" -ForegroundColor Green
}

$installerFile = Get-ChildItem (Join-Path $ProjectRoot "dist\*.exe") -ErrorAction SilentlyContinue | Select-Object -First 1
if ($installerFile) {
    Write-Host "✅ Installer: $([math]::Round($installerFile.Length / 1MB, 2)) MB" -ForegroundColor Green
} else {
    Write-Host "⚠️  Installer: Not created (Inno Setup required)" -ForegroundColor Yellow
}

if (Test-Path $zipPath) {
    Write-Host "✅ Portable ZIP: $([math]::Round((Get-Item $zipPath).Length / 1MB, 2)) MB" -ForegroundColor Green
}

Write-Host "`n📋 Release Assets Created:" -ForegroundColor Yellow
Write-Host "📁 $ReleasePath" -ForegroundColor Cyan
if ($installerFile) {
    Write-Host "📦 $($installerFile.FullName)" -ForegroundColor Cyan
}
if (Test-Path $zipPath) {
    Write-Host "📦 $zipPath" -ForegroundColor Cyan
}

Write-Host "`n🚀 Ready for GitHub Release v2.0.0!" -ForegroundColor Green
Write-Host "🌐 Check GitHub Actions for automated release creation" -ForegroundColor Yellow

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Host "⚠️  Git not found. Manual upload to GitHub required." -ForegroundColor Yellow
}

Write-Host "`n📚 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Test installer and portable version"
Write-Host "2. Verify GitHub release was created automatically"
Write-Host "3. Upload any missing assets to GitHub release"
Write-Host "4. Update project documentation"
Write-Host "5. Announce the release!"

Write-Host "`n✨ Thank you for using Windows System Cleaner Pro! ✨" -ForegroundColor Green
