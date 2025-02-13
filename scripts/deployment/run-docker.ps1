# Nécessite Docker Desktop installé et les droits pour l'exécuter

# Vérifier les droits administrateur
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Ce script doit être exécuté en tant qu'Administrateur" -ForegroundColor Red
    pause; exit 1
}

try {
    # Configuration
    $rootPath = (Get-Item $PSScriptRoot).Parent.Parent.FullName
    $srcPath = Join-Path $rootPath "src"
    $prodSettings = Get-Content -Path (Join-Path $srcPath "appsettings.Production.json") | ConvertFrom-Json
    
    # Validation des paramètres
    if (-not $prodSettings.DockerSettings -or -not $prodSettings.ApiSettings) {
        throw "Configuration Docker ou API manquante dans appsettings.Production.json"
    }

    Write-Host "`nVérification de l'environnement Docker..." -ForegroundColor Cyan
    $needsAction = $false
    $needsReboot = $false

    # 1. Vérifier la version de Windows
    Write-Host "`nVérification de Windows..." -ForegroundColor Green
    $osVersion = [System.Environment]::OSVersion.Version
    if ($osVersion.Major -lt 10 -or ($osVersion.Major -eq 10 -and $osVersion.Build -lt 19041)) {
        Write-Host "Windows 10 version 2004 (build 19041) ou supérieure est requise" -ForegroundColor Red
        Write-Host "Votre version: Windows $($osVersion.Major) build $($osVersion.Build)" -ForegroundColor Yellow
        throw "Version de Windows non compatible"
    }
    Write-Host "Version de Windows compatible" -ForegroundColor Green

    # 2. Vérifier la virtualisation
    Write-Host "`nVérification de la virtualisation..." -ForegroundColor Green
    $virtualization = systeminfo | findstr "Virtualization"
    if ($virtualization -match "Not enabled") {
        Write-Host "La virtualisation n'est pas activée dans le BIOS" -ForegroundColor Red
        throw "La virtualisation doit être activée dans le BIOS"
    }
    Write-Host "Virtualisation activée" -ForegroundColor Green

    # 3. Vérifier les composants Windows de manière sécurisée
    Write-Host "`nVérification des composants Windows..." -ForegroundColor Green
    $requiredFeatures = @{
        "Microsoft-Windows-Subsystem-Linux" = $false
        "VirtualMachinePlatform" = $false
    }

    # Créer un tableau des noms de features de manière sécurisée
    $featureNames = @($requiredFeatures.Keys)
    foreach ($feature in $featureNames) {
        $state = Get-WindowsOptionalFeature -Online -FeatureName $feature
        if ($state.State -eq "Enabled") {
            Write-Host "$feature : Déjà activé" -ForegroundColor Green
            $requiredFeatures[$feature] = $true
        } else {
            Write-Host "$feature : Non activé" -ForegroundColor Yellow
            $needsAction = $true
        }
    }

    # 4. Vérifier WSL2
    Write-Host "`nVérification de WSL2..." -ForegroundColor Green
    $wslPath = "${env:WinDir}\System32\wsl.exe"
    $wslKernelPath = "${env:WinDir}\System32\lxss\tools\kernel"
    $wsl2Installed = $false

    if ((Test-Path $wslPath) -and (Test-Path $wslKernelPath)) {
        $wslVersion = wsl --status 2>$null
        if ($wslVersion -match "2") {
            Write-Host "WSL2 est correctement installé" -ForegroundColor Green
            $wsl2Installed = $true
        }
    }

    if (-not $wsl2Installed) {
        Write-Host "WSL2 n'est pas installé correctement" -ForegroundColor Yellow
        $needsAction = $true
    }

    # 5. Vérifier Docker Desktop
    Write-Host "`nVérification de Docker Desktop..." -ForegroundColor Green
    $dockerPath = Join-Path ${env:ProgramFiles} "Docker\Docker\Docker Desktop.exe"
    $dockerInstalled = Test-Path $dockerPath
    $dockerRunning = Get-Process "Docker Desktop" -ErrorAction SilentlyContinue

    if (-not $dockerInstalled) {
        Write-Host "Docker Desktop n'est pas installé" -ForegroundColor Yellow
        $needsAction = $true
    } elseif (-not $dockerRunning) {
        Write-Host "Docker Desktop est installé mais n'est pas en cours d'exécution" -ForegroundColor Yellow
        $needsAction = $true
    } else {
        Write-Host "Docker Desktop est installé et en cours d'exécution" -ForegroundColor Green
    }

    # Si des actions sont nécessaires, les effectuer
    if ($needsAction) {
        Write-Host "`nDes actions sont nécessaires pour configurer l'environnement:" -ForegroundColor Yellow

        # Activer les composants Windows si nécessaire
        foreach ($feature in $requiredFeatures.Keys) {
            if (-not $requiredFeatures[$feature]) {
                Write-Host "Activation de $feature..." -ForegroundColor Yellow
                Enable-WindowsOptionalFeature -Online -FeatureName $feature -NoRestart
                $needsReboot = $true
            }
        }

        # Instructions pour WSL2 si nécessaire
        if (-not $wsl2Installed) {
            Write-Host "`nPour installer WSL2:" -ForegroundColor Yellow
            Write-Host "1. Téléchargez le package du kernel Linux:" -ForegroundColor Yellow
            Write-Host "   https://wslstorestorage.blob.core.windows.net/wslblob/wsl_update_x64.msi" -ForegroundColor Cyan
            Write-Host "2. Installez le package" -ForegroundColor Yellow
            Write-Host "3. Exécutez 'wsl --set-default-version 2'" -ForegroundColor Yellow
        }

        # Instructions pour Docker si nécessaire
        if (-not $dockerInstalled) {
            Write-Host "`nPour installer Docker Desktop:" -ForegroundColor Yellow
            Write-Host "1. Téléchargez Docker Desktop:" -ForegroundColor Yellow
            Write-Host "   https://www.docker.com/products/docker-desktop" -ForegroundColor Cyan
            Write-Host "2. Installez en cochant 'Use WSL 2 based engine'" -ForegroundColor Yellow
            throw "Docker Desktop doit être installé"
        }

        # Démarrer Docker si nécessaire
        if ($dockerInstalled -and (-not $dockerRunning)) {
            Write-Host "`nDémarrage de Docker Desktop..." -ForegroundColor Yellow
            Start-Process $dockerPath
            
            Write-Host "Attente du démarrage de Docker..." -ForegroundColor Yellow
            $attempts = 0
            while ($attempts -lt 30) {
                docker info >$null 2>&1
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "Docker est prêt!" -ForegroundColor Green
                    break
                }
                Start-Sleep -Seconds 2
                Write-Host "." -NoNewline -ForegroundColor Yellow
                $attempts++
            }
            Write-Host ""
            
            if ($attempts -ge 30) {
                throw "Docker n'a pas démarré correctement"
            }
        }

        if ($needsReboot) {
            Write-Host "`nUn redémarrage est nécessaire pour terminer l'installation" -ForegroundColor Red
            Write-Host "Appuyez sur Entrée pour redémarrer..." -ForegroundColor Yellow
            pause
            Restart-Computer -Force
            exit 0
        }
    }

    # Déploiement Docker
    Write-Host "`nDéploiement vers Docker..." -ForegroundColor Cyan
    Write-Host "Image: $($prodSettings.DockerSettings.ImageName)" -ForegroundColor Gray
    Write-Host "Port: 5040" -ForegroundColor Gray

    # Préparation des répertoires
    # Write-Host "`nPréparation des répertoires..." -ForegroundColor Green
    # $directories = @(
    #     (Join-Path $rootPath "data"),
    #     (Join-Path $rootPath "logs")
    # )
    # foreach ($dir in $directories) {
    #     if (-not (Test-Path $dir)) { 
    #         New-Item -ItemType Directory -Path $dir -Force | Out-Null
    #         Write-Host "Création du répertoire: $dir" -ForegroundColor Yellow
    #     }
    # }

    # Déploiement des conteneurs
    Write-Host "`nDéploiement des conteneurs..." -ForegroundColor Green
    Push-Location $rootPath

    # Stop any existing containers
    docker-compose -f (Join-Path $rootPath "docker-compose.yml") down

    # Start containers in foreground
    Write-Host "`nDémarrage des conteneurs... Ctrl+C pour arrêter" -ForegroundColor Yellow
    docker-compose -f (Join-Path $rootPath "docker-compose.yml") up --build

    # Test final
    $apiUrl = "http://localhost:5040/api/product"
    Start-Process $apiUrl
    Write-Host "`nDéploiement terminé!" -ForegroundColor Green
    Write-Host "API: $apiUrl" -ForegroundColor Cyan

    # Nettoyage des processus
    Get-Process | Where-Object { $_.MainWindowTitle -like "*$apiUrl*" } | Stop-Process -Force

    # No need for cleanup as docker-compose up (without -d) will handle it
}
catch {
    Write-Host "`nErreur: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Solutions possibles:" -ForegroundColor Yellow
    Write-Host "1. Vérifiez que la virtualisation est activée dans le BIOS" -ForegroundColor Yellow
    Write-Host "2. Installez WSL2 et le kernel Linux" -ForegroundColor Yellow
    Write-Host "3. Installez Docker Desktop" -ForegroundColor Yellow
    Write-Host "4. Redémarrez votre ordinateur" -ForegroundColor Yellow
    pause; exit 1
}
finally {
    Pop-Location
} 