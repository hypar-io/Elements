#define MyAppName "Hypar Revit"
#define MyAppVersion "0.0.2"
#define MyAppPublisher "Hypar"
#define MyAppURL "https://github.com/hypar-io/Elements/tree/master/src/Revit"

#define RevitAppName  "Hypar.Revit"
#define RevitAddinFolder "{userappdata}\Autodesk\REVIT\Addins"
;#define RevitFolder21 RevitAddinFolder+"\2021\"+RevitAppName
#define RevitAddin21  RevitAddinFolder+"\2021\"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{a32902cb-bacb-48d3-a0f4-ce25faf0236c}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName="{#RevitAddin21}"
DisableDirPage=yes
DefaultGroupName=Hypar\{#MyAppName}
DisableProgramGroupPage=yes
;LicenseFile=.\LICENSE
OutputDir=.
OutputBaseFilename=Hypar-installer
;SetupIconFile=assets\logo.ico
;WizardImageFile=assets\banner.bmp
Compression=lzma
SolidCompression=yes
;PrivilegesRequired=lowest
UninstallFilesDir="{userappdata}\.hypar\uninstall"
;info: http://revolution.screenstepslive.com/s/revolution/m/10695/l/95041-signing-installers-you-create-with-inno-setup
;comment/edit the line below if you are not signing the exe with the CASE pfx
;SignTool=signtoolcase

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Components]
Name: revit21; Description: Hypar Revit addin for Autodesk Revit 2021;  Types: full

[Files]

;REVIT 2021 ~~~~~~~~~~~~~~~~~~~
Source: "..\deploy\*"; DestDir: "{#RevitAddin21}"; Excludes: "*.pdb,*.xml,*.config,*.addin,*.tmp"; Flags: ignoreversion recursesubdirs createallsubdirs; Components: revit21
Source: "..\deploy\{#RevitAppName}.addin"; DestDir: "{#RevitAddin21}"; Flags: ignoreversion; Components: revit21

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"