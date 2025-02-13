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
    if (-not $prodSettings.IISSettings -or -not $prodSettings.ApiSettings) {
        throw "Configuration IIS ou API manquante dans appsettings.Production.json"
    }
    if (-not $prodSettings.IISSettings.PhysicalPath -or -not $prodSettings.IISSettings.SiteName -or -not $prodSettings.IISSettings.AppPoolName) {
        throw "Configuration IIS incomplète (PhysicalPath, SiteName ou AppPoolName manquant)"
    }

    $publishPath = [System.Environment]::ExpandEnvironmentVariables($prodSettings.IISSettings.PhysicalPath)
    
    Write-Host "`nDéploiement vers IIS..." -ForegroundColor Cyan
    Write-Host "Site: $($prodSettings.IISSettings.SiteName)" -ForegroundColor Gray
    Write-Host "Port: $($prodSettings.ApiSettings.Port)" -ForegroundColor Gray
    Write-Host "Chemin: $publishPath" -ForegroundColor Gray

    # Installation des composants Windows
    Write-Host "`nVérification des composants Windows..." -ForegroundColor Green
    
    $features = @(
        "IIS-WebServerRole",
        "IIS-WebServer",
        "IIS-CommonHttpFeatures",
        "IIS-StaticContent",
        "IIS-DefaultDocument",
        "IIS-ApplicationInit",
        "IIS-NetFxExtensibility45",
        "IIS-ASPNET45",
        "IIS-ISAPIExtensions",
        "IIS-ISAPIFilter",
        "IIS-ManagementConsole"
    )

    $missingFeatures = @()
    foreach ($feature in $features) {
        $state = Get-WindowsOptionalFeature -Online -FeatureName $feature
        if ($state.State -ne "Enabled") {
            $missingFeatures += $feature
        }
    }

    if ($missingFeatures.Count -gt 0) {
        Write-Host "Installation des composants manquants..." -ForegroundColor Yellow
        foreach ($feature in $missingFeatures) {
            Write-Host "Installation de $feature..." -ForegroundColor Yellow
            dism.exe /online /enable-feature /featurename:$feature /all /norestart
            if ($LASTEXITCODE -ne 0) {
                Write-Host "Erreur lors de l'installation de $feature" -ForegroundColor Red
                throw "Échec de l'installation des composants IIS"
            }
        }
    } else {
        Write-Host "Tous les composants IIS sont déjà installés" -ForegroundColor Green
    }

    # Installation du bundle d'hébergement .NET
    Write-Host "`nVérification du bundle d'hébergement .NET..." -ForegroundColor Green
    
    # Check if ASP.NET Core Module is installed
    $aspNetCoreModulePath = "${env:ProgramFiles}\IIS\Asp.Net Core Module\V2\aspnetcorev2.dll"
    $dotnetRuntimePath = "${env:ProgramFiles}\dotnet\shared\Microsoft.AspNetCore.App\8.0.2"
    
    if ((Test-Path $aspNetCoreModulePath) -and (Test-Path $dotnetRuntimePath)) {
        Write-Host "Bundle d'hébergement .NET 8.0 déjà installé" -ForegroundColor Green
    } else {
        Write-Host "Le bundle d'hébergement .NET 8.0 n'est pas installé" -ForegroundColor Yellow
        Write-Host "Veuillez installer le bundle d'hébergement .NET 8.0:" -ForegroundColor Yellow
        Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
        Write-Host "Ou exécutez cette commande dans PowerShell:" -ForegroundColor Yellow
        Write-Host "Invoke-WebRequest 'https://download.visualstudio.microsoft.com/download/pr/98ff0a08-a283-428f-8e54-19841d97154c/8c7d5f9600eadf264f04c82c813b7aab/dotnet-hosting-8.0.2-win.exe' -OutFile 'dotnet-hosting.exe'; Start-Process 'dotnet-hosting.exe' -Wait" -ForegroundColor Gray
        throw "Le bundle d'hébergement .NET doit être installé"
    }

    # Vérification des modules IIS
    Write-Host "`nVérification des modules IIS..." -ForegroundColor Green
    Import-Module WebAdministration -ErrorAction Stop

    # Redémarrage du service IIS
    Write-Host "Redémarrage d'IIS..." -ForegroundColor Yellow
    iisreset /restart
    Start-Sleep -Seconds 5

    # Préparation des répertoires
    Write-Host "`nPréparation des répertoires..." -ForegroundColor Green
    $directories = @(
        $publishPath,
        (Join-Path $publishPath "data"),
        (Join-Path $publishPath "logs")
    )
    foreach ($dir in $directories) {
        if (-not (Test-Path $dir)) { 
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
            Write-Host "Création du répertoire: $dir" -ForegroundColor Yellow
        }
    }

    # Set initial directory permissions
    foreach ($dir in $directories) {
        $acl = Get-Acl $dir
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
            "IIS_IUSRS", 
            "FullControl", 
            [System.Security.AccessControl.InheritanceFlags]::None,
            [System.Security.AccessControl.PropagationFlags]::None,
            "Allow"
        )
        $acl.AddAccessRule($accessRule)
        Set-Acl -Path $dir -AclObject $acl
    }

    # Copie de la base de données
    Write-Host "`nCopie de la base de données..." -ForegroundColor Green
    $sourceDbPath = Join-Path $srcPath "data\productmicroservice.db"
    $destDbPath = Join-Path $publishPath "data\productmicroservice.db"
    if (Test-Path $sourceDbPath) {
        Copy-Item -Path $sourceDbPath -Destination $destDbPath -Force
        # Set database file permissions
        $acl = Get-Acl $destDbPath
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
            "IIS AppPool\$($prodSettings.IISSettings.AppPoolName)", 
            "FullControl", 
            [System.Security.AccessControl.InheritanceFlags]::None,
            [System.Security.AccessControl.PropagationFlags]::None,
            "Allow"
        )
        $acl.AddAccessRule($accessRule)
        Set-Acl -Path $destDbPath -AclObject $acl
        Write-Host "Base de données copiée avec succès" -ForegroundColor Green
    } else {
        Write-Host "Base de données source non trouvée, une nouvelle sera créée" -ForegroundColor Yellow
    }

    # Copy database from release to publish
    $releaseDbPath = Join-Path $srcPath "bin\Release\net8.0\data\productmicroservice.db"
    if (Test-Path $releaseDbPath) {
        Copy-Item -Path $releaseDbPath -Destination $destDbPath -Force
        Write-Host "Base de données copiée du répertoire de release au répertoire de publication" -ForegroundColor Green
    } else {
        Write-Host "Base de données de release non trouvée" -ForegroundColor Yellow
    }

    # Configuration IIS
    Write-Host "`nConfiguration d'IIS..." -ForegroundColor Green

    # Nettoyage IIS
    if (Get-Website -Name $prodSettings.IISSettings.SiteName) {
        Stop-Website -Name $prodSettings.IISSettings.SiteName
        Remove-Website -Name $prodSettings.IISSettings.SiteName
    }
    if (Test-Path "IIS:\AppPools\$($prodSettings.IISSettings.AppPoolName)") {
        Stop-WebAppPool -Name $prodSettings.IISSettings.AppPoolName -ErrorAction SilentlyContinue
        Remove-WebAppPool -Name $prodSettings.IISSettings.AppPoolName
    }

    # Build et tests
    Push-Location $rootPath
    Write-Host "`nPréparation du projet..." -ForegroundColor Green
    
    Write-Host "Restauration des packages..." -ForegroundColor Yellow
    dotnet restore $srcPath --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Échec de la restauration" }

    Write-Host "Compilation..." -ForegroundColor Yellow
    dotnet build $srcPath -c Release --no-restore --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Échec de la compilation" }

    Write-Host "`nExécution des tests..." -ForegroundColor Green
    dotnet test (Join-Path $rootPath "tests") --no-build -c Release --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Échec des tests" }

    Write-Host "`nPublication..." -ForegroundColor Green
    dotnet publish $srcPath -c Release -o $publishPath --no-build --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Échec de la publication" }
    Copy-Item -Path (Join-Path $srcPath "web.config") -Destination $publishPath -Force

    # Configuration du pool et du site
    Write-Host "`nConfiguration du pool d'applications..." -ForegroundColor Green
    New-WebAppPool -Name $prodSettings.IISSettings.AppPoolName -Force
    
    # Configuration spécifique du pool pour .NET Core
    $appPool = "IIS:\AppPools\$($prodSettings.IISSettings.AppPoolName)"
    Set-ItemProperty $appPool -name "managedRuntimeVersion" -value ""
    Set-ItemProperty $appPool -name "processModel.identityType" -value "ApplicationPoolIdentity"
    Set-ItemProperty $appPool -name "startMode" -value "AlwaysRunning"
    Set-ItemProperty $appPool -name "processModel.loadUserProfile" -value "True"
    Set-ItemProperty $appPool -name "recycling.periodicRestart.time" -value "0"
    
    Write-Host "Configuration du site web..." -ForegroundColor Green
    New-Website -Name $prodSettings.IISSettings.SiteName `
               -PhysicalPath $publishPath `
               -Port $prodSettings.ApiSettings.Port `
               -ApplicationPool $prodSettings.IISSettings.AppPoolName `
               -Force

    # Vérifier le binding du site
    Write-Host "`nVérification du binding..." -ForegroundColor Green
    $binding = Get-WebBinding -Name $prodSettings.IISSettings.SiteName
    Write-Host "Port configuré: $($binding.bindingInformation)" -ForegroundColor Gray

    # Configuration des permissions
    Write-Host "`nConfiguration des permissions..." -ForegroundColor Green
    
    # Set permissions for each directory
    $directories = @(
        $publishPath,
        (Join-Path $publishPath "data"),
        (Join-Path $publishPath "logs")
    )
    
    foreach ($dir in $directories) {
        if (-not (Test-Path $dir)) {
            New-Item -ItemType Directory -Path $dir -Force | Out-Null
        }

        $acl = Get-Acl $dir

        # Application Pool Identity
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
            "IIS AppPool\$($prodSettings.IISSettings.AppPoolName)", 
            "FullControl", 
            [System.Security.AccessControl.InheritanceFlags]::None,
            [System.Security.AccessControl.PropagationFlags]::None,
            "Allow"
        )
        $acl.AddAccessRule($accessRule)

        # IIS_IUSRS
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
            "IIS_IUSRS", 
            "FullControl",
            [System.Security.AccessControl.InheritanceFlags]::None,
            [System.Security.AccessControl.PropagationFlags]::None,
            "Allow"
        )
        $acl.AddAccessRule($accessRule)

        # NETWORK SERVICE
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
            "NETWORK SERVICE", 
            "FullControl", 
            [System.Security.AccessControl.InheritanceFlags]::None,
            [System.Security.AccessControl.PropagationFlags]::None,
            "Allow"
        )
        $acl.AddAccessRule($accessRule)

        # Apply permissions
        Set-Acl -Path $dir -AclObject $acl

        # Apply to all files in this directory
        Get-ChildItem -Path $dir -File -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
            $fileAcl = Get-Acl $_.FullName
            foreach ($identity in @("IIS AppPool\$($prodSettings.IISSettings.AppPoolName)", "IIS_IUSRS", "NETWORK SERVICE")) {
                $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
                    $identity,
                    "FullControl",
                    [System.Security.AccessControl.InheritanceFlags]::None,
                    [System.Security.AccessControl.PropagationFlags]::None,
                    "Allow"
                )
                $fileAcl.AddAccessRule($accessRule)
            }
            Set-Acl -Path $_.FullName -AclObject $fileAcl
        }
    }

    # Ensure the data directory exists and is writable
    $dataDir = Join-Path $publishPath "data"
    if (-not (Test-Path $dataDir)) {
        New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
    }
    $testFile = Join-Path $dataDir "write_test.tmp"
    try {
        [System.IO.File]::WriteAllText($testFile, "test")
        Remove-Item $testFile -Force
        Write-Host "Permissions de données vérifiées avec succès" -ForegroundColor Green
    } catch {
        Write-Host "Erreur lors de la vérification des permissions de données" -ForegroundColor Red
        throw "Le répertoire de données n'est pas accessible en écriture"
    }

    # Démarrage et test
    Write-Host "`nDémarrage du site..." -ForegroundColor Green
    Start-WebAppPool -Name $prodSettings.IISSettings.AppPoolName
    Start-Website -Name $prodSettings.IISSettings.SiteName
    Start-Sleep -Seconds 5

    # Vérification des logs pour les erreurs
    $logPath = Join-Path $publishPath "logs"
    if (Test-Path $logPath) {
        Get-ChildItem -Path $logPath -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content
    }

    # Test final
    $apiUrl = "http://localhost:$($prodSettings.ApiSettings.Port)"
    Start-Process "$apiUrl/api/product"
    Write-Host "`nDéploiement terminé!" -ForegroundColor Green
    Write-Host "API: $apiUrl" -ForegroundColor Cyan

    # Nettoyage des processus
    Get-Process | Where-Object { $_.MainWindowTitle -like "*$apiUrl*" } | Stop-Process -Force
    
    pause
}
catch {
    Write-Host "`nErreur: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Solutions possibles:" -ForegroundColor Yellow
    Write-Host "1. Installez le bundle d'hébergement .NET 8.0" -ForegroundColor Yellow
    Write-Host "2. Exécutez 'iisreset' en tant qu'administrateur" -ForegroundColor Yellow
    Write-Host "3. Vérifiez les permissions Windows" -ForegroundColor Yellow
    Write-Host "4. Redémarrez votre ordinateur et réessayez" -ForegroundColor Yellow
    pause; exit 1
}
finally {
    Remove-Module WebAdministration -ErrorAction SilentlyContinue
    Pop-Location
}