$files = Get-ChildItem -Recurse -File app.manifest

foreach($file in $files){
    Write-Host "Updating manifest file " $file
    $xml = [xml](Get-Content $file)
    $xml.assembly.assemblyIdentity.version = $env:RELEASE_VERSION
    $xml.save($file)
}