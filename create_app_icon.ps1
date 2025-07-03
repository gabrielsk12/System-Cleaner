# PowerShell script to create a proper application icon
Add-Type -AssemblyName System.Drawing

# Create a 256x256 bitmap for high quality
$bitmap = New-Object System.Drawing.Bitmap(256, 256)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$graphics.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAlias

# Create gradient background
$brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    [System.Drawing.Point]::new(0, 0),
    [System.Drawing.Point]::new(256, 256),
    [System.Drawing.Color]::FromArgb(70, 130, 180),  # Steel Blue
    [System.Drawing.Color]::FromArgb(25, 25, 112)    # Midnight Blue
)
$graphics.FillRectangle($brush, 0, 0, 256, 256)

# Add a cleaning brush icon effect
$whiteBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$font = New-Object System.Drawing.Font("Segoe UI", 120, [System.Drawing.FontStyle]::Bold)
$textFormat = New-Object System.Drawing.StringFormat
$textFormat.Alignment = [System.Drawing.StringAlignment]::Center
$textFormat.LineAlignment = [System.Drawing.StringAlignment]::Center

# Draw the main "C" for Cleaner
$graphics.DrawString("C", $font, $whiteBrush, [System.Drawing.RectangleF]::new(0, 0, 256, 256), $textFormat)

# Add a subtle border
$borderPen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 4)
$graphics.DrawRectangle($borderPen, 2, 2, 252, 252)

# Save multiple sizes for ICO format
$outputPath = "WindowsCleanerNew\Resources\Icons\app.ico"
New-Item -ItemType Directory -Path "WindowsCleanerNew\Resources\Icons" -Force | Out-Null

# Convert to ICO format with multiple sizes
$bitmap256 = $bitmap
$bitmap128 = New-Object System.Drawing.Bitmap($bitmap, 128, 128)
$bitmap64 = New-Object System.Drawing.Bitmap($bitmap, 64, 64)
$bitmap48 = New-Object System.Drawing.Bitmap($bitmap, 48, 48)
$bitmap32 = New-Object System.Drawing.Bitmap($bitmap, 32, 32)
$bitmap16 = New-Object System.Drawing.Bitmap($bitmap, 16, 16)

# Create ICO file manually
$iconSizes = @(
    @{Size=256; Bitmap=$bitmap256},
    @{Size=128; Bitmap=$bitmap128},
    @{Size=64; Bitmap=$bitmap64},
    @{Size=48; Bitmap=$bitmap48},
    @{Size=32; Bitmap=$bitmap32},
    @{Size=16; Bitmap=$bitmap16}
)

# For now, save as PNG (we'll convert to ICO in the build process)
$bitmap.Save($outputPath.Replace(".ico", ".png"), [System.Drawing.Imaging.ImageFormat]::Png)

# Clean up
$graphics.Dispose()
$bitmap.Dispose()
$bitmap128.Dispose()
$bitmap64.Dispose()
$bitmap48.Dispose()
$bitmap32.Dispose()
$bitmap16.Dispose()
$brush.Dispose()
$whiteBrush.Dispose()
$font.Dispose()
$borderPen.Dispose()

Write-Host "Icon created successfully at: $outputPath (as PNG)"
Write-Host "Use online converter or ImageMagick to convert PNG to ICO format"
