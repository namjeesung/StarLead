#define MyAppName "星引"
#define MyAppVersion "0.32.0"
#define MyAppPublisher "StarLead"
#define MyAppExeName "StarLead.exe"

[Setup]
AppId={{B72C0F10-D6B7-49D7-9D1B-A4E668E31A75}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\StarLead
DefaultGroupName=星引
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\installer-output
OutputBaseFilename=StarLead-Setup-x64-v0.32
SetupIconFile=..\Assets\StarLead.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
DisableProgramGroupPage=yes

[Files]
Source: "..\publish\win-x64\StarLead.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\星引"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\星引"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加快捷方式："; Flags: unchecked

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "StarLead"; Flags: uninsdeletevalue dontcreatekey

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "启动星引"; Flags: nowait postinstall skipifsilent

[Code]
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
begin
  { The tray window intentionally hides on WM_CLOSE, so terminate the old user
    process before replacing files. This also prevents an old build remaining
    active after an upgrade. }
  Exec(ExpandConstant('{cmd}'), '/C taskkill /F /IM StarLead.exe >nul 2>&1', '', SW_HIDE,
    ewWaitUntilTerminated, ResultCode);
  Result := '';
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
    RegDeleteValue(HKEY_CURRENT_USER, 'Software\Microsoft\Windows\CurrentVersion\Run', 'StarLead');
end;
