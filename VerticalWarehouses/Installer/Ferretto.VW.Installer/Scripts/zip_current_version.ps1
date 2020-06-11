if(Test-Path $(Backup:Root:Path)\Current_Version_Backup.zip -PathType Leaf) {
    del $(Backup:Root:Path)\Current_Version_Backup.zip
}

Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::CreateFromDirectory("$(Backup:Root:Path)\staging", "$(Backup:Root:Path)\Current_Version_Backup.zip", [System.IO.Compression.CompressionLevel]::Optimal, "")
