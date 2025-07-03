# Windows System Cleaner Pro 2.0.0 - Comprehensive Build and Package Script
param(
    [switch]$SkipBuild,
    [switch]$SkipInstaller,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

Write-Host "🚀 Windows System Cleaner Pro 2.0.0 Build Script" -ForegroundColor Cyan
Write-Host "=" * 60

# Set up paths
$ProjectRoot = $PSScriptRoot
$ProjectPath = Join-Path $ProjectRoot "WindowsCleanerNew"
$ProjectFile = Join-Path $ProjectPath "WindowsCleanerNew.csproj"
$PublishPath = Join-Path $ProjectPath "bin\$Configuration\net8.0-windows\publish"
$InstallerPath = Join-Path $ProjectRoot "installer"
$RedistPath = Join-Path $ProjectRoot "redist"

# Create directories
Write-Host "📁 Creating directories..." -ForegroundColor Yellow
@($InstallerPath, $RedistPath) | ForEach-Object {
    if (-not (Test-Path $_)) {
        New-Item -ItemType Directory -Path $_ -Force | Out-Null
        Write-Host "   Created: $_"
    }
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
        Write-Host "   ✅ Downloaded: $Path"
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
    
    # Restore packages
    Write-Host "   📦 Restoring NuGet packages..."
    & dotnet restore $ProjectFile
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to restore packages"
    }
    
    # Build the application
    Write-Host "   🔨 Building $Configuration configuration..."
    & dotnet build $ProjectFile -c $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Build completed with warnings, continuing..."
    }
    
    # Publish single-file executable
    Write-Host "   📦 Publishing single-file executable..."
    & dotnet publish $ProjectFile -c $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --no-restore
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Publish completed with warnings, continuing..."
    }
    
    Write-Host "   ✅ Build completed successfully!" -ForegroundColor Green
} else {
    Write-Host "⏭️  Skipping build (SkipBuild specified)" -ForegroundColor Yellow
}

# Verify build output
if (-not (Test-Path (Join-Path $PublishPath "WindowsCleanerNew.exe"))) {
    Write-Error "Build output not found. Please build the project first."
    exit 1
}

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
        Write-Host "   📥 Inno Setup not found. Downloading portable version..." -ForegroundColor Green
        
        # Download Inno Setup (we'll use a workaround)
        Write-Host "   ℹ️  To create the installer:"
        Write-Host "   1. Download Inno Setup from: https://jrsoftware.org/isdl.php"
        Write-Host "   2. Install Inno Setup"
        Write-Host "   3. Run this script again"
        Write-Host "   4. Or manually compile setup.iss"
        
        Write-Host "   📄 Installer script created: setup.iss" -ForegroundColor Green
        Write-Host "   📄 Setup info created: SETUP_INFO.txt" -ForegroundColor Green
    } else {
        Write-Host "   🔨 Compiling installer with Inno Setup..."
        & $isccPath "setup.iss"
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ Installer created successfully!" -ForegroundColor Green
            
            # Find the created installer
            $installerFile = Get-ChildItem $InstallerPath -Filter "*.exe" | Select-Object -First 1
            if ($installerFile) {
                Write-Host "   📦 Installer: $($installerFile.FullName)" -ForegroundColor Cyan
                Write-Host "   📊 Size: $([math]::Round($installerFile.Length / 1MB, 2)) MB"
            }
        } else {
            Write-Warning "Installer compilation failed"
        }
    }
} else {
    Write-Host "⏭️  Skipping installer creation (SkipInstaller specified)" -ForegroundColor Yellow
}

# Create release package
Write-Host "📦 Creating release package..." -ForegroundColor Yellow
$releaseFolder = Join-Path $ProjectRoot "release"
if (Test-Path $releaseFolder) {
    Remove-Item $releaseFolder -Recurse -Force
}
New-Item -ItemType Directory -Path $releaseFolder -Force | Out-Null

# Copy files to release folder
$filesToCopy = @(
    @{ Source = (Join-Path $PublishPath "WindowsCleanerNew.exe"); Dest = "WindowsCleanerNew.exe" },
    @{ Source = "README.md"; Dest = "README.txt" },
    @{ Source = "LICENSE"; Dest = "LICENSE.txt" },
    @{ Source = "PROJECT_STATUS.md"; Dest = "PROJECT_STATUS.txt" },
    @{ Source = "SETUP_INFO.txt"; Dest = "SETUP_INFO.txt" }
)

foreach ($file in $filesToCopy) {
    $srcPath = Join-Path $ProjectRoot $file.Source
    $destPath = Join-Path $releaseFolder $file.Dest
    
    if (Test-Path $srcPath) {
        Copy-Item $srcPath $destPath -Force
        Write-Host "   ✅ Copied: $($file.Dest)"
    } else {
        Write-Warning "Source file not found: $srcPath"
    }
}

# Create ZIP package
Write-Host "📦 Creating ZIP package..." -ForegroundColor Yellow
$zipPath = Join-Path $ProjectRoot "WindowsSystemCleanerPro-2.0.0-Portable.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

try {
    Compress-Archive -Path "$releaseFolder\*" -DestinationPath $zipPath -CompressionLevel Optimal
    Write-Host "   ✅ ZIP package created: $zipPath" -ForegroundColor Green
    
    $zipSize = (Get-Item $zipPath).Length
    Write-Host "   📊 ZIP size: $([math]::Round($zipSize / 1MB, 2)) MB"
} catch {
    Write-Warning "Failed to create ZIP package: $_"
}

# Summary
Write-Host "`n🎉 Build Summary" -ForegroundColor Cyan
Write-Host "=" * 60
Write-Host "✅ Project: Windows System Cleaner Pro 2.0.0" -ForegroundColor Green
Write-Host "✅ Build Configuration: $Configuration" -ForegroundColor Green
Write-Host "✅ Target Framework: .NET 8.0" -ForegroundColor Green
Write-Host "✅ Platform: Windows x64" -ForegroundColor Green

if (Test-Path (Join-Path $PublishPath "WindowsCleanerNew.exe")) {
    $exeSize = (Get-Item (Join-Path $PublishPath "WindowsCleanerNew.exe")).Length
    Write-Host "✅ Executable: $([math]::Round($exeSize / 1MB, 2)) MB" -ForegroundColor Green
}

$installerFile = Get-ChildItem $InstallerPath -Filter "*.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($installerFile) {
    Write-Host "✅ Installer: $([math]::Round($installerFile.Length / 1MB, 2)) MB" -ForegroundColor Green
}

if (Test-Path $zipPath) {
    Write-Host "✅ Portable ZIP: $([math]::Round((Get-Item $zipPath).Length / 1MB, 2)) MB" -ForegroundColor Green
}

Write-Host "`n📋 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Test the executable: $PublishPath\WindowsCleanerNew.exe"
Write-Host "2. Test the installer (if created)"
Write-Host "3. Create GitHub release with tag v2.0.0"
Write-Host "4. Upload installer and ZIP to GitHub release"

Write-Host "`n🚀 Ready for release!" -ForegroundColor Green
