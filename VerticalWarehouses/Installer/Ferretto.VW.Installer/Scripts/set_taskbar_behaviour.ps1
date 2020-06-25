
$key_path = 'HKCU:SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3'
$val = (Get-ItemProperty -Path $key_path)."Settings"
$val[8]=3
Set-ItemProperty -Path $key_path -Name Settings -Value $val
Stop-Process -Name explorer
