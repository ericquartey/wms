$driverFullPath = "$(Update:Temp:Path)\$(Installer:DirName)\Drivers\TeamViewer\ext";
if(Test-Path $driverFullPath -PathType Container) {Remove-Item $driverFullPath -Recurse -Force};

$driverFullPath = "$(Update:Temp:Path)\$(Installer:DirName)\Drivers\TeamViewer";
$contents = Get-ChildItem -Path $driverFullPath | sort | Select-Object -First 1;
$driverFileName = $contents.Name;
if ($driverFileName -ne $null)
{
    Add-Type -assembly "system.io.compression.filesystem"
    [io.compression.zipfile]::ExtractToDirectory("$driverFullPath\$driverFileName", "$driverFullPath\ext\")
    Copy-Item "$driverFullPath\ext\*" "F:\" -recurse
}

