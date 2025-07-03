# Windows System Cleaner Pro 2.0.0 - Build Script
param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

Write-Host "Building Windows System Cleaner Pro 2.0.0" -ForegroundColor Cyan
Write-Host "================================================"

# Set up paths
$ProjectRoot = $PSScriptRoot
$ProjectPath = Join-Path $ProjectRoot "WindowsCleanerNew"
$ProjectFile = Join-Path $ProjectPath "WindowsCleanerNew.csproj"
$PublishPath = Join-Path $ProjectPath "bin\$Configuration\net8.0-windows\publish"

Write-Host "Building application..." -ForegroundColor Yellow

# Clean previous builds
if (Test-Path $PublishPath) {
    Remove-Item $PublishPath -Recurse -Force
}

# Restore packages
Write-Host "Restoring NuGet packages..."
& dotnet restore $ProjectFile

# Build the application
Write-Host "Building $Configuration configuration..."
& dotnet build $ProjectFile -c $Configuration --no-restore

# Publish single-file executable
Write-Host "Publishing single-file executable..."
& dotnet publish $ProjectFile -c $Configuration -r win-x64 --self-contained true -p:PublishSingleFile=true --no-restore

# Create release folder
$releaseFolder = Join-Path $ProjectRoot "release"
if (Test-Path $releaseFolder) {
    Remove-Item $releaseFolder -Recurse -Force
}
New-Item -ItemType Directory -Path $releaseFolder -Force | Out-Null

# Copy files to release folder
Write-Host "Creating release package..."
Copy-Item (Join-Path $PublishPath "WindowsCleanerNew.exe") (Join-Path $releaseFolder "WindowsCleanerNew.exe") -Force
Copy-Item "README.md" (Join-Path $releaseFolder "README.txt") -Force
Copy-Item "LICENSE" (Join-Path $releaseFolder "LICENSE.txt") -Force
Copy-Item "PROJECT_STATUS.md" (Join-Path $releaseFolder "PROJECT_STATUS.txt") -Force

# Create ZIP package
$zipPath = Join-Path $ProjectRoot "WindowsSystemCleanerPro-2.0.0-Portable.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive -Path "$releaseFolder\*" -DestinationPath $zipPath -CompressionLevel Optimal

Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "Executable: $PublishPath\WindowsCleanerNew.exe"
Write-Host "ZIP package: $zipPath"
