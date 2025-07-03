[Setup]
; Basic Information
AppId={{A92DAB39-4E2C-4304-B47B-51B5C56A7AAE}
AppName=Windows System Cleaner Pro
AppVersion=2.0.0
AppVerName=Windows System Cleaner Pro 2.0.0
AppPublisher=System Optimizer
AppPublisherURL=https://github.com/gabrielsk12/System-Cleaner
AppSupportURL=https://github.com/gabrielsk12/System-Cleaner/issues
AppUpdatesURL=https://github.com/gabrielsk12/System-Cleaner/releases
AppCopyright=Copyright (C) 2025 System Optimizer
DefaultDirName={autopf}\Windows System Cleaner Pro
DefaultGroupName=Windows System Cleaner Pro
AllowNoIcons=yes
LicenseFile=LICENSE
InfoBeforeFile=SETUP_INFO.txt
OutputDir=installer
OutputBaseFilename=WindowsSystemCleanerPro-2.0.0-Setup
SetupIconFile=WindowsCleanerNew\Resources\Icons\app.png
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
MinVersion=10.0.17763
PrivilegesRequired=admin
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\WindowsCleanerNew.exe

; Modern UI Settings
WizardImageFile=compiler:WizModernImage-IS.bmp
WizardSmallImageFile=compiler:WizModernSmallImage-IS.bmp
DisableWelcomePage=no
WizardImageStretch=no
WizardImageAlphaFormat=defined

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "slovak"; MessagesFile: "compiler:Languages\Slovak.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode
Name: "startmenu"; Description: "Add to Start Menu"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce
Name: "startup"; Description: "Start with Windows (recommended)"; GroupDescription: "Startup Options"; Flags: unchecked

[Files]
; Main Application
Source: "WindowsCleanerNew\bin\Release\net8.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; .NET 8 Runtime (if needed)
Source: "redist\dotnet-runtime-8.0-win-x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsDotNet8Installed

; Visual C++ Redistributables (if needed)
Source: "redist\VC_redist.x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsVCRedistInstalled

; Documentation
Source: "README.md"; DestDir: "{app}"; DestName: "README.txt"; Flags: ignoreversion
Source: "LICENSE"; DestDir: "{app}"; Flags: ignoreversion
Source: "PROJECT_STATUS.md"; DestDir: "{app}"; DestName: "PROJECT_STATUS.txt"; Flags: ignoreversion

[Icons]
Name: "{group}\Windows System Cleaner Pro"; Filename: "{app}\WindowsCleanerNew.exe"; IconFilename: "{app}\WindowsCleanerNew.exe"; Parameters: ""; WorkingDir: "{app}"; Comment: "Advanced Windows system cleaner and optimizer"
Name: "{group}\{cm:UninstallProgram,Windows System Cleaner Pro}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Windows System Cleaner Pro"; Filename: "{app}\WindowsCleanerNew.exe"; IconFilename: "{app}\WindowsCleanerNew.exe"; Tasks: desktopicon; Comment: "Advanced Windows system cleaner and optimizer"
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\Windows System Cleaner Pro"; Filename: "{app}\WindowsCleanerNew.exe"; Tasks: quicklaunchicon

[Run]
; Install .NET 8 Runtime if needed
Filename: "{tmp}\dotnet-runtime-8.0-win-x64.exe"; Parameters: "/quiet /norestart"; WorkingDir: "{tmp}"; StatusMsg: "Installing .NET 8 Runtime..."; Check: not IsDotNet8Installed; Flags: waituntilterminated

; Install Visual C++ Redistributable if needed
Filename: "{tmp}\VC_redist.x64.exe"; Parameters: "/quiet /norestart"; WorkingDir: "{tmp}"; StatusMsg: "Installing Visual C++ Redistributable..."; Check: not IsVCRedistInstalled; Flags: waituntilterminated

; Run the application after installation
Filename: "{app}\WindowsCleanerNew.exe"; Description: "{cm:LaunchProgram,Windows System Cleaner Pro}"; Flags: nowait postinstall skipifsilent runascurrentuser

[UninstallRun]
; Clean up scheduled tasks and registry entries
Filename: "{app}\WindowsCleanerNew.exe"; Parameters: "--uninstall-cleanup"; RunOnceId: "CleanupSettings"; Flags: runhidden

[Registry]
; Add to Windows Programs and Features
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{A92DAB39-4E2C-4304-B47B-51B5C56A7AAE}_is1"; ValueType: string; ValueName: "DisplayName"; ValueData: "Windows System Cleaner Pro"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{A92DAB39-4E2C-4304-B47B-51B5C56A7AAE}_is1"; ValueType: string; ValueName: "DisplayVersion"; ValueData: "2.0.0"
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{{A92DAB39-4E2C-4304-B47B-51B5C56A7AAE}_is1"; ValueType: string; ValueName: "Publisher"; ValueData: "System Optimizer"

; File associations for .cleaner files (optional)
Root: HKCR; Subkey: ".cleaner"; ValueType: string; ValueName: ""; ValueData: "WindowsCleanerFile"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "WindowsCleanerFile"; ValueType: string; ValueName: ""; ValueData: "Windows Cleaner Profile"; Flags: uninsdeletekey
Root: HKCR; Subkey: "WindowsCleanerFile\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\WindowsCleanerNew.exe,0"
Root: HKCR; Subkey: "WindowsCleanerFile\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\WindowsCleanerNew.exe"" ""%1"""

; Startup entry if requested
Root: HKCU; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "WindowsSystemCleanerPro"; ValueData: """{app}\WindowsCleanerNew.exe"" --minimized"; Tasks: startup; Flags: uninsdeletevalue

[Code]
// Check if .NET 8 Runtime is installed
function IsDotNet8Installed: Boolean;
var
  Version: String;
  Success: Boolean;
begin
  Result := False;
  Success := RegQueryStringValue(HKLM, 'SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedframework\Microsoft.NETCore.App', '8.0', Version);
  if not Success then
    Success := RegQueryStringValue(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedframework\Microsoft.NETCore.App', '8.0', Version);
  
  if Success then
    Result := True;
end;

// Check if Visual C++ Redistributable is installed
function IsVCRedistInstalled: Boolean;
var
  Version: String;
begin
  Result := RegQueryStringValue(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\VisualStudio\14.0\VC\Runtimes\x64', 'Version', Version);
  if not Result then
    Result := RegQueryStringValue(HKLM, 'SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64', 'Version', Version);
end;

// Custom installation pages
procedure InitializeWizard;
begin
  // Custom welcome message
  WizardForm.WelcomeLabel2.Caption := 'This will install Windows System Cleaner Pro 2.0.0 on your computer.' + #13#10#13#10 +
    'Windows System Cleaner Pro is an advanced system optimization tool that helps you:' + #13#10 +
    '• Clean temporary files and system junk' + #13#10 +
    '• Manage and update device drivers' + #13#10 +
    '• Optimize system performance' + #13#10 +
    '• Schedule automatic maintenance' + #13#10#13#10 +
    'It is recommended that you close all other applications before continuing.';
end;

// Pre-installation checks
function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  
  // Check Windows version compatibility
  if CurPageID = wpWelcome then
  begin
    if GetWindowsVersion < $0A000000 then  // Windows 10 version 1507
    begin
      MsgBox('Windows System Cleaner Pro requires Windows 10 version 1809 or later.' + #13#10 +
             'Your current Windows version is not supported.', mbError, MB_OK);
      Result := False;
    end;
  end;
end;

// Post-installation setup
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Create application data directory
    ForceDirectories(ExpandConstant('{localappdata}\WindowsCleaner'));
    
    // Set permissions for the application directory
    // This ensures the app can write logs and settings
  end;
end;
