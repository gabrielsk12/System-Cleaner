# Windows System Cleaner Pro - Professional Build and Installer Creation Script
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

function Test-Command($cmdname) {
    return [bool](Get-Command -Name $cmdname -ErrorAction SilentlyContinue)
}

function Install-NSIS {
    $nsisPath = "${env:ProgramFiles(x86)}\NSIS\makensis.exe"
    if (Test-Path $nsisPath) {
        Write-Host "✅ NSIS found at: $nsisPath" -ForegroundColor Green
        return $nsisPath
    }
    
    Write-Host "🔍 NSIS not found. Attempting to install..." -ForegroundColor Yellow
    
    if (Test-Command "winget") {
        Write-Host "📦 Installing NSIS via winget..." -ForegroundColor Yellow
        try {
            winget install NSIS.NSIS --silent --accept-source-agreements --accept-package-agreements
            Start-Sleep 10
            if (Test-Path $nsisPath) {
                Write-Host "✅ NSIS installed successfully!" -ForegroundColor Green
                return $nsisPath
            }
        } catch {
            Write-Host "⚠️ winget installation failed." -ForegroundColor Yellow
        }
    }
    
    Write-Host "❌ Please install NSIS manually from: https://nsis.sourceforge.io/" -ForegroundColor Red
    return $null
}

# Check administrator privileges
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Host "⚠️ Administrator privileges recommended for best results." -ForegroundColor Yellow
}

if (-not $SkipBuild) {
    Write-Host "🔍 Step 1: Environment Verification" -ForegroundColor Yellow
    
    # Check for .NET SDK
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
    
    if (-not (Test-Path $SolutionFile)) {
        Write-Host "❌ Solution file not found: $SolutionFile" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ Solution file found: $SolutionFile" -ForegroundColor Green
    
    Write-Host "🏗️ Step 2: Building Application" -ForegroundColor Yellow
    
    # Clean previous builds
    if (Test-Path $OutputDir) {
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
    Write-Host "⚙️ Building solution..." -ForegroundColor White
    dotnet build $SolutionFile --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Build failed!" -ForegroundColor Red
        exit 1
    }
    
    # Publish application
    Write-Host "📦 Publishing application..." -ForegroundColor White
    $publishPath = Join-Path $OutputDir "publish"
    dotnet publish "$ProjectName/$ProjectName.csproj" --configuration $Configuration --output $publishPath --self-contained false
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Publish failed!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "✅ Build completed successfully!" -ForegroundColor Green
    
    if ($OnlyBuild) {
        Write-Host "✅ Build-only mode complete!" -ForegroundColor Green
        exit 0
    }
}

Write-Host "📦 Step 3: Creating Professional Installer" -ForegroundColor Yellow

# Check for NSIS
$nsisPath = Install-NSIS
if (-not $nsisPath) {
    Write-Host "❌ NSIS installation failed! Cannot create installer." -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $InstallerScript)) {
    Write-Host "❌ Installer script not found: $InstallerScript" -ForegroundColor Red
    exit 1
}

# Create installer graphics if needed
if (-not (Test-Path "installer_header.bmp")) {
    Write-Host "📸 Creating installer graphics..." -ForegroundColor White
    Add-Type -AssemblyName System.Drawing
    
    # Header bitmap (150x57)
    $headerBitmap = New-Object System.Drawing.Bitmap(150, 57)
    $graphics = [System.Drawing.Graphics]::FromImage($headerBitmap)
    $graphics.Clear([System.Drawing.Color]::FromArgb(0, 120, 215))
    $headerBitmap.Save("installer_header.bmp", [System.Drawing.Imaging.ImageFormat]::Bmp)
    $graphics.Dispose()
    $headerBitmap.Dispose()
    
    # Side bitmap (164x314)
    $sideBitmap = New-Object System.Drawing.Bitmap(164, 314)
    $graphics = [System.Drawing.Graphics]::FromImage($sideBitmap)
    $graphics.Clear([System.Drawing.Color]::FromArgb(0, 120, 215))
    $sideBitmap.Save("installer_side.bmp", [System.Drawing.Imaging.ImageFormat]::Bmp)
    $graphics.Dispose()
    $sideBitmap.Dispose()
}

# Compile installer
Write-Host "🔨 Compiling installer..." -ForegroundColor White
$process = Start-Process -FilePath $nsisPath -ArgumentList @("/V2", $InstallerScript) -Wait -PassThru -NoNewWindow

if ($process.ExitCode -eq 0 -and (Test-Path $InstallerOutput)) {
    $installerSize = [math]::Round((Get-Item $InstallerOutput).Length / 1MB, 2)
    Write-Host "✅ Installer created successfully!" -ForegroundColor Green
    Write-Host "📁 Installer file: $InstallerOutput ($installerSize MB)" -ForegroundColor White
    
    Write-Host "🎯 Installer features:" -ForegroundColor Cyan
    Write-Host "   ✅ Professional Windows installer" -ForegroundColor White
    Write-Host "   ✅ Automatic dependency installation" -ForegroundColor White
    Write-Host "   ✅ Desktop and Start Menu shortcuts" -ForegroundColor White
    Write-Host "   ✅ Programs and Features registration" -ForegroundColor White
    Write-Host "   ✅ Professional uninstaller" -ForegroundColor White
    Write-Host ""
    Write-Host "🚀 Ready for distribution!" -ForegroundColor Green
} else {
    Write-Host "❌ Failed to create installer!" -ForegroundColor Red
    exit 1
}

Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "  Build Complete!" -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan
