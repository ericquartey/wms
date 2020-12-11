$driverFullPath = "$(Update:Temp:Path)\$(Installer:DirName)\Drivers\ScaleBaronPesi";
Get-ChildItem -Path "$driverFullPath\*.exe.zip" | Rename-Item -NewName { $_.Name -replace '.exe.zip','.exe' };
$contents = Get-ChildItem -Path $driverFullPath | sort | Select-Object -First 1;
$driverFileName = $contents.Name;
#if ($driverFileName -ne $null)
#{
#    $arguments = "/SILENT /SUPPRESSMSGBOXES /NORESTART";
#    Start-Process -FilePath "$driverFullPath\$driverFileName" -Wait -ArgumentList $arguments;
#}

