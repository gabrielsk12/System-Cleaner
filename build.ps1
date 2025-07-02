# Build script for Windows Cleaner

# Check if running as administrator
if (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator"))
{
    Write-Host "This script requires administrator privileges. Restarting as administrator..." -ForegroundColor Yellow
    Start-Process PowerShell -Verb RunAs "-NoProfile -ExecutionPolicy Bypass -Command `"cd '$pwd'; & '$PSCommandPath';`""
    exit
}

Write-Host "Building Windows System Cleaner..." -ForegroundColor Green

# Navigate to project directory
Set-Location "WindowsCleaner"

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

# Build in Release mode
Write-Host "Building in Release mode..." -ForegroundColor Yellow
dotnet build -c Release

# Publish as single executable
Write-Host "Publishing as single executable..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "..\Build"

Write-Host "Build completed! Executable available in 'Build' folder." -ForegroundColor Green
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
