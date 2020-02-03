Write-Host "Creating  SFX options file ..."

mkdir ".\staging\"

$sfxOptionsPath = ".\sfx_options.txt"

$sfxOptions = "Setup=E:\updates\installer\Ferretto.VW.Installer.exe" + "`r`n" + "Path=E:\updates" + "`r`n" + "Silent=1"

$sfxOptions | Add-Content -Path $sfxOptionsPath

$sourceFolder =".\binaries\"
$outputArchive = ".\staging\VMag_Setup_" + $env:BUILD_NUMBER + ".exe"

Write-Host "Compressing folder "  $sourceFolder " into file " $outputArchive

.\storage\winrar\winrar.exe a -afzip -cfg- -ed -ep1 -k -m5 -r -tl "-sfxZip64.sfx" "-zsfx_options.txt" $outputArchive $sourceFolder 
Write-Host "Done."

