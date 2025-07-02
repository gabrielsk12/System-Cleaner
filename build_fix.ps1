Write-Host "Creating missing resource files if needed..."

$resourceDir = "d:\Projects\cleaner\WindowsCleaner\Resources\Strings"
$languages = @("en-US", "de-DE", "sk-SK", "cs-CZ", "ru-RU", "ja-JP", "ko-KR", "zh-CN")

foreach ($lang in $languages) {
    $resxFile = Join-Path $resourceDir "$lang.resx"
    if (-not (Test-Path $resxFile)) {
        Write-Host "Creating placeholder for $lang.resx"
        Copy-Item (Join-Path $resourceDir "en-US.resx") $resxFile
    }
}

Write-Host "Building the project..."
cd "d:\Projects\cleaner"
dotnet build WindowsCleaner.sln

Write-Host "Done."
