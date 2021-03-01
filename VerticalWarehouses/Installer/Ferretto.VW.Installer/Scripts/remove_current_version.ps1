
$as="$(Install:Root:Path)\$(MAS:DirName)";
$ppc="$(Install:Root:Path)\$(PPC:DirName)";
$installer="$(Install:Root:Path)\$(Installer:DirName)";
$telemetry="$(Install:Root:Path)\$(TS:DirName)";

# remove current app dirs
Remove-Item $as -Force -Recurse;
Remove-Item $ppc -Force -Recurse;
Remove-Item $installer -Force -Recurse
Remove-Item $telemetry -Force -Recurse

if(Test-Path "E:\Database\Telemetry" -PathType Container) {Remove-Item "E:\Database\Telemetry\*.*" -Force -Recurse};
