@echo off
echo ========================================
echo   Windows System Cleaner Pro Builder
echo ========================================
echo.

REM Check if running as administrator
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo This build script works best with administrator privileges.
    echo Right-click and select "Run as administrator" for full features.
    echo.
    echo Continuing with standard user privileges...
    timeout /t 3 /nobreak >nul
)

echo Starting PowerShell build system...
echo.

REM Run the PowerShell build script
powershell -ExecutionPolicy Bypass -File "create_installer.ps1"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo   Build and Installer Creation Complete!
    echo ========================================
    echo.
    echo The installer has been created: WindowsCleanerPro_Setup.exe
    echo.
    echo You can now:
    echo 1. Test the installer locally
    echo 2. Distribute the installer to users
    echo 3. Upload to GitHub releases
    echo.
) else (
    echo.
    echo ========================================
    echo   Build Failed!
    echo ========================================
    echo.
    echo Please check the error messages above.
    echo.
)

pause
