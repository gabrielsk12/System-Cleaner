# Windows System Cleaner Pro 2.0.0 - Simple Build Script
param(
    [switch]$SkipBuild,
    [switch]$SkipInstaller,
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "🚀 Windows System Cleaner Pro 2.0.0 Build Script" -ForegroundColor Cyan

# Set up paths
$ProjectRoot = $PSScriptRoot
$ProjectPath = Join-Path $ProjectRoot "WindowsCleanerNew"
$PublishPath = Join-Path $ProjectPath "bin\$Configuration\net8.0-windows\publish"
$ReleasePath = Join-Path $ProjectRoot "release"

# Create release directory
if (Test-Path $ReleasePath) {
    Remove-Item $ReleasePath -Recurse -Force
}
New-Item -ItemType Directory -Path $ReleasePath -Force | Out-Null

if (-not $SkipBuild) {
    Write-Host "🔨 Building application..." -ForegroundColor Yellow
    
    Push-Location $ProjectPath
    
    # Clean previous builds
    Write-Host "   Cleaning..."
    & dotnet clean --configuration $Configuration
    
    # Restore packages
    Write-Host "   Restoring packages..."
    & dotnet restore
    
    # Build
    Write-Host "   Building..."
    & dotnet build --configuration $Configuration --no-restore
    
    # Publish
    Write-Host "   Publishing..."
    & dotnet publish --configuration $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=true --no-restore
    
    Pop-Location
    
    Write-Host "   ✅ Build completed!" -ForegroundColor Green
}

# Verify build output
$executablePath = Join-Path $PublishPath "WindowsCleanerNew.exe"
if (-not (Test-Path $executablePath)) {
    Write-Error "Build output not found. Please build first."
    exit 1
}

# Copy files to release folder
Write-Host "📦 Creating release package..." -ForegroundColor Yellow
Copy-Item $executablePath $ReleasePath -Force

$readmePath = Join-Path $ProjectRoot "README.md"
if (Test-Path $readmePath) {
    Copy-Item $readmePath (Join-Path $ReleasePath "README.txt") -Force
}

$licensePath = Join-Path $ProjectRoot "LICENSE"
if (Test-Path $licensePath) {
    Copy-Item $licensePath (Join-Path $ReleasePath "LICENSE.txt") -Force
}

# Create ZIP package
$zipPath = Join-Path $ProjectRoot "WindowsSystemCleanerPro-v2.0.0-Portable.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($ReleasePath, $zipPath, 'Optimal', $false)

Write-Host "✅ ZIP package created: $zipPath" -ForegroundColor Green

if (-not $SkipInstaller) {
    Write-Host "📦 Checking for Inno Setup..." -ForegroundColor Yellow
    
    # Check for Inno Setup
    $innoSetupPaths = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
    )
    
    $isccPath = $null
    foreach ($path in $innoSetupPaths) {
        if (Test-Path $path) {
            $isccPath = $path
            break
        }
    }
    
    if ($isccPath) {
        Write-Host "   Found Inno Setup, creating installer..." -ForegroundColor Green
        Push-Location $ProjectPath
        & $isccPath "setup.iss"
        Pop-Location
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ Installer created successfully!" -ForegroundColor Green
        } else {
            Write-Warning "Installer creation failed"
        }
    } else {
        Write-Host "   ⚠️  Inno Setup not found" -ForegroundColor Yellow
        Write-Host "   Download from: https://jrsoftware.org/isdl.php" -ForegroundColor Cyan
    }
}

Write-Host ""
Write-Host "🎉 Build completed successfully!" -ForegroundColor Green
Write-Host "📦 Release files ready in: $ReleasePath" -ForegroundColor Cyan
