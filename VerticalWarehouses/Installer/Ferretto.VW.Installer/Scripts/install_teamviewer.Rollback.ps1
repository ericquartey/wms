$driverFullPath = "$(Update:Temp:Path)\$(Installer:DirName)\Drivers\TeamViewer\ext";
if(Test-Path $driverFullPath -PathType Container) {Remove-Item $driverFullPath -Recurse -Force};

