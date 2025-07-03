# Windows System Cleaner Pro 2.0.0 - Build and Package Script
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
$PublishPath = Join-Path $ProjectPath "bin\$Configuration\net8.0-windows\publish"
$ReleasePath = Join-Path $ProjectRoot "release"

# Create directories
Write-Host "📁 Creating directories..." -ForegroundColor Yellow
if (Test-Path $ReleasePath) {
    Remove-Item $ReleasePath -Recurse -Force
}
New-Item -ItemType Directory -Path $ReleasePath -Force | Out-Null

if (-not $SkipBuild) {
    Write-Host "🔨 Building application..." -ForegroundColor Yellow
    
    Push-Location $ProjectPath
    try {
        # Clean, restore, build, and publish
        & dotnet clean --configuration $Configuration
        & dotnet restore
        & dotnet build --configuration $Configuration --no-restore
        & dotnet publish --configuration $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=true --no-restore
        
        Write-Host "   ✅ Build completed!" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}
}

# Verify build output
$executablePath = Join-Path $PublishPath "WindowsCleanerNew.exe"
if (-not (Test-Path $executablePath)) {
    Write-Error "Build output not found. Please build first."
    exit 1
}

# Copy files to release folder
Copy-Item $executablePath $ReleasePath -Force
Copy-Item (Join-Path $ProjectRoot "README.md") (Join-Path $ReleasePath "README.txt") -Force -ErrorAction SilentlyContinue
Copy-Item (Join-Path $ProjectRoot "LICENSE") (Join-Path $ReleasePath "LICENSE.txt") -Force -ErrorAction SilentlyContinue

# Create ZIP package
$zipPath = Join-Path $ProjectRoot "WindowsSystemCleanerPro-v2.0.0-Portable.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($ReleasePath, $zipPath, 'Optimal', $false)

Write-Host "✅ ZIP package created: $zipPath" -ForegroundColor Green

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
    
    if ($isccPath) {
        Push-Location $ProjectPath
        try {
            & $isccPath "setup.iss"
            if ($LASTEXITCODE -eq 0) {
                Write-Host "   ✅ Installer created!" -ForegroundColor Green
            }
        }
        finally {
            Pop-Location
        }
    } else {
        Write-Host "   ⚠️  Inno Setup not found. Download from https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    }
}

Write-Host "🎉 Build completed successfully!" -ForegroundColor Green
Write-Host "📦 Release files ready in: $ReleasePath" -ForegroundColor Cyan
