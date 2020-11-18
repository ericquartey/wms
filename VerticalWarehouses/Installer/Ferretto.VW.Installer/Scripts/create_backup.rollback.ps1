$stageAs="$(Backup:Root:Path)\Staging\$(MAS:DirName)";
$stagePpc="$(Backup:Root:Path)\Staging\$(PPC:DirName)";
$stageInstaller="$(Backup:Root:Path)\Staging\$(Installer:DirName)";
$stageTelemetry="$(Backup:Root:Path)\Staging\$(TS:DirName)";

$as="$(Install:Root:Path)\$(MAS:DirName)";
$ppc="$(Install:Root:Path)\$(PPC:DirName)";
$installer="$(Install:Root:Path)\$(Installer:DirName)";
$telemetry="$(Install:Root:Path)\$(TS:DirName)";


Copy-Item $stageAs $as -Recurse -Exclude (Get-ChildItem $as -Recurse);
Copy-Item $stagePpc $ppc -Recurse -Exclude (Get-ChildItem $ppc -Recurse);
Copy-Item $stageInstaller $installer -Recurse -Exclude (Get-ChildItem $installer -Recurse);
Copy-Item $stageTelemetry $telemetry -Recurse -Exclude (Get-ChildItem $telemetry -Recurse);

Remove-Item $stageAs -Force -Recurse;
Remove-Item $stagePpc -Force -Recurse;
Remove-Item $stageInstaller -Force -Recurse;
Remove-Item $stageTelemetry -Force -Recurse

