# Nécessite Docker Desktop installé et les droits pour l'exécuter

# Vérifier si Docker Desktop est installé
if (-not (Test-Path "C:\Program Files\Docker\Docker\Docker Desktop.exe")) {
    Write-Host "Docker Desktop n'est pas installé" -ForegroundColor Red
    pause
    exit 1
}

try {
    # Chemins d'accès
    $rootPath = (Get-Item $PSScriptRoot).Parent.Parent.FullName

    # 1. Vérifier Docker
    Write-Host "Vérification de Docker..." -ForegroundColor Yellow
    if (-not (Get-Process "Docker Desktop" -ErrorAction SilentlyContinue)) {
        Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
        Write-Host "Attente du démarrage de Docker..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
    }

    # 2. Configuration
    Write-Host "`nMise à jour de la configuration..." -ForegroundColor Yellow
    $appSettings = Get-Content -Path "$rootPath/src/appsettings.Production.json" -Raw | ConvertFrom-Json
    $appSettings.ConnectionStrings.DefaultConnection = "Data Source=/app/data/ProductMicroservice.db"
    $appSettings | ConvertTo-Json -Depth 10 | Set-Content -Path "$rootPath/src/appsettings.Production.json"

    # 3. Déploiement Docker
    Write-Host "`nDéploiement avec Docker Compose..." -ForegroundColor Green
    docker-compose -f "$rootPath/docker-compose.yml" down
    docker-compose -f "$rootPath/docker-compose.yml" up --build -d

    # 4. Vérification
    Start-Sleep -Seconds 3
    $url = "http://localhost:41528/api/Product"
    Write-Host "`nApplication déployée sur: $url" -ForegroundColor Green
    Start-Process $url

    Write-Host "`nAppuyez sur Entrée pour arrêter le conteneur..." -ForegroundColor Yellow
    $null = Read-Host

} catch {
    Write-Host "Erreur: $($_.Exception.Message)" -ForegroundColor Red
} finally {
    Write-Host "`nArrêt des conteneurs..." -ForegroundColor Yellow
    docker-compose -f "$rootPath/docker-compose.yml" down
} 