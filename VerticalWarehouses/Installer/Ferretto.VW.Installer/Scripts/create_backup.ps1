$stageAs="$(Backup:Root:Path)\Staging\$(MAS:DirName)";
$stagePpc="$(Backup:Root:Path)\Staging\$(PPC:DirName)";
$stageInstaller="$(Backup:Root:Path)\Staging\$(Installer:DirName)";
$stageTelemetry="$(Backup:Root:Path)\Staging\$(TS:DirName)";
$stageDatabase="$(Backup:Root:Path)\Staging\Database"

$as="$(Install:Root:Path)\$(MAS:DirName)";
$ppc="$(Install:Root:Path)\$(PPC:DirName)";
$installer="$(Install:Root:Path)\$(Installer:DirName)";
$telemetry="$(Install:Root:Path)\$(TS:DirName)";

# remove dirty staging dirs, if any
#if(Test-Path $stageAs -PathType Container) {Remove-Item $stageAs -Recurse -Force};
#if(Test-Path $stagePpc -PathType Container) {Remove-Item $stagePpc -Recurse -Force};
#if(Test-Path $stageInstaller -PathType Container) {Remove-Item $stageInstaller -Recurse -Force};
#if(Test-Path $stageTelemetry -PathType Container) {Remove-Item $stageTelemetry -Recurse -Force};
#if(Test-Path $stageDatabase -PathType Container) {Remove-Item $stageDatabase -Recurse -Force};

# copy current app in staging dirs
Copy-Item $as $stageAs -Recurse;
Copy-Item $ppc $stagePpc -Recurse;
Copy-Item $installer $stageInstaller -Recurse;
Copy-Item $telemetry $stageTelemetry -Recurse;

Copy-Item "E:\Database" $stageDatabase -Recurse;
