$ErrorActionPreference = 'Stop'

# Configuration
$projectPath = "WindowsCleaner\WindowsCleaner.csproj"
$configuration = "Release"
$platform = "x64"
$outputPath = "publish"
$setupOutputPath = "installer"
$version = "1.0.0"

Write-Host "Building Windows System Cleaner Pro..." -ForegroundColor Green

# Ensure clean state
if (Test-Path $outputPath) {
    Remove-Item -Path $outputPath -Recurse -Force
}
if (Test-Path $setupOutputPath) {
    Remove-Item -Path $setupOutputPath -Recurse -Force
}

# Restore dependencies
Write-Host "Restoring dependencies..." -ForegroundColor Cyan
dotnet restore $projectPath

# Build solution
Write-Host "Building solution..." -ForegroundColor Cyan
dotnet build $projectPath -c $configuration -p:Platform=$platform

# Publish application
Write-Host "Publishing application..." -ForegroundColor Cyan
dotnet publish $projectPath -c $configuration -p:Platform=$platform --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -r win-x64 -o $outputPath

# Create installer directory
New-Item -ItemType Directory -Path $setupOutputPath -Force | Out-Null

# Create Inno Setup script
$innoScript = @"
[Setup]
AppName=Windows System Cleaner Pro
AppVersion=$version
AppPublisher=Gabriel SK
AppPublisherURL=https://github.com/gabrielsk12/System-Cleaner
DefaultDirName={pf}\Windows System Cleaner Pro
DefaultGroupName=Windows System Cleaner Pro
OutputDir=installer
OutputBaseFilename=WindowsSystemCleanerPro_Setup
SetupIconFile=WindowsCleaner\Resources\win.ico
UninstallDisplayIcon={app}\WindowsCleaner.exe
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Windows System Cleaner Pro"; Filename: "{app}\WindowsCleaner.exe"
Name: "{commondesktop}\Windows System Cleaner Pro"; Filename: "{app}\WindowsCleaner.exe"

[Run]
Filename: "{app}\WindowsCleaner.exe"; Description: "Launch Windows System Cleaner Pro"; Flags: postinstall nowait
"@

$innoScript | Out-File -FilePath "setup.iss" -Encoding UTF8

# Check if Inno Setup is installed
$innoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if (Test-Path $innoSetupPath) {
    Write-Host "Creating installer..." -ForegroundColor Cyan
    & $innoSetupPath "setup.iss"
} else {
    Write-Host "Inno Setup not found. Please install Inno Setup 6 to create the installer." -ForegroundColor Yellow
    Write-Host "Download from: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
}

Write-Host "Build completed!" -ForegroundColor Green
