# Windows System Cleaner Pro - Professional Build and Installer Creation Script
# This script builds the application and creates a proper installer executable

param(
    [switch]$SkipBuild,
    [switch]$OnlyBuild,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# Configuration
$ProjectName = "WindowsCleaner"
$SolutionFile = "WindowsCleaner.sln"
$OutputDir = "Build"
$InstallerScript = "installer.nsi"
$InstallerOutput = "WindowsCleanerPro_Setup.exe"

Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "  Windows System Cleaner Pro - Professional Build System" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host ""

# Function to check if a command exists
function Test-Command($cmdname) {
    return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
}

# Function to download and install NSIS if not present
function Install-NSIS {
    $nsisPath = "${env:ProgramFiles(x86)}\NSIS\makensis.exe"
    if (Test-Path $nsisPath) {
        Write-Host "✅ NSIS found at: $nsisPath" -ForegroundColor Green
        return $nsisPath
    }
    
    Write-Host "🔍 NSIS not found. Attempting to install..." -ForegroundColor Yellow
    
    # Try to install via Chocolatey if available
    if (Test-Command "choco") {
        Write-Host "📦 Installing NSIS via Chocolatey..." -ForegroundColor Yellow
        try {
            choco install nsis -y
            if (Test-Path $nsisPath) {
                Write-Host "✅ NSIS installed successfully!" -ForegroundColor Green
                return $nsisPath
            }
        } catch {
            Write-Host "⚠️ Chocolatey installation failed." -ForegroundColor Yellow
        }
    }
    
    # Try to install via winget if available
    if (Test-Command "winget") {
        Write-Host "📦 Installing NSIS via winget..." -ForegroundColor Yellow
        try {
            winget install NSIS.NSIS
            Start-Sleep 5
            if (Test-Path $nsisPath) {
                Write-Host "✅ NSIS installed successfully!" -ForegroundColor Green
                return $nsisPath
            }
        } catch {
            Write-Host "⚠️ winget installation failed." -ForegroundColor Yellow
        }
    }
    
    # Manual download and install
    Write-Host "📥 Downloading NSIS manually..." -ForegroundColor Yellow
    $nsisUrl = "https://sourceforge.net/projects/nsis/files/NSIS%203/3.09/nsis-3.09-setup.exe/download"
    $nsisInstaller = "$env:TEMP\nsis-setup.exe"
    
    try {
        Invoke-WebRequest -Uri $nsisUrl -OutFile $nsisInstaller -UseBasicParsing
        Write-Host "🔧 Installing NSIS... Please wait and follow the installer prompts." -ForegroundColor Yellow
        Start-Process -FilePath $nsisInstaller -Wait
        Remove-Item $nsisInstaller -ErrorAction SilentlyContinue
        
        if (Test-Path $nsisPath) {
            Write-Host "✅ NSIS installed successfully!" -ForegroundColor Green
            return $nsisPath
        }
    } catch {
        Write-Host "❌ Failed to download/install NSIS automatically." -ForegroundColor Red
        Write-Host "Please download and install NSIS manually from: https://nsis.sourceforge.io/" -ForegroundColor Yellow
        return $null
    }
    
    return $null
}

# Check administrator privileges
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Host "⚠️ Administrator privileges recommended for best results." -ForegroundColor Yellow
    Write-Host "   Some features may require elevated permissions." -ForegroundColor Yellow
    Write-Host ""
}

if (-not $SkipBuild) {
    # Check .NET SDK
    Write-Host "🔍 Checking .NET SDK..." -ForegroundColor Yellow
    if (-not (Test-Command "dotnet")) {
        Write-Host "❌ .NET SDK not found!" -ForegroundColor Red
        Write-Host "Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
        exit 1
    }
    
    $dotnetVersion = (dotnet --version)
    Write-Host "✅ .NET SDK found: $dotnetVersion" -ForegroundColor Green
    
    # Clean previous build
    Write-Host "🧹 Cleaning previous build..." -ForegroundColor Yellow
    if (Test-Path $OutputDir) {
        Remove-Item $OutputDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    
    # Restore packages
    Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
    Set-Location $ProjectName
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Failed to restore packages!" -ForegroundColor Red
        exit 1
    }
    
    # Build project
    Write-Host "🔨 Building $Configuration configuration..." -ForegroundColor Yellow
    dotnet build -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed!" -ForegroundColor Red
        exit 1
    }
    
    # Publish as self-contained executable
    Write-Host "📱 Publishing optimized executable..." -ForegroundColor Yellow
    dotnet publish -c $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "..\$OutputDir" --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Publish failed!" -ForegroundColor Red
        exit 1
    }
    
    Set-Location ..
    Write-Host "✅ Build completed successfully!" -ForegroundColor Green
    
    # Show build output
    $buildFiles = Get-ChildItem $OutputDir
    Write-Host "📄 Build output:" -ForegroundColor Cyan
    foreach ($file in $buildFiles) {
        $size = if ($file.PSIsContainer) { "Folder" } else { "{0:N0} KB" -f [math]::Round($file.Length / 1KB, 0) }
        Write-Host "   $($file.Name) - $size" -ForegroundColor White
    }
    Write-Host ""
}

