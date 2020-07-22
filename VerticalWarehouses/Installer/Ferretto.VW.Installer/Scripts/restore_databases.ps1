$AppSettings = \"$(Backup:Root:Path)\\Staging\\$(MAS:DirName)\\appsettings.json\";
$primDbStageFile = \"$(Backup:Root:Path)\\staging\\primary.db\";
$secDbStageFile = \"$(Backup:Root:Path)\\staging\\secondary.db\";
$telemetryFristDbStageFile = \"$(Backup:Root:Path)\\staging\\telemetry.real\";
$telemetrySecondDbStageFile = \"$(Backup:Root:Path)\\staging\\telemetry.realm.lock\";
$json_object = Get-Content -Raw -Path $AppSettings | ConvertFrom-Json;
$primDbName = $json_object.ConnectionStrings.AutomationServicePrimary;
$secDbName = $json_object.ConnectionStrings.AutomationServiceSecondary;
$primFileDbName = $primDbName.Replace(\"Data Source='\", \"\").TrimEnd(\"'\");
$secFileDbName = $secDbName.Replace(\"Data Source='\", \"\").TrimEnd(\"'\");
$primDbFilePath =  \"E:\\database\\\" +  ([io.fileinfo]$primFileDbName).Name;
$secDbFileName = \"F:\\database\\\" + ([io.fileinfo]$secFileDbName).Name;
$telemetryFirstDbFileName = \"F:\\database\\\" + ([io.fileinfo]telemetry.real).Name;
$telemetrySecondDbFileName = \"F:\\database\\\" + ([io.fileinfo]telemetry.realm.lock).Name;
Move-Item -Path $primDbStageFile -Destination $primDbFilePath -Force;
Move-Item -Path $secDbStageFile -Destination $secDbFileName -Force;
Move-Item -Path $telemetryFristDbStageFile -Destination $telemetryFirstDbFileName -Force;
Move-Item -Path $telemetrySecondDbStageFile -Destination $telemetrySecondDbFileName -Force
