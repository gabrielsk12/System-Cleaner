Write-Host "Creating Windows System Cleaner Pro Installer..." -ForegroundColor Cyan

# Check if WiX toolset is installed
if (-not (Get-Command wix -ErrorAction SilentlyContinue)) {
    Write-Host "WiX toolset is not installed. Installing it now..." -ForegroundColor Yellow
    dotnet tool install -g wix
}

# Set working directory
$scriptPath = if ($PSScriptRoot) { $PSScriptRoot } else { Get-Location }
Set-Location -Path $scriptPath

# First build the project
Write-Host "Building the project..." -ForegroundColor Yellow
./build-clean.ps1

# Variables
$appName = "WindowsSystemCleanerPro"
$outputDir = "$scriptPath\Installer"
$sourceDir = "$scriptPath\WindowsCleaner\bin\Release\net8.0-windows\win-x64\publish"
$iconPath = "$scriptPath\WindowsCleaner\Resources\Icons\app.ico"
$version = "1.0.0"

# Create installer directory if it doesn't exist
if (-not (Test-Path -Path $outputDir)) {
    New-Item -Path $outputDir -ItemType Directory | Out-Null
}

# Generate the WiX project
Write-Host "Generating WiX project..." -ForegroundColor Yellow
wix init -o "$outputDir\installer.wxs" -n "$appName" --icon "$iconPath" --manufacturer "System Optimizer" --product-name "Windows Cleaner Pro" --product-version $version --upgrade-guid "559F3358-D571-4047-B9F2-BB9EE62C9133"

# Build the installer
Write-Host "Building the installer..." -ForegroundColor Yellow
wix build "$outputDir\installer.wxs" -ext WixToolset.UI.wixext -out "$outputDir\WindowsCleanerProSetup.msi" -binpath "$sourceDir"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Installer created successfully at: $outputDir\WindowsCleanerProSetup.msi" -ForegroundColor Green
    
    # Open the installer directory
    $response = Read-Host "Do you want to open the installer directory? (y/n)"
    if ($response -eq "y") {
        Invoke-Item $outputDir
    }
} else {
    Write-Host "Failed to create installer." -ForegroundColor Red
}

Write-Host "Setup script completed." -ForegroundColor Cyan
