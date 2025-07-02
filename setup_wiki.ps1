# GitHub Wiki Automation Script
# This script helps set up the GitHub wiki for Windows System Cleaner Pro

param(
    [string]$WikiAction = "help"
)

$ErrorActionPreference = "Stop"

function Show-Help {
    Write-Host "=== GitHub Wiki Setup for Windows System Cleaner Pro ===" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Usage: .\setup_wiki.ps1 [action]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Actions:" -ForegroundColor Green
    Write-Host "  help      - Show this help message"
    Write-Host "  check     - Check wiki content and repository status"
    Write-Host "  prepare   - Prepare wiki files for manual upload"
    Write-Host "  clone     - Clone and setup wiki repository (after first page created)"
    Write-Host ""
    Write-Host "Manual Steps Required:" -ForegroundColor Red
    Write-Host "1. Go to: https://github.com/gabrielsk12/System-Cleaner"
    Write-Host "2. Click 'Wiki' tab"
    Write-Host "3. Click 'Create the first page'"
    Write-Host "4. Title: 'Home'"
    Write-Host "5. Copy content from wiki/Home.md"
    Write-Host "6. Save page"
    Write-Host "7. Run: .\setup_wiki.ps1 clone"
    Write-Host ""
}

function Test-WikiFiles {
    Write-Host "Checking wiki files..." -ForegroundColor Yellow
    
    $wikiFiles = @(
        "wiki/Home.md",
        "wiki/Installation-Guide.md", 
        "wiki/Quick-Start-Tutorial.md",
        "wiki/System-Cleaner-Guide.md",
        "wiki/Troubleshooting.md"
    )
    
    $allExist = $true
    foreach ($file in $wikiFiles) {
        if (Test-Path $file) {
            $size = (Get-Item $file).Length
            Write-Host "✅ $file ($size bytes)" -ForegroundColor Green
        } else {
            Write-Host "❌ $file (missing)" -ForegroundColor Red
            $allExist = $false
        }
    }
    
    return $allExist
}

