# Windows System Cleaner Pro - Build Script
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$ProjectName = "WindowsCleaner"
$SolutionFile = "WindowsCleaner.sln"
$OutputDir = "Build"
$InstallerOutput = "WindowsCleanerPro_Setup.exe"

Write-Host "================================================================="
Write-Host "  Windows System Cleaner Pro - Build System"
Write-Host "================================================================="

# Check for .NET SDK
try {
    $dotnetVersion = dotnet --version 2>$null
    if ($dotnetVersion) {
        Write-Host "✓ .NET SDK found: $dotnetVersion"
    }
} catch {
    Write-Host "✗ .NET SDK not found!"
    Write-Host "Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download"
    exit 1
}

if (-not (Test-Path $SolutionFile)) {
    Write-Host "✗ Solution file not found: $SolutionFile"
    exit 1
}

Write-Host "✓ Solution file found: $SolutionFile"
Write-Host "Building Application..."

# Clean previous builds
if (Test-Path $OutputDir) {
    Remove-Item -Recurse -Force $OutputDir
}

# Restore packages
Write-Host "Restoring NuGet packages..."
dotnet restore $SolutionFile
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Package restore failed!"
    exit 1
}

# Build solution
Write-Host "Building solution..."
dotnet build $SolutionFile --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed!"
    exit 1
}

# Publish application
Write-Host "Publishing application..."
$publishPath = Join-Path $OutputDir "publish"
dotnet publish "$ProjectName/$ProjectName.csproj" --configuration $Configuration --output $publishPath --self-contained false
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Publish failed!"
    exit 1
}

Write-Host "✓ Build completed successfully!"
Write-Host "Output location: $publishPath"

# Check for NSIS
$nsisPath = "${env:ProgramFiles(x86)}\NSIS\makensis.exe"
if (-not (Test-Path $nsisPath)) {
    Write-Host "NSIS not found. Installing via winget..."
    try {
        winget install NSIS.NSIS --silent --accept-source-agreements --accept-package-agreements
        Start-Sleep 10
    } catch {
        Write-Host "Please install NSIS manually from: https://nsis.sourceforge.io/"
        exit 1
    }
}

if (Test-Path $nsisPath) {
    Write-Host "Creating installer..."
    
    # Create simple installer graphics
    if (-not (Test-Path "installer_header.bmp")) {
        Add-Type -AssemblyName System.Drawing
        $headerBitmap = New-Object System.Drawing.Bitmap(150, 57)
        $graphics = [System.Drawing.Graphics]::FromImage($headerBitmap)
        $graphics.Clear([System.Drawing.Color]::Blue)
        $headerBitmap.Save("installer_header.bmp")
        $graphics.Dispose()
        $headerBitmap.Dispose()
        
        $sideBitmap = New-Object System.Drawing.Bitmap(164, 314)
        $graphics = [System.Drawing.Graphics]::FromImage($sideBitmap)
        $graphics.Clear([System.Drawing.Color]::Blue)
        $sideBitmap.Save("installer_side.bmp")
        $graphics.Dispose()
        $sideBitmap.Dispose()
    }
    
    # Compile installer
    $process = Start-Process -FilePath $nsisPath -ArgumentList @("installer.nsi") -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0 -and (Test-Path $InstallerOutput)) {
        $size = [math]::Round((Get-Item $InstallerOutput).Length / 1MB, 2)
        Write-Host "✓ Installer created: $InstallerOutput ($size MB)"
        Write-Host "Ready for distribution!"
    } else {
        Write-Host "✗ Failed to create installer!"
        exit 1
    }
} else {
    Write-Host "NSIS not available. Skipping installer creation."
}

Write-Host "================================================================="
Write-Host "Build Complete!"
Write-Host "================================================================="
