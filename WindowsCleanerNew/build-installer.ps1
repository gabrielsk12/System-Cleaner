# Windows System Cleaner Pro - Build and Package Script
param(
    [switch]$Release = $false,
    [switch]$CreateInstaller = $false,
    [switch]$SkipBuild = $false
)

$ErrorActionPreference = "Stop"

Write-Host "=== Windows System Cleaner Pro Build Script ===" -ForegroundColor Cyan

# Configuration
$ProjectPath = "WindowsCleanerNew.csproj"
$Configuration = if ($Release) { "Release" } else { "Debug" }
$OutputPath = "bin\$Configuration\net8.0-windows"
$PublishPath = "bin\$Configuration\net8.0-windows\publish"
$DistPath = "..\dist"

# Create dist directory
if (!(Test-Path $DistPath)) {
    New-Item -ItemType Directory -Path $DistPath -Force | Out-Null
    Write-Host "Created dist directory: $DistPath" -ForegroundColor Green
}

if (!$SkipBuild) {
    Write-Host "`n=== Building Application ===" -ForegroundColor Yellow
    
    # Clean previous builds
    Write-Host "Cleaning previous builds..." -ForegroundColor Gray
    if (Test-Path "bin") {
        Remove-Item "bin" -Recurse -Force
    }
    if (Test-Path "obj") {
        Remove-Item "obj" -Recurse -Force
    }
    
    # Restore packages
    Write-Host "Restoring NuGet packages..." -ForegroundColor Gray
    dotnet restore $ProjectPath
    if ($LASTEXITCODE -ne 0) {
        throw "Package restore failed"
    }
    
    # Build application
    Write-Host "Building application in $Configuration mode..." -ForegroundColor Gray
    dotnet build $ProjectPath --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed"
    }
    
    # Publish self-contained
    Write-Host "Publishing self-contained application..." -ForegroundColor Gray
    dotnet publish $ProjectPath --configuration $Configuration --runtime win-x64 --self-contained true --output $PublishPath -p:PublishSingleFile=false
    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed"
    }
    
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "Output location: $PublishPath" -ForegroundColor Gray
}

if ($CreateInstaller) {
    Write-Host "`n=== Creating Installer ===" -ForegroundColor Yellow
    
    # Check if Inno Setup is installed
    $InnoCompilerPaths = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 5\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 5\ISCC.exe"
    )
    
    $InnoCompiler = $null
    foreach ($path in $InnoCompilerPaths) {
        if (Test-Path $path) {
            $InnoCompiler = $path
            break
        }
    }
    
    if (-not $InnoCompiler) {
        Write-Host "Inno Setup compiler not found. Please install Inno Setup from: https://jrsoftware.org/isdl.php" -ForegroundColor Red
        Write-Host "Alternatively, you can create a ZIP package instead." -ForegroundColor Yellow
        
        # Create ZIP package as fallback
        Write-Host "`n=== Creating ZIP Package ===" -ForegroundColor Yellow
        $ZipPath = "$DistPath\WindowsSystemCleanerPro_v1.0.1_Portable.zip"
        
        if (Test-Path $ZipPath) {
            Remove-Item $ZipPath -Force
        }
        
        # Copy files to a temp directory for zipping
        $TempDir = "$env:TEMP\WindowsCleanerPro_Package"
        if (Test-Path $TempDir) {
            Remove-Item $TempDir -Recurse -Force
        }
        New-Item -ItemType Directory -Path $TempDir -Force | Out-Null
        
        # Copy application files
        Copy-Item "$PublishPath\*" $TempDir -Recurse -Force
        Copy-Item "..\README.md" "$TempDir\README.txt" -Force
        Copy-Item "..\LICENSE" "$TempDir\LICENSE.txt" -Force
        
        # Create ZIP
        Compress-Archive -Path "$TempDir\*" -DestinationPath $ZipPath -CompressionLevel Optimal
        
        # Cleanup temp directory
        Remove-Item $TempDir -Recurse -Force
        
        Write-Host "ZIP package created: $ZipPath" -ForegroundColor Green
    } else {
        Write-Host "Found Inno Setup compiler: $InnoCompiler" -ForegroundColor Gray
        
        # Ensure the published build exists
        if (!(Test-Path "$PublishPath\WindowsCleanerNew.exe")) {
            throw "Published executable not found. Run with -SkipBuild:$false first."
        }
        
        # Copy published files to Release directory for Inno Setup
        $ReleaseDir = "bin\Release\net8.0-windows"
        if (!(Test-Path $ReleaseDir)) {
            New-Item -ItemType Directory -Path $ReleaseDir -Force | Out-Null
        }
        
        Copy-Item "$PublishPath\*" $ReleaseDir -Recurse -Force
        
        # Run Inno Setup compiler
        Write-Host "Compiling installer with Inno Setup..." -ForegroundColor Gray
        & $InnoCompiler "setup.iss"
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Installer created successfully!" -ForegroundColor Green
            $InstallerFile = Get-ChildItem "$DistPath\WindowsSystemCleanerPro_Setup_*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
            if ($InstallerFile) {
                Write-Host "Installer location: $($InstallerFile.FullName)" -ForegroundColor Gray
                Write-Host "Installer size: $([math]::Round($InstallerFile.Length / 1MB, 2)) MB" -ForegroundColor Gray
            }
        } else {
            throw "Installer creation failed"
        }
    }
}

Write-Host "`n=== Build Summary ===" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration" -ForegroundColor Gray
Write-Host "Target Framework: .NET 8.0 Windows" -ForegroundColor Gray
Write-Host "Platform: Windows x64" -ForegroundColor Gray

if (!$SkipBuild) {
    $ExePath = "$PublishPath\WindowsCleanerNew.exe"
    if (Test-Path $ExePath) {
        $ExeSize = [math]::Round((Get-Item $ExePath).Length / 1MB, 2)
        Write-Host "Executable size: $ExeSize MB" -ForegroundColor Gray
    }
}

$DistFiles = Get-ChildItem $DistPath -ErrorAction SilentlyContinue
if ($DistFiles) {
    Write-Host "`nDistribution files:" -ForegroundColor Gray
    foreach ($file in $DistFiles) {
        $size = [math]::Round($file.Length / 1MB, 2)
        Write-Host "  $($file.Name) ($size MB)" -ForegroundColor Gray
    }
}

Write-Host "`nBuild completed successfully! 🎉" -ForegroundColor Green