function Test-Repository {
    Write-Host "Checking repository status..." -ForegroundColor Yellow
    
    try {
        $status = git status --porcelain
        if ($status) {
            Write-Host "⚠️  Uncommitted changes found:" -ForegroundColor Yellow
            Write-Host $status
        } else {
            Write-Host "✅ Repository is clean" -ForegroundColor Green
        }
        
        $remote = git remote get-url origin
        Write-Host "✅ Remote: $remote" -ForegroundColor Green
        
        $tags = git tag -l "v1.0.0"
        if ($tags) {
            Write-Host "✅ Release tag v1.0.0 exists" -ForegroundColor Green
        } else {
            Write-Host "❌ Release tag v1.0.0 not found" -ForegroundColor Red
        }
        
    } catch {
        Write-Host "❌ Git repository error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    
    return $true
}

function Test-GitHubRelease {
    Write-Host "Checking GitHub release..." -ForegroundColor Yellow
    
    try {
        $releases = gh release list --json tagName,name | ConvertFrom-Json
        $v1Release = $releases | Where-Object { $_.tagName -eq "v1.0.0" }
        
        if ($v1Release) {
            Write-Host "✅ GitHub release v1.0.0 exists: $($v1Release.name)" -ForegroundColor Green
            
            # Check for EXE asset
            $assets = gh release view v1.0.0 --json assets | ConvertFrom-Json
            $exeAsset = $assets.assets | Where-Object { $_.name -like "*.exe" }
            
            if ($exeAsset) {
                Write-Host "✅ Installer EXE attached: $($exeAsset.name) ($([math]::Round($exeAsset.size/1MB, 1)) MB)" -ForegroundColor Green
            } else {
                Write-Host "❌ No EXE file found in release" -ForegroundColor Red
            }
        } else {
            Write-Host "❌ GitHub release v1.0.0 not found" -ForegroundColor Red
        }
        
    } catch {
        Write-Host "❌ GitHub CLI error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    
    return $true
}

function Invoke-PrepareWiki {
    Write-Host "Preparing wiki files for manual upload..." -ForegroundColor Yellow
    
    if (-not (Test-WikiFiles)) {
        Write-Host "❌ Wiki files are missing or incomplete" -ForegroundColor Red
        return
    }
    
    # Create a summary file for easy copying
    $summaryPath = "wiki_upload_guide.txt"
    $summary = @"
=== WIKI UPLOAD GUIDE ===

Your wiki content is ready! Follow these steps:

1. Go to: https://github.com/gabrielsk12/System-Cleaner
2. Click the "Wiki" tab
3. Click "Create the first page" (if first time)

PAGE CREATION ORDER:
====================

1. HOME PAGE (Create this first!)
   Title: Home
   File: wiki/Home.md
   
2. INSTALLATION GUIDE
   Title: Installation-Guide  
   File: wiki/Installation-Guide.md
   
3. QUICK START TUTORIAL
   Title: Quick-Start-Tutorial
   File: wiki/Quick-Start-Tutorial.md
   
4. SYSTEM CLEANER GUIDE
   Title: System-Cleaner-Guide
   File: wiki/System-Cleaner-Guide.md
   
5. TROUBLESHOOTING
   Title: Troubleshooting
   File: wiki/Troubleshooting.md

INSTRUCTIONS:
============
- Copy the content from each .md file exactly as-is
- Use the exact titles shown above (GitHub will auto-format URLs)
- Save each page before moving to the next
- The Home page creates navigation links to other pages

After creating all pages manually, you can run:
.\setup_wiki.ps1 clone

This will set up automated wiki management for future updates.
"@
    
    Set-Content -Path $summaryPath -Value $summary
    Write-Host "✅ Created upload guide: $summaryPath" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Open: $summaryPath"
    Write-Host "2. Follow the manual upload instructions"
    Write-Host "3. Run: .\setup_wiki.ps1 clone (after creating Home page)"
}

function Invoke-CloneWiki {
    Write-Host "Setting up automated wiki management..." -ForegroundColor Yellow
    
    $wikiUrl = "https://github.com/gabrielsk12/System-Cleaner.wiki.git"
    $wikiDir = "wiki-repo"
    
    if (Test-Path $wikiDir) {
        Write-Host "Removing existing wiki directory..." -ForegroundColor Yellow
        Remove-Item $wikiDir -Recurse -Force
    }
    
    try {
        Write-Host "Cloning wiki repository..." -ForegroundColor Yellow
        git clone $wikiUrl $wikiDir
        
        if (-not (Test-Path $wikiDir)) {
            throw "Wiki clone failed - repository may not exist yet. Create the Home page first!"
        }
        
        Push-Location $wikiDir
        
        Write-Host "Copying wiki files..." -ForegroundColor Yellow
        Copy-Item "..\wiki\*.md" . -Force
        
        Write-Host "Committing changes..." -ForegroundColor Yellow
        git add .
        git commit -m "Add comprehensive wiki documentation for v1.0.0"
        git push origin master
        
        Write-Host "✅ Wiki setup completed successfully!" -ForegroundColor Green
        Write-Host "View your wiki at: https://github.com/gabrielsk12/System-Cleaner/wiki" -ForegroundColor Cyan
        
        Pop-Location
        
    } catch {
        Write-Host "❌ Wiki setup failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        Write-Host "This usually means the wiki hasn't been initialized yet." -ForegroundColor Yellow
        Write-Host "Create the Home page manually first, then try again." -ForegroundColor Yellow
        
        if (Get-Location | Select-Object -ExpandProperty Path | Where-Object { $_ -like "*wiki-repo*" }) {
            Pop-Location
        }
    }
}

function Invoke-CheckAll {
    Write-Host "=== Windows System Cleaner Pro - Status Check ===" -ForegroundColor Cyan
    Write-Host ""
    
    $wikiOk = Test-WikiFiles
    Write-Host ""
    
    $repoOk = Test-Repository  
    Write-Host ""
    
    $releaseOk = Test-GitHubRelease
    Write-Host ""
    
    Write-Host "=== SUMMARY ===" -ForegroundColor Cyan
    if ($wikiOk -and $repoOk -and $releaseOk) {
        Write-Host "✅ Everything looks good! Ready for wiki setup." -ForegroundColor Green
        Write-Host ""
        Write-Host "Next step: Run .\setup_wiki.ps1 prepare" -ForegroundColor Yellow
    } else {
        Write-Host "⚠️  Some issues found. Please resolve them first." -ForegroundColor Yellow
    }
}

# Main execution
switch ($WikiAction.ToLower()) {
    "help" { Show-Help }
    "check" { Invoke-CheckAll }
    "prepare" { Invoke-PrepareWiki }
    "clone" { Invoke-CloneWiki }
    default { 
        Write-Host "Unknown action: $WikiAction" -ForegroundColor Red
        Show-Help
    }
}
