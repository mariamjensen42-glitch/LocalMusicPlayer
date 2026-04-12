; Inno Setup 安装脚本 for LocalMusicPlayer
; 生成 Windows 安装包

#define MyAppName "LocalMusicPlayer"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "LocalMusicPlayer"
#define MyAppURL "https://github.com/your-username/LocalMusicPlayer"
#define MyAppExeName "LocalMusicPlayer.exe"
#define MyAppTarget "win-x64"
#define MyAppArch "x64"

[Setup]
; 注意: AppId 是每个应用的唯一标识，不要更改
AppId={{B8F5D2A1-3C4E-4F5G-6H7I-8J9K0L1M2N3O}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; LicenseFile=..\LICENSE.txt
; 如果没有 LICENSE 文件，请注释掉上一行
OutputDir=..\installer-output
OutputBaseFilename=LocalMusicPlayer-{#MyAppVersion}-{#MyAppArch}-setup
SetupIconFile=..\Assets\avalonia-logo.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
; 请求管理员权限（用于安装到 Program Files）
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; 发布目录下的所有文件
Source: "..\publish\{#MyAppTarget}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; 注意: 不要在任何共享系统文件上使用 "Flags: ignoreversion"

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
