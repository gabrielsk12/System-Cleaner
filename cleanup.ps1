Write-Host "Cleaning Windows System Cleaner Pro project..." -ForegroundColor Cyan

# Set working directory
$scriptPath = if ($PSScriptRoot) { $PSScriptRoot } else { Get-Location }
Set-Location -Path $scriptPath

# Define directories to clean
$binDir = "$scriptPath\WindowsCleaner\bin"
$objDir = "$scriptPath\WindowsCleaner\obj"
$installerDir = "$scriptPath\Installer"
$iconsDir = "$scriptPath\WindowsCleaner\Resources\Icons"
$releaseDir = "$scriptPath\Release"
$tempFiles = @(
    "*.user",
    "*.suo",
    "*.tmp",
    "*.temp",
    "*.log",
    "*.bak",
    "*.cache"
)

# Clean bin and obj directories
Write-Host "Cleaning build artifacts..." -ForegroundColor Yellow
if (Test-Path -Path $binDir) {
    Remove-Item -Path $binDir -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  Removed bin directory" -ForegroundColor Gray
}

if (Test-Path -Path $objDir) {
    Remove-Item -Path $objDir -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "  Removed obj directory" -ForegroundColor Gray
}

# Clean installer artifacts if requested
$cleanInstaller = Read-Host "Do you want to clean installer artifacts? (y/n)"
if ($cleanInstaller -eq "y") {
    if (Test-Path -Path $installerDir) {
        Remove-Item -Path $installerDir -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  Removed installer directory" -ForegroundColor Gray
    }
    
    if (Test-Path -Path $releaseDir) {
        Remove-Item -Path $releaseDir -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  Removed release directory" -ForegroundColor Gray
    }
    
    if (Test-Path -Path $iconsDir) {
        Remove-Item -Path $iconsDir -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  Removed icons directory" -ForegroundColor Gray
    }
}

# Clean temporary files
Write-Host "Cleaning temporary files..." -ForegroundColor Yellow
foreach ($pattern in $tempFiles) {
    Get-ChildItem -Path $scriptPath -Recurse -Filter $pattern -File | ForEach-Object {
        Remove-Item -Path $_.FullName -Force -ErrorAction SilentlyContinue
        Write-Host "  Removed $($_.FullName)" -ForegroundColor Gray
    }
}

# Clean other unnecessary files
$cleanVsFiles = Read-Host "Do you want to clean Visual Studio files (.vs folder)? (y/n)"
if ($cleanVsFiles -eq "y") {
    $vsDir = "$scriptPath\.vs"
    if (Test-Path -Path $vsDir) {
        Remove-Item -Path $vsDir -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  Removed .vs directory" -ForegroundColor Gray
    }
}

# Remove other unnecessary files
$filesToRemove = @(
    "$scriptPath\win.png",  # Original PNG since we use it directly
    "$scriptPath\WindowsCleaner\Resources\Icons\app.png", # Unnecessary copied PNG
    "$scriptPath\WindowsCleaner\Resources\Icons\app.ico"  # Unnecessary icon file
)

foreach ($file in $filesToRemove) {
    if (Test-Path -Path $file) {
        Remove-Item -Path $file -Force -ErrorAction SilentlyContinue
        Write-Host "  Removed $file" -ForegroundColor Gray
    }
}

Write-Host "Project cleaned successfully!" -ForegroundColor Green

# Git options
$gitCommit = Read-Host "Do you want to commit clean state to Git? (y/n)"
if ($gitCommit -eq "y") {
    $commitMessage = Read-Host "Enter commit message"
    git add .
    git commit -m $commitMessage
    
    $gitPush = Read-Host "Push changes to remote repository? (y/n)"
    if ($gitPush -eq "y") {
        git push
    }
    
    Write-Host "Git operations completed!" -ForegroundColor Green
}

Write-Host "Cleanup script completed." -ForegroundColor Cyan
