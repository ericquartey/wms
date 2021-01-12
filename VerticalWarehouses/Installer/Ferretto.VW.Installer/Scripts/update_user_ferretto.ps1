$userName = "Ferretto"
$groupName = "Administrators"

$existingMember = Get-LocalGroupMember -Name $groupName | Where {$_.Name -like "*$userName"}

if ($existingMember)
{
    Write-Host "'$userName' is already a member of '$groupName'"
}
else
{
    Add-LocalGroupMember -Group $groupName -Member $userName
}
