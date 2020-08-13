$newDriverName = "BaronPesiWeightScale.exe";
$driverFullPath = "$(Update:Temp:Path)\$(Installer:DirName)\Drivers\ScaleBaronPesi";
$newDriverFullPathName = "$driverFullPath\$newDriverName"
if(Test-Path $newDriverFullPathName -PathType Leaf)
{
    Remove-Item $newDriverFullPathName -Force;
}
$contents = Get-ChildItem -Path $driverFullPath | sort | Select-Object -First 1;
$driverFileName = $contents.Name;
if ($driverFileName -ne $null)
{
    currentDriverName = ""$driverFullPath\$driverFileName"";
    Copy-Item $currentDriverName $newDriverFullPathName;
    Start-Process ""$newDriverFullPathName"";
}

