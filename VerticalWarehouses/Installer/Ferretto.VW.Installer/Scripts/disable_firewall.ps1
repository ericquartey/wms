Set-NetFirewallProfile -Profile Domain,Public,Private -Enabled False

Set-ItemProperty -Path "HKLM:\Software\Policies\Microsoft\Windows Defender" -Name "DisableRoutinelyTakingAction" -Value 1
Set-ItemProperty -Path "HKLM:\Software\Policies\Microsoft\Windows Defender" -Name "DisableAntiSpyware" -Value 1
if(!(Test-Path "HKLM:\Software\Policies\Microsoft\Windows Defender\Real-Time Protection"))
{
    New-Item -Path "HKLM:\Software\Policies\Microsoft\Windows Defender\Real-Time Protection"
    Set-ItemProperty -Path "HKLM:\Software\Policies\Microsoft\Windows Defender\Real-Time Protection" -Name "DisableBehaviorMonitoring" -Value 1
    Set-ItemProperty -Path "HKLM:\Software\Policies\Microsoft\Windows Defender\Real-Time Protection" -Name "DisableOnAccessProtection" -Value 1
    Set-ItemProperty -Path "HKLM:\Software\Policies\Microsoft\Windows Defender\Real-Time Protection" -Name "DisableScanOnRealtimeEnable" -Value 1
}

Set-ItemProperty -Path "HKLM:\System\CurrentControlSet\Control\Session Manager" -Name "ProtectionMode" -Value 0

#enable remote desktop with blank password
Set-ItemProperty -Path "HKLM:\System\CurrentControlSet\Control\Lsa" -Name "LimitBlankPasswordUse" -Value 0
