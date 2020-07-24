$basePath = $(Update:Temp:Path)\$(Installer:DirName) + "\Drivers";
$driverFullPath = $basePath + "\TokenReaderEuchner";
$contents = Get-ChildItem -Path $driverFullPath | sort | Select-Object -First 1;
$driverFileName = $contents.Name;
if ($driverFileName -ne $null)
{
    $arguments = "'/I ""$driverFullPath\$driverFileName"" /quiet'";
    Start-Process msiexec.exe -Wait -ArgumentList $arguments;
}

