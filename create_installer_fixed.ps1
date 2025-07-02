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
    Write-Host "🔍 Step 1: Environment Verification" -ForegroundColor Yellow
    Write-Host "----------------------------------------" -ForegroundColor Yellow
    
    # Check for .NET SDK
    $dotnetVersion = $null
    try {
        $dotnetVersion = dotnet --version 2>$null
        if ($dotnetVersion) {
            Write-Host "✅ .NET SDK found: $dotnetVersion" -ForegroundColor Green
        }
    } catch {
        Write-Host "❌ .NET SDK not found!" -ForegroundColor Red
        Write-Host "Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
        exit 1
    }
    
    # Verify project files
    if (-not (Test-Path $SolutionFile)) {
        Write-Host "❌ Solution file not found: $SolutionFile" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Solution file found: $SolutionFile" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "🏗️ Step 2: Building Application" -ForegroundColor Yellow
    Write-Host "----------------------------------------" -ForegroundColor Yellow
    
    # Clean previous builds
    if (Test-Path $OutputDir) {
        Write-Host "🧹 Cleaning previous build..." -ForegroundColor White
        Remove-Item -Recurse -Force $OutputDir
    }
    
    # Restore packages
    Write-Host "📦 Restoring NuGet packages..." -ForegroundColor White
    dotnet restore $SolutionFile
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Package restore failed!" -ForegroundColor Red
        exit 1
    }
    
    # Build solution
    Write-Host "⚙️ Building solution in $Configuration mode..." -ForegroundColor White
    dotnet build $SolutionFile --configuration $Configuration --no-restore --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed!" -ForegroundColor Red
        exit 1
    }
    
    # Publish application
    Write-Host "📦 Publishing application..." -ForegroundColor White
    $publishPath = Join-Path $OutputDir "publish"
    dotnet publish "$ProjectName/$ProjectName.csproj" --configuration $Configuration --output $publishPath --self-contained false --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Publish failed!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Build completed successfully!" -ForegroundColor Green
    Write-Host "📁 Output location: $publishPath" -ForegroundColor White
    
    # Create build info file
    $buildInfo = @{
        BuildDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
        Configuration = $Configuration
        DotNetVersion = $dotnetVersion
        ComputerName = $env:COMPUTERNAME
        UserName = $env:USERNAME
    }
    $buildInfo | ConvertTo-Json | Out-File (Join-Path $publishPath "build-info.json") -Encoding UTF8
    
    if ($OnlyBuild) {
        Write-Host ""
        Write-Host "✅ Build-only mode complete!" -ForegroundColor Green
        exit 0
    }
} else {
    Write-Host "⏩ Skipping build step (using existing build)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📦 Step 3: Creating Professional Installer" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Yellow

# Check for NSIS
$nsisPath = Install-NSIS
if (-not $nsisPath) {
    Write-Host "❌ NSIS installation failed! Cannot create installer." -ForegroundColor Red
    Write-Host "Please install NSIS manually from: https://nsis.sourceforge.io/" -ForegroundColor Yellow
    exit 1
}

# Verify installer script exists
if (-not (Test-Path $InstallerScript)) {
    Write-Host "❌ Installer script not found: $InstallerScript" -ForegroundColor Red
    exit 1
}

# Create installer graphics if they don't exist
$headerBmp = "installer_header.bmp"
$sideBmp = "installer_side.bmp"

if (-not (Test-Path $headerBmp)) {
    Write-Host "📸 Creating installer header image..." -ForegroundColor White
    # Create a simple header bitmap (150x57 pixels)
    Add-Type -AssemblyName System.Drawing
    $headerBitmap = New-Object System.Drawing.Bitmap(150, 57)
    $graphics = [System.Drawing.Graphics]::FromImage($headerBitmap)
    $graphics.Clear([System.Drawing.Color]::FromArgb(0, 120, 215))  # Windows blue
    $font = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)
    $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    $graphics.DrawString("System Cleaner", $font, $brush, 5, 20)
    $headerBitmap.Save($headerBmp, [System.Drawing.Imaging.ImageFormat]::Bmp)
    $graphics.Dispose()
    $headerBitmap.Dispose()
}

if (-not (Test-Path $sideBmp)) {
    Write-Host "📸 Creating installer side image..." -ForegroundColor White
    # Create a simple side bitmap (164x314 pixels)
    $sideBitmap = New-Object System.Drawing.Bitmap(164, 314)
    $graphics = [System.Drawing.Graphics]::FromImage($sideBitmap)
    $graphics.Clear([System.Drawing.Color]::FromArgb(0, 120, 215))  # Windows blue
    $font = New-Object System.Drawing.Font("Segoe UI", 12, [System.Drawing.FontStyle]::Bold)
    $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    $graphics.DrawString("Windows", $font, $brush, 10, 100)
    $graphics.DrawString("System", $font, $brush, 10, 130)
    $graphics.DrawString("Cleaner", $font, $brush, 10, 160)
    $graphics.DrawString("Pro", $font, $brush, 10, 190)
    $sideBitmap.Save($sideBmp, [System.Drawing.Imaging.ImageFormat]::Bmp)
    $graphics.Dispose()
    $sideBitmap.Dispose()
}

# Compile installer
Write-Host "🔨 Compiling installer with NSIS..." -ForegroundColor White
$nsisArgs = @("/V2", $InstallerScript)
$process = Start-Process -FilePath $nsisPath -ArgumentList $nsisArgs -Wait -PassThru -NoNewWindow

if ($process.ExitCode -eq 0 -and (Test-Path $InstallerOutput)) {
    $installerSize = [math]::Round((Get-Item $InstallerOutput).Length / 1MB, 2)
    Write-Host "✅ Installer created successfully!" -ForegroundColor Green
    Write-Host "📁 Installer file: $InstallerOutput" -ForegroundColor White
    Write-Host "📊 Installer size: $installerSize MB" -ForegroundColor White
    
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
