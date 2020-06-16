schtasks.exe /Create /SC ONLOGON /TN "$(Install:TaskScheduler:Installer)" /TR "$(Update:Temp:Path)\$(Installer:DirName)\$(Installer:FileName)" /RL HIGHEST /F
