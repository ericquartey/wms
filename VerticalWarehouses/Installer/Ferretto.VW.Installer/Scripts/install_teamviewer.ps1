$driverFullPath = "$(Update:Temp:Path)\$(Installer:DirName)\Drivers\TeamViewer";
$contents = Get-ChildItem -Path $driverFullPath | sort | Select-Object -First 1;
$driverFileName = $contents.Name;
if ($driverFileName -ne $null)
{
    Expand-Archive -Path $driverFileName -DestinationPath "F:\";
}

