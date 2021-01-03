Set-ItemProperty "$(PPC:Registry:Path)" -Name "$(PPC:Registry:Key)" -Value "$(Update:Temp:Path)\$(Installer:DirName)\$(Installer:FileName)";
