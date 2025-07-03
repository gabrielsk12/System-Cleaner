Write-Host "Building Windows System Cleaner Pro as a single file executable..." -ForegroundColor Cyan

# Set working directory
$scriptPath = if ($PSScriptRoot) { $PSScriptRoot } else { Get-Location }
Set-Location -Path $scriptPath

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path -Path "WindowsCleaner\bin") {
    Remove-Item -Path "WindowsCleaner\bin" -Recurse -Force -ErrorAction SilentlyContinue
}
if (Test-Path -Path "WindowsCleaner\obj") {
    Remove-Item -Path "WindowsCleaner\obj" -Recurse -Force -ErrorAction SilentlyContinue
}

# Build single file executable with icon
Write-Host "Building single file executable..." -ForegroundColor Yellow

# Create executable
dotnet publish WindowsCleaner\WindowsCleaner.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -p:PublishReadyToRun=true `
    -p:EnableCompressionInSingleFile=true

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build completed successfully!" -ForegroundColor Green
    
    # Create output directory
    $outputDir = "$scriptPath\Release"
    if (-not (Test-Path -Path $outputDir)) {
        New-Item -Path $outputDir -ItemType Directory | Out-Null
    }
    
    # Copy executable and icon to output directory
    Copy-Item -Path "WindowsCleaner\bin\Release\net8.0-windows\win-x64\publish\WindowsCleaner.exe" -Destination "$outputDir\WindowsCleanerPro.exe" -Force
    Copy-Item -Path "win.png" -Destination "$outputDir\win.png" -Force
    
    # Create a shortcut with proper icon handling
    $WshShell = New-Object -comObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut("$outputDir\Windows System Cleaner Pro.lnk")
    $Shortcut.TargetPath = "$outputDir\WindowsCleanerPro.exe"
    # For the shortcut, use the exe itself as the icon source
    $Shortcut.IconLocation = "$outputDir\WindowsCleanerPro.exe,0"
    $Shortcut.WorkingDirectory = "$outputDir"
    $Shortcut.Description = "Windows System Cleaner Pro"
    $Shortcut.Save()
    
    # Show build output location
    Write-Host "Output files available at: $outputDir" -ForegroundColor Green
    Get-ChildItem -Path $outputDir | Select-Object Name, Length | Format-Table
    
    # Ask to open the output directory
    $response = Read-Host "Do you want to open the output directory? (y/n)"
    if ($response -eq "y") {
        Invoke-Item $outputDir
    }
} else {
    Write-Host "Build failed!" -ForegroundColor Red
}

Write-Host "Build script completed." -ForegroundColor Cyan
