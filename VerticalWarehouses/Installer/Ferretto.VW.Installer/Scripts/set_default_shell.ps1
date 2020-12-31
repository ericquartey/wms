$userName = "Ferretto"

$winLogonPath = "HKCU:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon"

#New-ItemProperty -Path $winLogonPath -Name "Shell" -Value "$(Install:Root:Path)\$(PPC:DirName)\$(PPC:FileName)" -Force

Set-ItemProperty -Path $winLogonPath -Name "Shell" -Value "explorer.exe" -Force

