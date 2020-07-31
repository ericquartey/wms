# UWF disabled, logged as Ferretto
$userName = "Ferretto"

Remove-LocalGroupMember -Group "Administrators" -Member $userName
