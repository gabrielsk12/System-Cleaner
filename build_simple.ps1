param([string]$Configuration = "Release")

$ErrorActionPreference = "Stop"
$ProjectName = "WindowsCleaner"
$SolutionFile = "WindowsCleaner.sln"
$OutputDir = "Build"
$InstallerOutput = "WindowsCleanerPro_Setup.exe"

Write-Host "Building Windows System Cleaner Pro..."

# Check for .NET SDK
$dotnetVersion = dotnet --version 2>$null
if (-not $dotnetVersion) {
    Write-Host "ERROR: .NET SDK not found!"
    exit 1
}
Write-Host "Found .NET SDK: $dotnetVersion"

# Check solution file
if (-not (Test-Path $SolutionFile)) {
    Write-Host "ERROR: Solution file not found: $SolutionFile"
    exit 1
}

# Clean previous builds
if (Test-Path $OutputDir) {
    Remove-Item -Recurse -Force $OutputDir
}

# Build
Write-Host "Restoring packages..."
dotnet restore $SolutionFile
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "Building solution..."
dotnet build $SolutionFile --configuration $Configuration --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "Publishing application..."
$publishPath = Join-Path $OutputDir "publish"
dotnet publish "$ProjectName/$ProjectName.csproj" --configuration $Configuration --output $publishPath --self-contained false
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "Build completed successfully!"

# Create installer if NSIS available
$nsisPath = "${env:ProgramFiles(x86)}\NSIS\makensis.exe"
if (Test-Path $nsisPath) {
    Write-Host "Creating installer..."
    
    # Create basic installer graphics
    if (-not (Test-Path "installer_header.bmp")) {
        Add-Type -AssemblyName System.Drawing
        $bmp = New-Object System.Drawing.Bitmap(150, 57)
        $g = [System.Drawing.Graphics]::FromImage($bmp)
        $g.Clear([System.Drawing.Color]::Blue)
        $bmp.Save("installer_header.bmp")
        $g.Dispose()
        $bmp.Dispose()
    }
    
    if (-not (Test-Path "installer_side.bmp")) {
        Add-Type -AssemblyName System.Drawing
        $bmp = New-Object System.Drawing.Bitmap(164, 314)
        $g = [System.Drawing.Graphics]::FromImage($bmp)
        $g.Clear([System.Drawing.Color]::Blue)
        $bmp.Save("installer_side.bmp")
        $g.Dispose()
        $bmp.Dispose()
    }
    
    # Compile installer
    $proc = Start-Process -FilePath $nsisPath -ArgumentList "installer.nsi" -Wait -PassThru -NoNewWindow
    
    if ($proc.ExitCode -eq 0 -and (Test-Path $InstallerOutput)) {
        Write-Host "Installer created: $InstallerOutput"
    }
}

Write-Host "Done!"
