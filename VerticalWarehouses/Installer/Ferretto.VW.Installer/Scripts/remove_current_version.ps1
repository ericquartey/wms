
$as="$(Install:Root:Path)\$(MAS:DirName)";
$ppc="$(Install:Root:Path)\$(PPC:DirName)";
$installer="$(Install:Root:Path)\$(Installer:DirName)";
$telemetry="$(Install:Root:Path)\$(TS:DirName)";

# remove current app dirs
if(Test-Path $as -PathType Container) {Remove-Item $as -Force -Recurse};
if(Test-Path $ppc -PathType Container) {Remove-Item $ppc -Force -Recurse};
if(Test-Path $installer -PathType Container) {Remove-Item $installer -Force -Recurse};
if(Test-Path $telemetry -PathType Container) {Remove-Item $telemetry -Force -Recurse};

if(Test-Path "E:\Database\Telemetry" -PathType Container) {Remove-Item "E:\Database\Telemetry\*.*" -Force -Recurse};
