Write-Host "Building Windows System Cleaner Pro..." -ForegroundColor Cyan

# Get script directory if executed directly, otherwise use working directory
$scriptPath = if ($PSScriptRoot) { $PSScriptRoot } else { Get-Location }
Set-Location -Path $scriptPath

# Check if we're in the right directory
if (-not (Test-Path -Path "WindowsCleaner\WindowsCleaner.csproj")) {
    Write-Host "Error: Could not find project file. Make sure you're running this script from the root folder." -ForegroundColor Red
    exit 1
}

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path -Path "WindowsCleaner\bin") {
    Remove-Item -Path "WindowsCleaner\bin" -Recurse -Force -ErrorAction SilentlyContinue
}
if (Test-Path -Path "WindowsCleaner\obj") {
    Remove-Item -Path "WindowsCleaner\obj" -Recurse -Force -ErrorAction SilentlyContinue
}

# Clean solution if needed
Write-Host "Running dotnet clean..." -ForegroundColor Yellow
dotnet clean WindowsCleaner\WindowsCleaner.csproj

# Build Debug version first
Write-Host "Building Debug version..." -ForegroundColor Yellow
dotnet build WindowsCleaner\WindowsCleaner.csproj -c Debug

# If Debug build is successful, build Release version
if ($LASTEXITCODE -eq 0) {
    Write-Host "Building Release version..." -ForegroundColor Yellow
    dotnet publish WindowsCleaner\WindowsCleaner.csproj -c Release -r win-x64 --self-contained
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Build completed successfully!" -ForegroundColor Green
        
        # Show build output location
        $outputPath = "WindowsCleaner\bin\Release\net8.0-windows\win-x64\publish"
        if (Test-Path -Path $outputPath) {
            Write-Host "Output files available at: $((Get-Item $outputPath).FullName)" -ForegroundColor Green
            Get-ChildItem -Path $outputPath -File | Select-Object Name, Length | Format-Table
        }
    } else {
        Write-Host "Release build failed!" -ForegroundColor Red
    }
} else {
    Write-Host "Debug build failed!" -ForegroundColor Red
}

# Git commit if requested
$commitChanges = Read-Host "Do you want to commit changes to Git? (y/n)"
if ($commitChanges -eq "y") {
    $commitMessage = Read-Host "Enter commit message"
    git add .
    git commit -m $commitMessage
    git push
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Changes committed and pushed to Git successfully!" -ForegroundColor Green
    } else {
        Write-Host "Git commit/push failed!" -ForegroundColor Red
    }
}

# Open the output directory if the build was successful
if ($LASTEXITCODE -eq 0) {
    $outputPath = "WindowsCleaner\bin\Release\net8.0-windows\win-x64\publish"
    if (Test-Path -Path $outputPath) {
        $response = Read-Host "Do you want to open the output directory? (y/n)"
        if ($response -eq "y") {
            Invoke-Item $outputPath
        }
    }
}

Write-Host "Script completed." -ForegroundColor Cyan
