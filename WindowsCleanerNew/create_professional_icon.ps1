# Create a proper ICO file for the application
Add-Type -AssemblyName System.Drawing

# Create multiple icon sizes for better Windows integration
$sizes = @(16, 24, 32, 48, 64, 128, 256)
$icons = @()

foreach ($size in $sizes) {
    # Create bitmap
    $bitmap = New-Object System.Drawing.Bitmap($size, $size)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    
    # Create gradient background
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        [System.Drawing.Point]::new(0, 0),
        [System.Drawing.Point]::new($size, $size),
        [System.Drawing.Color]::FromArgb(45, 125, 255),
        [System.Drawing.Color]::FromArgb(25, 85, 215)
    )
    $graphics.FillRectangle($brush, 0, 0, $size, $size)
    
    # Add cleaning brush icon effect
    $pen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, [Math]::Max(1, $size / 32))
    $graphics.DrawEllipse($pen, $size * 0.2, $size * 0.2, $size * 0.6, $size * 0.6)
    
    # Add "C" for Cleaner
    $fontSize = [Math]::Max(8, $size * 0.5)
    $font = New-Object System.Drawing.Font("Arial", $fontSize, [System.Drawing.FontStyle]::Bold)
    $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    
    $textSize = $graphics.MeasureString("C", $font)
    $x = ($size - $textSize.Width) / 2
    $y = ($size - $textSize.Height) / 2
    $graphics.DrawString("C", $font, $textBrush, $x, $y)
    
    # Save as PNG first
    $tempFile = [System.IO.Path]::GetTempFileName() + "_$size.png"
    $bitmap.Save($tempFile, [System.Drawing.Imaging.ImageFormat]::Png)
    $icons += $tempFile
    
    # Cleanup
    $graphics.Dispose()
    $bitmap.Dispose()
    $brush.Dispose()
    $pen.Dispose()
    $font.Dispose()
    $textBrush.Dispose()
}

# Try to convert to ICO using ImageMagick if available
try {
    $magick = Get-Command "magick.exe" -ErrorAction SilentlyContinue
    if ($magick) {
        $iconArgs = $icons -join " "
        $outputPath = "Resources\Icons\app.ico"
        & magick.exe $icons $outputPath
        Write-Host "Professional multi-size icon created at: $outputPath"
    } else {
        # Fallback: use the largest size as ICO
        Copy-Item $icons[-1] "Resources\Icons\app.ico"
        Write-Host "Basic icon created at: Resources\Icons\app.ico"
        Write-Host "For best results, install ImageMagick for multi-size ICO creation"
    }
} catch {
    Write-Error "Failed to create icon: $_"
} finally {
    # Clean up temp files
    foreach ($tempFile in $icons) {
        if (Test-Path $tempFile) {
            Remove-Item $tempFile -Force
        }
    }
}
