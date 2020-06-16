Set-Location "C:\Users\Alessandro\source\repos\ferretto\VW\VerticalWarehouses\Pipelines"

Remove-Item "staging\*" -Recurse
Remove-Item "sfx_options.txt"

$buildVersion = Get-Date -Format "yyyy_MM_dd_HH_mm"
$configuration = "Release"
$runtime = "win10-x64"

Write-Host "--------------------"
Write-Host "Building PPC App ..."
Invoke-MsBuild "../PanelPC.sln" -MsBuildParameters "/target:Build /property:Configuration=Release;Platform=x64;BuildInParallel=true /maxcpucount"
Copy-Item -Path "../Panel PC UI/Ferretto.VW.App/bin/release/net471" -Destination "staging/panelpc_app"  -Recurse

Write-Host "-------------------------------"
Write-Host "Building Automation Service ..."
dotnet publish "../Machine Automation Service/Ferretto.VW.MAS.AutomationService/Ferretto.VW.MAS.AutomationService.csproj" --verbosity quiet --framework netcoreapp2.2 --runtime $runtime  --configuration $configuration
Copy-Item -Path "../Machine Automation Service/Ferretto.VW.MAS.AutomationService/bin/release/netcoreapp2.2/win10-x64/publish" -Destination "staging/automation_service" -Recurse

Write-Host "----------------------"
Write-Host "Building Installer ..."
dotnet publish  "../Installer/Ferretto.VW.Installer/Ferretto.VW.Installer.csproj" --framework netcoreapp3.1 --verbosity quiet --runtime $runtime  --configuration $configuration
Copy-Item -Path "../Installer/Ferretto.VW.Installer/bin/release/netcoreapp3.1/win10-x64/publish" -Destination "staging/installer" -Recurse


cd staging

Write-Host "--------------------------------------"
Write-Host "Cleanup MAS configuration ..."

cd "automation_service"
Get-Location #pipelines/staging/automation_service

del appsettings.*.json
del configuration\seeds\*.*
del configuration\*.json

cd ..


Write-Host "--------------------------------------"
Write-Host "Update MAS Nlog Config ..."
cd "automation_service"
Get-Location #pipelines/staging/automation_service

$nlogConfigPath = Resolve-Path "./nlog.config"

Write-Host "Updating NLog file " $nlogConfigPath

[xml] $nlogConfigXml = Get-Content $nlogConfigPath

$logDirectory = $nlogConfigXml.nlog.variable | Where-Object {$_.name -like "logDirectory"}
$archiveDirectory = $nlogConfigXml.nlog.variable | Where-Object {$_.name -like "archiveDirectory"}

$logDirectory.value = "F:/Logs/"
$archiveDirectory.value = "F:/Logs/Archive"

$nlogConfigXml.Save($nlogConfigPath);

cd ..

Write-Host "--------------------------------------"
Write-Host "Update PPC Nlog Config ..."

Set-Location "panelpc_app"
Get-Location #pipelines/staging/panelpc_app

$nlogConfigPath = Resolve-Path "./nlog.config"

Write-Host "Updating NLog file " $nlogConfigPath

[xml] $nlogConfigXml = Get-Content $nlogConfigPath

$logDirectory = $nlogConfigXml.nlog.variable | Where-Object {$_.name -like "logDirectory"}
$archiveDirectory = $nlogConfigXml.nlog.variable | Where-Object {$_.name -like "archiveDirectory"}

$logDirectory.value = "F:/Logs/"
$archiveDirectory.value = "F:/Logs/Archive"

$nlogConfigXml.Save($nlogConfigPath);

cd ..

Write-Host "--------------------------------------"
Write-Host "Update manifest files version ..."
Get-Location #pipelines/staging

$files = Get-ChildItem -Recurse -File app.manifest

foreach($file in $files){
    Write-Host "Updating manifest file " $file
    $xml = [xml](Get-Content $file)
    $xml.assembly.assemblyIdentity.version = $buildVersion
    $xml.save($file)
}


Write-Host "--------------------------------------"
Write-Host "Update md5 checksums ..."
Get-Location #pipelines/staging

dir -File -Recurse | Get-FileHash -Algorithm MD5 | Select {Resolve-Path -Relative $_.Path},Hash | Export-Csv -Path "..\md5-checksums.csv" -NoTypeInformation

copy "..\md5-checksums.csv" .


Write-Host "--------------------------------------"
Write-Host "Create SFX ..."
pwd #pipelines/staging

$sfxOptionsPath = "..\sfx_options.txt"

$sfxOptions = "Setup=F:\Update\Temp\installer\Ferretto.VW.Installer.exe" + "`r`n" + "Path=F:\Update\Temp" + "`r`n" + "Silent=1"

$sfxOptions | Add-Content -Path $sfxOptionsPath

$sourceFolder ="."
$outputArchive = "..\VMag_Setup_" + $buildVersion + ".exe"

Write-Host "Compressing folder " $sourceFolder " into file " $outputArchive

..\..\..\pipelines\winrar\winrar.exe a -afzip -cfg- -ed -ep1 -k -m5 -r -tl "-sfxZip64.sfx" "-z..\sfx_options.txt" $outputArchive

Write-Host "Done."
