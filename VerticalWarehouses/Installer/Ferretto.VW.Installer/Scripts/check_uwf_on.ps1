$NAMESPACE = "root\standardcimv2\embedded"
$uwfInstance = Get-CimInstance -className "UWF_Filter" -Namespace $NAMESPACE

if(!$uwfInstance)
{
    throw "UWF is NOT installed."
}

if(!$uwfInstance.CurrentEnabled)
{
    throw "UWF is disabled."
}
