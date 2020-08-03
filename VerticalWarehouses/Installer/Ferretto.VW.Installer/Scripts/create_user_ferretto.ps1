$userName = "Ferretto"

New-LocalUser -Name $userName -NoPassword -UserMayNotChangePassword -AccountNeverExpires | Set-LocalUser -PasswordNeverExpires $true
Add-LocalGroupMember -Group "Administrators" -Member $userName
