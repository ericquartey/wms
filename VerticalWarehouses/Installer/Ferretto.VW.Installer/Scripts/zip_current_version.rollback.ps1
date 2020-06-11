if (Test-Path $(Backup:Root:Path)\\Current_Version_Backup.zip -PathType Leaf) {
    Remove-Item $(Backup:Root:Path)\\Current_Version_Backup.zip
}
