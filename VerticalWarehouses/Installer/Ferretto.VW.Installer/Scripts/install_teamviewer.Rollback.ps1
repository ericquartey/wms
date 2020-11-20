$driverFullPath = "$(Update:Temp:Path)\$(Installer:DirName)\Drivers\TeamViewer\ext";
$contents = Get-ChildItem -Path $driverFullPath | sort | Select-Object -First 1;
$driverFileName = $contents.Name;
if ($driverFileName -ne $null)
{
    Remove-Item $driverFullPath -recurse

    if(Test-Path "F:\$driverFileName" -PathType Leaf) {Remove-Item "F:\$driverFileName" -Recurse -Force};

}

