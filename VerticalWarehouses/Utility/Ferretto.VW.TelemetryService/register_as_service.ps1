$serviceName = "Ferretto_VW_TS"
$serviceDisplayName = "Ferretto Vertimag Telemetry Service"

$servicePort = 5050
$serviceUri = "http://localhost:" + $servicePort + "/health/live"
$totalHealthRetries = 20

$relativeBinaryPath = "Ferretto.VW.TelemetryService.exe"

Clear-Host

if(-not(Test-Path $relativeBinaryPath)) {
    throw "Cannot register the service. The file '" + $relativeBinaryPath + "' does not exist in the current directory."
}

$existingService = Get-Service $serviceName -ErrorAction SilentlyContinue
if(-not($null -eq $existingService)) {
    Write-Host "Servizio già registrato. Arresto del servizio ..."

    Stop-Service $serviceName
    Write-Host "Eliminazione del servizio ..."

    sc.exe delete $serviceName

    if($LASTEXITCODE -lt 0)
    {
        throw "Errore durante l'eliminazione del servizio."
    }
}

while(-not($null -eq (Get-Service $serviceName -ErrorAction SilentlyContinue)))
{
    Write-Host "Attesa eliminazione del servizio ..."

    Start-Sleep 1
}

$absoluteBinaryPath = Resolve-Path $relativeBinaryPath
$absoluteBinaryPath = $absoluteBinaryPath.Path + " --service"
Write-Host "Creazione nuovo servizio ..."

New-Service -BinaryPathName $absoluteBinaryPath -Name $serviceName -DisplayName $serviceDisplayName -StartupType Automatic

try {
    Write-Host "Avvio nuovo servizio ..."
    Start-Service $serviceName 
}
catch {
    throw "Impossibile avviare il servizio"
}

Write-Host "Attendo che il servizio sia pronto ..."

$isServiceHealthy = $false
$retriesDone = 0
do {
    try {
        $response = Invoke-WebRequest -UseBasicParsing -URI $serviceUri -ErrorAction SilentlyContinue 
        $isServiceHealthy = $response.StatusCode -eq 200 -and $response.Content -like "healthy"
        Write-Host "Tentativo" $retriesDone": il servizio è" $response.Content
        
    }
    catch {
        Write-Host "Il servizio non è pronto ..."
    }
   
    Start-Sleep 1
    
    $retriesDone = $retriesDone + 1

} while (-not($isServiceHealthy) -and $retriesDone -le $totalHealthRetries)

if($isServiceHealthy) {
    Write-Host "Il servizio è pronto."
}
else {
    throw "Il servizio non è pronto."
}

