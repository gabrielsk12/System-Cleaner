# PowerShell script to create a basic application icon
param(
    [string]$OutputPath = "Resources\Icons\app.ico"
)

# Create a simple bitmap and convert to ICO format
Add-Type -AssemblyName System.Drawing

# Create a 32x32 bitmap
$bitmap = New-Object System.Drawing.Bitmap(32, 32)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)

# Set background color to blue
$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::Blue)
$graphics.FillRectangle($brush, 0, 0, 32, 32)

# Draw a simple "C" for Cleaner
$font = New-Object System.Drawing.Font("Arial", 20, [System.Drawing.FontStyle]::Bold)
$textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$graphics.DrawString("C", $font, $textBrush, 4, 2)

# Save as temporary PNG first
$tempPng = [System.IO.Path]::GetTempFileName() + ".png"
$bitmap.Save($tempPng, [System.Drawing.Imaging.ImageFormat]::Png)

# Clean up
$graphics.Dispose()
$bitmap.Dispose()
$brush.Dispose()
$textBrush.Dispose()
$font.Dispose()

# Convert PNG to ICO using built-in Windows capability
try {
    # Try using magick (ImageMagick) if available
    $magickPath = Get-Command "magick.exe" -ErrorAction SilentlyContinue
    if ($magickPath) {
        & magick.exe $tempPng -resize 32x32 $OutputPath
        Write-Host "Icon created successfully using ImageMagick at: $OutputPath"
    } else {
        # Fallback: Just copy the PNG as ICO (some apps can handle this)
        Copy-Item $tempPng $OutputPath
        Write-Host "Icon created (PNG format) at: $OutputPath"
        Write-Host "Note: For proper ICO format, install ImageMagick"
    }
} catch {
    Write-Error "Failed to create icon: $_"
} finally {
    # Clean up temp file
    if (Test-Path $tempPng) {
        Remove-Item $tempPng
    }
}
