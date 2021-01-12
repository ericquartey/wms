Set-ItemProperty "$(PPC:Registry:Path)" -Name "$(PPC:Registry:Key)" -Value "$(Install:Root:Path)\$(PPC:DirName)\$(PPC:FileName)";
