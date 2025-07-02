; Windows System Cleaner Pro - Professional NSIS Installer
; This creates a proper Windows installer executable
; Requires NSIS (Nullsoft Scriptable Install System) to compile

!define APPNAME "Windows System Cleaner Pro"
!define COMPANYNAME "System Optimizer"
!define DESCRIPTION "Professional Windows system optimization tool"
!define VERSIONMAJOR 1
!define VERSIONMINOR 0
!define VERSIONBUILD 0
!define HELPURL "https://github.com/gabrielsk12/System-Cleaner"
!define UPDATEURL "https://github.com/gabrielsk12/System-Cleaner/releases"
!define ABOUTURL "https://github.com/gabrielsk12/System-Cleaner"
!define INSTALLSIZE 51200 ; Estimate in KB

; Request application privileges for Windows Vista/7/8/10/11
RequestExecutionLevel admin

; Include Modern UI
!include "MUI2.nsh"
!include "LogicLib.nsh"
!include "WinVer.nsh"
!include "x64.nsh"

; General configuration
Name "${APPNAME}"
OutFile "WindowsCleanerPro_Setup.exe"
InstallDir "$PROGRAMFILES64\${APPNAME}"
InstallDirRegKey HKLM "Software\${COMPANYNAME}\${APPNAME}" "InstallLocation"
ShowInstDetails show
ShowUnInstDetails show

; Interface Settings
!define MUI_ABORTWARNING
!define MUI_ICON "WindowsCleaner\icon.ico"
!define MUI_UNICON "WindowsCleaner\icon.ico"
!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "installer_header.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP "installer_side.bmp"

; Pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE"
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN "$INSTDIR\WindowsCleaner.exe"
!define MUI_FINISHPAGE_SHOWREADME "$INSTDIR\README.md"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

; Languages
!insertmacro MUI_LANGUAGE "English"

; Version Information
VIProductVersion "${VERSIONMAJOR}.${VERSIONMINOR}.${VERSIONBUILD}.0"
VIAddVersionKey "ProductName" "${APPNAME}"
VIAddVersionKey "CompanyName" "${COMPANYNAME}"
VIAddVersionKey "LegalCopyright" "Copyright © 2025 ${COMPANYNAME}"
VIAddVersionKey "FileDescription" "${DESCRIPTION}"
VIAddVersionKey "FileVersion" "${VERSIONMAJOR}.${VERSIONMINOR}.${VERSIONBUILD}.0"
VIAddVersionKey "ProductVersion" "${VERSIONMAJOR}.${VERSIONMINOR}.${VERSIONBUILD}.0"

; Installer sections
Section "Core Application" SecCore
    SectionIn RO ; Required section
    
    ; Set output path
    SetOutPath "$INSTDIR"
    
    ; Main application files
    File "Build\WindowsCleaner.exe"
    File /nonfatal "Build\*.dll"
    File /nonfatal "Build\*.json"
    File "README.md"
    File "LICENSE"
    File "SETUP.md"
    
    ; Create application data directory
    CreateDirectory "$LOCALAPPDATA\WindowsCleanerPro"
    
    ; Write registry entries
    WriteRegStr HKLM "Software\${COMPANYNAME}\${APPNAME}" "InstallLocation" "$INSTDIR"
    WriteRegStr HKLM "Software\${COMPANYNAME}\${APPNAME}" "Version" "${VERSIONMAJOR}.${VERSIONMINOR}.${VERSIONBUILD}"
    
    ; Add to Programs and Features
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$INSTDIR\Uninstall.exe"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "QuietUninstallString" "$INSTDIR\Uninstall.exe /S"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "InstallLocation" "$INSTDIR"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayIcon" "$INSTDIR\WindowsCleaner.exe"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "Publisher" "${COMPANYNAME}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "HelpLink" "${HELPURL}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "URLUpdateInfo" "${UPDATEURL}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "URLInfoAbout" "${ABOUTURL}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayVersion" "${VERSIONMAJOR}.${VERSIONMINOR}.${VERSIONBUILD}"
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "VersionMajor" ${VERSIONMAJOR}
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "VersionMinor" ${VERSIONMINOR}
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoModify" 1
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "NoRepair" 1
    WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "EstimatedSize" ${INSTALLSIZE}
    
    ; Create uninstaller
    WriteUninstaller "$INSTDIR\Uninstall.exe"
SectionEnd

