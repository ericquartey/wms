Start-Process msiexec.exe -Wait -ArgumentList '/I "$(Update:Temp:Path)\$(Installer:DirName)\Drivers\ufcom-1.7.12.msi" /quiet'
