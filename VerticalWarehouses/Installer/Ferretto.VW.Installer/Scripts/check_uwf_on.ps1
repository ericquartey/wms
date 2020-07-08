$outUwf = uwfmgr.exe filter get-config
$outUwf2 = ($outUwf -replace "\0", '') | Select-String -Pattern "FILTER STATE(.*)"

$uwfCurrentEnabled = $outUwf2[0] -match "ON"

if(!$uwfCurrentEnabled)
{
    throw "UWF is disabled, but should have been enabled."
}
