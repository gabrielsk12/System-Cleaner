# Create a proper Windows icon file
Add-Type -AssemblyName System.Drawing

# Create a new 32x32 icon
$size = 32
$bitmap = New-Object System.Drawing.Bitmap($size, $size)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)

# Clear with transparent background
$graphics.Clear([System.Drawing.Color]::Transparent)

# Draw a simple cleaning icon (broom/cleaner)
$brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::DodgerBlue)
$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::Navy, 2)

# Draw main cleaning tool body
$graphics.FillEllipse($brush, 4, 4, 24, 24)
$graphics.DrawEllipse($pen, 4, 4, 24, 24)

# Draw cleaning bristles/lines
$pen2 = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 1)
for ($i = 8; $i -lt 24; $i += 3) {
    $graphics.DrawLine($pen2, $i, 10, $i, 22)
}

# Draw handle
$pen3 = New-Object System.Drawing.Pen([System.Drawing.Color]::SaddleBrown, 3)
$graphics.DrawLine($pen3, 16, 8, 16, 2)

$graphics.Dispose()

# Convert to icon
$iconPath = "Resources\Icons\app.ico"
$stream = New-Object System.IO.FileStream($iconPath, [System.IO.FileMode]::Create)

try {
    # Create icon from bitmap
    $icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
    $icon.Save($stream)
    Write-Host "✅ Created icon: $iconPath" -ForegroundColor Green
}
catch {
    Write-Warning "Failed to create icon: $_"
    
    # Fallback: try to use the PNG file as base
    $pngPath = "Resources\Icons\app.png"
    if (Test-Path $pngPath) {
        Write-Host "🔄 Converting PNG to ICO..." -ForegroundColor Yellow
        $pngBitmap = [System.Drawing.Bitmap]::FromFile($pngPath)
        $resizedBitmap = New-Object System.Drawing.Bitmap($pngBitmap, 32, 32)
        $icon = [System.Drawing.Icon]::FromHandle($resizedBitmap.GetHicon())
        $icon.Save($stream)
        Write-Host "✅ Converted PNG to ICO" -ForegroundColor Green
        $pngBitmap.Dispose()
        $resizedBitmap.Dispose()
    }
}
finally {
    $stream.Close()
    $bitmap.Dispose()
    if ($brush) { $brush.Dispose() }
    if ($pen) { $pen.Dispose() }
    if ($pen2) { $pen2.Dispose() }
    if ($pen3) { $pen3.Dispose() }
}

Write-Host "Icon file created at: $iconPath"