Section "Desktop Shortcut" SecDesktop
    CreateShortcut "$DESKTOP\${APPNAME}.lnk" "$INSTDIR\WindowsCleaner.exe" "" "$INSTDIR\WindowsCleaner.exe" 0
SectionEnd

Section "Start Menu Shortcuts" SecStartMenu
    CreateDirectory "$SMPROGRAMS\${APPNAME}"
    CreateShortcut "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk" "$INSTDIR\WindowsCleaner.exe" "" "$INSTDIR\WindowsCleaner.exe" 0
    CreateShortcut "$SMPROGRAMS\${APPNAME}\Uninstall.lnk" "$INSTDIR\Uninstall.exe" "" "$INSTDIR\Uninstall.exe" 0
    CreateShortcut "$SMPROGRAMS\${APPNAME}\User Guide.lnk" "$INSTDIR\SETUP.md" "" "" 0
SectionEnd

Section "Quick Launch Shortcut" SecQuickLaunch
    CreateShortcut "$QUICKLAUNCH\${APPNAME}.lnk" "$INSTDIR\WindowsCleaner.exe" "" "$INSTDIR\WindowsCleaner.exe" 0
SectionEnd

; Section descriptions
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
!insertmacro MUI_DESCRIPTION_TEXT ${SecCore} "Core application files (required)"
!insertmacro MUI_DESCRIPTION_TEXT ${SecDesktop} "Create a desktop shortcut"
!insertmacro MUI_DESCRIPTION_TEXT ${SecStartMenu} "Create Start Menu shortcuts"
!insertmacro MUI_DESCRIPTION_TEXT ${SecQuickLaunch} "Create Quick Launch shortcut"
!insertmacro MUI_FUNCTION_DESCRIPTION_END

; Installer Functions
Function .onInit
    ; Check if Windows 10 or later
    ${IfNot} ${AtLeastWin10}
        MessageBox MB_OK|MB_ICONSTOP "This application requires Windows 10 or later."
        Abort
    ${EndIf}
    
    ; Check for 64-bit system
    ${IfNot} ${RunningX64}
        MessageBox MB_OK|MB_ICONSTOP "This application requires a 64-bit version of Windows."
        Abort
    ${EndIf}
    
    ; Check if already installed
    ReadRegStr $R0 HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString"
    StrCmp $R0 "" done
    
    MessageBox MB_OKCANCEL|MB_ICONEXCLAMATION "${APPNAME} is already installed. Do you want to uninstall the previous version?" IDOK uninst
    Abort
    
    uninst:
        ClearErrors
        ExecWait '$R0 _?=$INSTDIR'
        
        IfErrors no_remove_uninstaller done
        no_remove_uninstaller:
    
    done:
FunctionEnd

; Uninstaller section
Section "Uninstall"
    ; Remove application files
    Delete "$INSTDIR\WindowsCleaner.exe"
    Delete "$INSTDIR\*.dll"
    Delete "$INSTDIR\*.json"
    Delete "$INSTDIR\README.md"
    Delete "$INSTDIR\LICENSE"
    Delete "$INSTDIR\SETUP.md"
    Delete "$INSTDIR\Uninstall.exe"
    
    ; Remove shortcuts
    Delete "$DESKTOP\${APPNAME}.lnk"
    Delete "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk"
    Delete "$SMPROGRAMS\${APPNAME}\Uninstall.lnk"
    Delete "$SMPROGRAMS\${APPNAME}\User Guide.lnk"
    Delete "$QUICKLAUNCH\${APPNAME}.lnk"
    RMDir "$SMPROGRAMS\${APPNAME}"
    
    ; Remove registry entries
    DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
    DeleteRegKey HKLM "Software\${COMPANYNAME}\${APPNAME}"
    DeleteRegKey /ifempty HKLM "Software\${COMPANYNAME}"
    
    ; Remove startup entry if exists
    DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "WindowsCleanerPro"
    
    ; Remove scheduled task if exists
    ExecWait 'schtasks /delete /tn "WindowsCleanerPro_AutoClean" /f'
    
    ; Ask user if they want to remove settings
    MessageBox MB_YESNO|MB_ICONQUESTION "Do you want to remove application settings and logs?" IDNO keep_settings
    RMDir /r "$LOCALAPPDATA\WindowsCleanerPro"
    
    keep_settings:
    
    ; Remove installation directory
    RMDir "$INSTDIR"
    
    ; Show completion message
    MessageBox MB_OK "${APPNAME} has been successfully uninstalled."
SectionEnd
