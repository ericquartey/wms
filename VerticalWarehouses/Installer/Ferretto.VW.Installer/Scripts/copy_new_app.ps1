Copy-Item "$(Update:Temp:Path)\$(MAS:DirName)" "$(Install:Root:Path)" -recurse
Copy-Item "$(Update:Temp:Path)\$(PPC:DirName)" "$(Install:Root:Path)" -recurse
Copy-Item "$(Update:Temp:Path)\$(Installer:DirName)" "$(Install:Root:Path)" -recurse -Exclude "steps-snapshot.json"
