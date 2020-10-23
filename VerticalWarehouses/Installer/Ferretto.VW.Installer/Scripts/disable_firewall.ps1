NetSh Advfirewall set allprofiles state off

Set-MpPreference -DisableIOAVProtection $true -DisableScriptScanning $true -DisableRemovableDriveScanning $false -MAPSReporting Disabled -SubmitSamplesConsent NeverSend

Add-MpPreference -ExclusionPath D:\, E:\, F:\


Set-ItemProperty -Path "HKLM:\Software\Policies\Microsoft\Windows Defender" -Name "DisableRoutinelyTakingAction" -Value 1

