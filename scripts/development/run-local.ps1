try {
    # Configuration
    $rootPath = (Get-Item $PSScriptRoot).Parent.Parent.FullName
    $srcPath = Join-Path $rootPath "src"
    $devSettings = Get-Content -Path (Join-Path $srcPath "appsettings.Development.json") | ConvertFrom-Json
    
    # Vérification des prérequis
    Write-Host "`nVérification des prérequis..." -ForegroundColor Green
    
    # Vérifier le SDK .NET
    $needsDotnet = $false
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        $needsDotnet = $true
    } else {
        $hasNet8 = dotnet --list-sdks | Where-Object { $_ -like "8.0.*" }
        if (-not $hasNet8) {
            $needsDotnet = $true
        }
    }

    if ($needsDotnet) {
        Write-Host "Installation du SDK .NET 8.0..." -ForegroundColor Yellow
        $dotnetInstallUrl = "https://dot.net/v1/dotnet-install.ps1"
        $dotnetInstallScript = "$env:TEMP\dotnet-install.ps1"
        
        try {
            Invoke-WebRequest -Uri $dotnetInstallUrl -OutFile $dotnetInstallScript
            & $dotnetInstallScript -Channel 8.0 -InstallDir "$env:ProgramFiles\dotnet"
            $env:Path = "$env:ProgramFiles\dotnet;$env:Path"
        }
        catch {
            throw "Échec de l'installation du SDK .NET 8.0. Veuillez l'installer manuellement: https://dot.net/download"
        }
        finally {
            Remove-Item $dotnetInstallScript -ErrorAction SilentlyContinue
        }
    } else {
        Write-Host "SDK .NET 8.0 déjà installé" -ForegroundColor Green
    }

    Write-Host "`nDémarrage de l'environnement de développement..." -ForegroundColor Cyan
    
    # Vérification de la disponibilité du port
    $port = $devSettings.ApiSettings.Port
    $portInUse = Get-NetTCPConnection -State Listen -LocalPort $port -ErrorAction SilentlyContinue
    if ($portInUse) {
        throw "Le port $port est déjà utilisé par un autre processus"
    }
    
    Write-Host "Port: $port" -ForegroundColor Gray
    Write-Host "Base de données: $($devSettings.ConnectionStrings.DefaultConnection)" -ForegroundColor Gray
    Write-Host "Swagger: http://localhost:$port/swagger" -ForegroundColor Gray

    # Vérification et préparation des répertoires
    $directories = @(
        (Join-Path $srcPath "data"),
        (Join-Path $srcPath "logs")
    )
    
    $needsDirectories = $false
    foreach ($dir in $directories) {
        if (-not (Test-Path $dir)) {
            $needsDirectories = $true
            break
        }
    }

    if ($needsDirectories) {
        Write-Host "`nCréation des répertoires manquants..." -ForegroundColor Yellow
        foreach ($dir in $directories) {
            if (-not (Test-Path $dir)) { 
                New-Item -ItemType Directory -Path $dir -Force | Out-Null
                Write-Host "Création du répertoire: $dir" -ForegroundColor Yellow
            }
        }
    } else {
        Write-Host "`nTous les répertoires sont déjà créés" -ForegroundColor Green
    }

    # Build et tests
    Push-Location $rootPath
    Write-Host "`nPréparation du projet..." -ForegroundColor Green
    
    Write-Host "Nettoyage..." -ForegroundColor Yellow
    dotnet clean $srcPath --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Échec du nettoyage" }

    Write-Host "Restauration des packages..." -ForegroundColor Yellow
    dotnet restore $srcPath --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Échec de la restauration" }

    Write-Host "Compilation..." -ForegroundColor Yellow
    dotnet build $srcPath --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Échec de la compilation" }

    Write-Host "`nExécution des tests..." -ForegroundColor Green
    dotnet test (Join-Path $rootPath "tests") --no-build --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Échec des tests" }

    # Démarrage de l'application
    Write-Host "`nDémarrage de l'application..." -ForegroundColor Green
    Write-Host "Hot reload activé - Ctrl+C pour arrêter`n" -ForegroundColor Yellow
    dotnet watch run --project $srcPath --no-hot-reload
}
catch {
    Write-Host "`nErreur: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Solutions possibles:" -ForegroundColor Yellow
    Write-Host "1. Vérifiez que le SDK .NET 8.0 est installé" -ForegroundColor Yellow
    Write-Host "2. Vérifiez que tous les ports requis sont disponibles" -ForegroundColor Yellow
    Write-Host "3. Supprimez les répertoires bin et obj, puis réessayez" -ForegroundColor Yellow
    pause; exit 1
}
finally {
    Pop-Location
}