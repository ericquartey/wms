Copy-Item "$(Update:Temp:Path)\$(Installer:DirName)" "$(Install:Root:Path)" -recurse -Exclude "steps-snapshot.json"
