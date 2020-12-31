Set-ItemProperty "$(PPC:Registry:Path)" -Name "$(PPC:Registry:Key)" -Value "$(Update:Temp:Path)\$(Installer:DirName)\$(Installer:FileName)";

$winLogonPath = "HKCU:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon"
Set-ItemProperty -Path $winLogonPath -Name "Shell" -Value "explorer.exe" -Force
