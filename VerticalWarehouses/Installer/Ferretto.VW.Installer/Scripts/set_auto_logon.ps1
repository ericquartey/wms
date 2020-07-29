$userName = "Ferretto"

New-LocalUser -Name $userName -NoPassword -UserMayNotChangePassword -AccountNeverExpires | Set-LocalUser -PasswordNeverExpires $true

$winLogonPath = "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon"

Set-ItemProperty $winLogonPath -Name DefaultUserName -Value $userName;
New-ItemProperty -Path $winLogonPath -Name DefaultPassword -Force
