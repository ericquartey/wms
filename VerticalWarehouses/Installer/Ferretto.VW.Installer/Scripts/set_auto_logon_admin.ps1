$userName = "Administrator"

$winLogonPath = "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon"

Set-ItemProperty $winLogonPath -Name DefaultUserName -Value $userName;
New-ItemProperty -Path $winLogonPath -Name "DefaultPassword" -Value "password" -Force