if ($OnlyBuild) {
    Write-Host "🎯 Build-only mode completed." -ForegroundColor Green
    exit 0
}

# Create installer
Write-Host "📦 Creating professional installer..." -ForegroundColor Yellow

# Check for NSIS
$makensis = Install-NSIS
if (-not $makensis -or -not (Test-Path $makensis)) {
    Write-Host "❌ NSIS not available. Cannot create installer." -ForegroundColor Red
    Write-Host "The application was built successfully in the '$OutputDir' folder." -ForegroundColor Yellow
    Write-Host "You can run WindowsCleaner.exe directly from there." -ForegroundColor Yellow
    exit 1
}

# Create installer graphics if they don't exist
if (-not (Test-Path "installer_header.bmp")) {
    Write-Host "🎨 Creating installer graphics..." -ForegroundColor Yellow
    # Create a simple bitmap header (150x57 pixels)
    Add-Type -AssemblyName System.Drawing
    $headerBitmap = New-Object System.Drawing.Bitmap(150, 57)
    $graphics = [System.Drawing.Graphics]::FromImage($headerBitmap)
    $graphics.Clear([System.Drawing.Color]::FromArgb(0, 120, 215))
    $font = New-Object System.Drawing.Font("Arial", 12, [System.Drawing.FontStyle]::Bold)
    $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    $graphics.DrawString("Cleaner Pro", $font, $brush, 10, 20)
    $headerBitmap.Save("installer_header.bmp", [System.Drawing.Imaging.ImageFormat]::Bmp)
    $graphics.Dispose()
    $headerBitmap.Dispose()
    $font.Dispose()
    $brush.Dispose()
}

if (-not (Test-Path "installer_side.bmp")) {
    # Create a simple side bitmap (164x314 pixels)
    $sideBitmap = New-Object System.Drawing.Bitmap(164, 314)
    $graphics = [System.Drawing.Graphics]::FromImage($sideBitmap)
    $brush1 = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        [System.Drawing.Point]::new(0, 0),
        [System.Drawing.Point]::new(164, 314),
        [System.Drawing.Color]::FromArgb(0, 120, 215),
        [System.Drawing.Color]::FromArgb(0, 90, 158)
    )
    $graphics.FillRectangle($brush1, 0, 0, 164, 314)
    $sideBitmap.Save("installer_side.bmp", [System.Drawing.Imaging.ImageFormat]::Bmp)
    $graphics.Dispose()
    $sideBitmap.Dispose()
    $brush1.Dispose()
}

# Compile installer
Write-Host "🔧 Compiling installer with NSIS..." -ForegroundColor Yellow
$nsisArgs = @("/V4", $InstallerScript)
$process = Start-Process -FilePath $makensis -ArgumentList $nsisArgs -Wait -PassThru -NoNewWindow

if ($process.ExitCode -eq 0 -and (Test-Path $InstallerOutput)) {
    $installerSize = Get-Item $InstallerOutput | ForEach-Object { "{0:N1} MB" -f ($_.Length / 1MB) }
    Write-Host "✅ Installer created successfully!" -ForegroundColor Green
    Write-Host "📦 Installer: $InstallerOutput ($installerSize)" -ForegroundColor Cyan
    Write-Host ""
    
    # Test installer signature (if possible)
    try {
        $signature = Get-AuthenticodeSignature $InstallerOutput
        if ($signature.Status -eq "NotSigned") {
            Write-Host "ℹ️ Note: Installer is not digitally signed." -ForegroundColor Yellow
            Write-Host "   For distribution, consider code signing for security." -ForegroundColor Yellow
        }
    } catch {
        # Ignore signature check errors
    }
    
    Write-Host "🎯 What's included in the installer:" -ForegroundColor Cyan
    Write-Host "   ✅ Professional Windows installer (MSI-style)" -ForegroundColor White
    Write-Host "   ✅ Automatic dependency detection and installation" -ForegroundColor White
    Write-Host "   ✅ Desktop and Start Menu shortcuts" -ForegroundColor White
    Write-Host "   ✅ Programs and Features registration" -ForegroundColor White
    Write-Host "   ✅ Professional uninstaller" -ForegroundColor White
    Write-Host "   ✅ Windows 10/11 compatibility checks" -ForegroundColor White
    Write-Host "   ✅ Admin privilege handling" -ForegroundColor White
    Write-Host ""
    
    Write-Host "🚀 Ready for distribution!" -ForegroundColor Green
    Write-Host "   You can now distribute '$InstallerOutput' to users." -ForegroundColor White
    Write-Host "   Users can simply run the installer to install your application." -ForegroundColor White
    
} else {
    Write-Host "❌ Failed to create installer!" -ForegroundColor Red
    if (Test-Path "installer.nsi") {
        Write-Host "Check the NSIS script for errors." -ForegroundColor Yellow
    }
    exit 1
}

Write-Host ""
Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "  Build and Installer Creation Complete!" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan
