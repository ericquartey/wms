$AppSettings = \"$(Backup:Root:Path)\\Staging\\$(MAS:DirName)\\appsettings.json\";
$primDbStageFile = \"$(Backup:Root:Path)\\staging\\primary.db\";
$secDbStageFile = \"$(Backup:Root:Path)\\staging\\secondary.db\";
$telemetryDbStageFolder = \"$(Backup:Root:Path)\\Staging\\Telemetry\"
$json_object = Get-Content -Raw -Path $AppSettings | ConvertFrom-Json;
$primDbName = $json_object.ConnectionStrings.AutomationServicePrimary;
$secDbName = $json_object.ConnectionStrings.AutomationServiceSecondary;
$primFileDbName = $primDbName.Replace(\"Data Source='\", \"\").TrimEnd(\"'\");
$secFileDbName = $secDbName.Replace(\"Data Source='\", \"\").TrimEnd(\"'\");
$primDbFilePath =  \"E:\\database\\\" +  ([io.fileinfo]$primFileDbName).Name;
$secDbFileName = \"F:\\database\\\" + ([io.fileinfo]$secFileDbName).Name;
Move-Item -Path $primDbStageFile -Destination $primDbFilePath -Force;
Move-Item -Path $secDbStageFile -Destination $secDbFileName -Force;
Move-Item -Path $telemetryDbStageFolder -Destination \"E:\\database\\Telemetry\" -Force
