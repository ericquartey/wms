﻿$userName = "Ferretto"

$winLogonPath = "HKCU:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon"

New-ItemProperty -Path $winLogonPath -Name "Shell" -Value "$(Install:Root:Path)\$(PPC:DirName)\$(PPC:FileName)" -Force

#Set-ItemProperty -Path $winLogonPath -Name "Shell" -Value "explorer.exe" -Force

#hide taskbar
$key_path = 'HKCU:SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3'
$val = (Get-ItemProperty -Path $key_path)."Settings"
$val[8]=3
Set-ItemProperty -Path $key_path -Name Settings -Value $val
