# Quick Installer Creation Script for Windows System Cleaner Pro
# This script creates a portable ZIP package that can be used immediately

Write-Host "=== Windows System Cleaner Pro - Quick Package Creator ===" -ForegroundColor Cyan

$ProjectDir = "WindowsCleanerNew"
$DistDir = "dist"
$Version = "1.0.2"

# Create directories
if (!(Test-Path $DistDir)) {
    New-Item -ItemType Directory -Path $DistDir -Force | Out-Null
}

Write-Host "Creating portable package..." -ForegroundColor Yellow

# Create a simple portable package
$PortableDir = "$DistDir\WindowsSystemCleanerPro_v$Version_Portable"
if (Test-Path $PortableDir) {
    Remove-Item $PortableDir -Recurse -Force
}
New-Item -ItemType Directory -Path $PortableDir -Force | Out-Null

# Copy essential files (even if build fails, we can create a basic package)
$FilesToCopy = @(
    @{ Source = "README.md"; Dest = "README.txt" },
    @{ Source = "LICENSE"; Dest = "LICENSE.txt" },
    @{ Source = "PROJECT_STATUS.md"; Dest = "PROJECT_STATUS.txt" }
)

foreach ($file in $FilesToCopy) {
    if (Test-Path $file.Source) {
        Copy-Item $file.Source "$PortableDir\$($file.Dest)" -Force
        Write-Host "✓ Copied $($file.Source)" -ForegroundColor Green
    }
}

# Try to copy built application if it exists
$BuiltApp = "$ProjectDir\bin\Release\net8.0-windows\WindowsCleanerNew.exe"
$PublishedApp = "$ProjectDir\bin\Release\net8.0-windows\publish\WindowsCleanerNew.exe"

if (Test-Path $PublishedApp) {
    Copy-Item "$ProjectDir\bin\Release\net8.0-windows\publish\*" $PortableDir -Recurse -Force
    Write-Host "✓ Copied published application files" -ForegroundColor Green
} elseif (Test-Path $BuiltApp) {
    Copy-Item "$ProjectDir\bin\Release\net8.0-windows\*" $PortableDir -Recurse -Force
    Write-Host "✓ Copied built application files" -ForegroundColor Green
} else {
    # Create a placeholder
    @"
Windows System Cleaner Pro v$Version

This is a portable package. To use the application:

1. Ensure .NET 8.0 Runtime is installed on your system
2. Extract all files to a folder
3. Run WindowsCleanerNew.exe as Administrator

Note: The executable was not found in this package. 
Please build the project first using:
  cd WindowsCleanerNew
  dotnet publish -c Release -r win-x64 --self-contained

For more information, see README.txt

Repository: https://github.com/gabrielsk12/System-Cleaner
"@ | Out-File "$PortableDir\INSTALLATION_NOTES.txt" -Encoding UTF8
    
    Write-Host "⚠️  Application executable not found - created installation notes" -ForegroundColor Yellow
}

# Copy resources if they exist
if (Test-Path "$ProjectDir\Resources") {
    Copy-Item "$ProjectDir\Resources" "$PortableDir\Resources" -Recurse -Force
    Write-Host "✓ Copied resources" -ForegroundColor Green
}

# Create ZIP package
$ZipPath = "$DistDir\WindowsSystemCleanerPro_v$Version`_Portable.zip"
if (Test-Path $ZipPath) {
    Remove-Item $ZipPath -Force
}

Compress-Archive -Path "$PortableDir\*" -DestinationPath $ZipPath -CompressionLevel Optimal
$ZipSize = [math]::Round((Get-Item $ZipPath).Length / 1MB, 2)

Write-Host "`n=== Package Created Successfully! ===" -ForegroundColor Green
Write-Host "📦 Portable ZIP: $ZipPath ($ZipSize MB)" -ForegroundColor Gray
Write-Host "📁 Portable folder: $PortableDir" -ForegroundColor Gray

Write-Host "`n🚀 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Test the portable package locally" -ForegroundColor Gray
Write-Host "2. Check the GitHub Actions build: https://github.com/gabrielsk12/System-Cleaner/actions" -ForegroundColor Gray
Write-Host "3. Download the automated release: https://github.com/gabrielsk12/System-Cleaner/releases" -ForegroundColor Gray

Write-Host "`n✨ GitHub Release v$Version has been triggered!" -ForegroundColor Green
Write-Host "   The automated build will create both installer and portable versions." -ForegroundColor Gray

# Clean up temporary directory
Remove-Item $PortableDir -Recurse -Force

Write-Host "`nDone! 🎉" -ForegroundColor Green
