#remove autostart installer - used if ppc app will be launched as shell
Remove-ItemProperty -Path $(PPC:Registry:Path) -Name $(PPC:Registry:Key)

#set ppc app autostart - used if shell is explorer
#Set-ItemProperty "$(PPC:Registry:Path)" -Name "$(PPC:Registry:Key)" -Value "$(Install:Root:Path)\$(PPC:DirName)\$(PPC:FileName)";
