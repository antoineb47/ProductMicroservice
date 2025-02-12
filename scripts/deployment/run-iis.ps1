# Doit être exécuté en tant qu'Administrateur

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "Veuillez exécuter ce script en tant qu'Administrateur" -ForegroundColor Red
    pause
    exit 1
}

try {
    # Chemins d'accès
    $rootPath = (Get-Item $PSScriptRoot).Parent.Parent.FullName
    $publishPath = Join-Path $rootPath "src\bin\Release\net8.0\publish"
    $siteName = "ProductMicroservice"
    $port = 5000

    # 1. Nettoyage IIS
    Write-Host "Nettoyage d'IIS..." -ForegroundColor Yellow
    if (Get-Website -Name $siteName) {
        Stop-Website -Name $siteName -ErrorAction SilentlyContinue
        Remove-Website -Name $siteName -ErrorAction SilentlyContinue
        Remove-WebAppPool -Name $siteName -ErrorAction SilentlyContinue
        Get-Process | Where-Object { $_.ProcessName -eq "w3wp" } | Stop-Process -Force
        Start-Sleep -Seconds 2
    }

    # 2. Build et Test
    Write-Host "`nBuild et Test..." -ForegroundColor Green
    dotnet test $rootPath -c Release
    if ($LASTEXITCODE -ne 0) { throw "Tests échoués" }

    # 3. Publier
    Write-Host "`nPublication..." -ForegroundColor Green
    dotnet publish (Join-Path $rootPath "src") -c Release
    
    # 4. Configuration Base de données
    $dbPath = Join-Path $publishPath "data"
    New-Item -ItemType Directory -Path $dbPath -Force | Out-Null
    $devDb = Join-Path $rootPath "src\data\productmicroservice.db"
    if (Test-Path $devDb) {
        Copy-Item -Path $devDb -Destination (Join-Path $dbPath "productmicroservice.db") -Force
    }

    # 5. Configuration IIS
    Write-Host "`nConfiguration d'IIS..." -ForegroundColor Green
    Import-Module WebAdministration
    New-WebAppPool -Name $siteName -ErrorAction SilentlyContinue
    Set-ItemProperty "IIS:\AppPools\$siteName" -Name "managedRuntimeVersion" -Value ""
    New-Website -Name $siteName -PhysicalPath $publishPath -Port $port -ApplicationPool $siteName -Force

    # Permissions
    $acl = Get-Acl $publishPath
    @("IIS AppPool\$siteName", "IUSR", "IIS_IUSRS") | ForEach-Object {
        $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($_, "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
        $acl.SetAccessRule($accessRule)
    }
    Set-Acl $publishPath $acl

    # 6. Démarrage
    Start-Website -Name $siteName
    Start-Process "http://localhost:$port/api/Product"
    Write-Host "`nSite web déployé sur: http://localhost:$port/api/Product" -ForegroundColor Green
    
    Write-Host "`nAppuyez sur une touche pour terminer..." -ForegroundColor Yellow
    pause
}
catch {
    Write-Host "Erreur: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    Write-Host "`nNettoyage..." -ForegroundColor Yellow
    if (Get-Website -Name $siteName) {
        Stop-Website -Name $siteName -ErrorAction SilentlyContinue
        Remove-Website -Name $siteName -ErrorAction SilentlyContinue
        Remove-WebAppPool -Name $siteName -ErrorAction SilentlyContinue
    }
}