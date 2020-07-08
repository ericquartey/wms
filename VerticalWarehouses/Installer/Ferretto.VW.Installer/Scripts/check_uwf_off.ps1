$outUwf = uwfmgr.exe filter get-config
$outUwf2 = ($outUwf -replace "\0", '') | Select-String -Pattern "FILTER STATE(.*)"

$uwfCurrentEnabled = $outUwf2[0] -match "ON"

if($uwfCurrentEnabled)
{
    Write-Host  "UWF is enabled and it will be disabled before continuing installation."
    uwfmgr.exe filter disable
    Restart-Computer -Force
    Start-Sleep -Seconds 90
}
else
{
    Write-Host "UWF is already disabled. No action needed."
}
