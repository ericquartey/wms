$nlogConfigPath = "nlog.config"

[xml] $nlogConfigXml = Get-Content $nlogConfigPath

$logDirectory = $nlogConfigXml.nlog.variable | Where-Object {$_.name -like "logDirectory"}
$archiveDirectory = $nlogConfigXml.nlog.variable | Where-Object {$_.name -like "archiveDirectory"}

$logDirectory.value = "F:/Logs/"
$archiveDirectory.value = "F:/Logs/Archive"

$nlogConfigXml.Save($nlogConfigPath);
