function Invoke-DotnetCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Operation,
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )
    Write-Host "$Operation..." -ForegroundColor Cyan
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Échec de l'opération!" -ForegroundColor Red
        Read-Host -Prompt "Appuyez sur Entrée pour quitter"
        exit $LASTEXITCODE
    }
}

# Aller au répertoire de la solution
$projectPath = Join-Path $PSScriptRoot "..\.."
Push-Location $projectPath

Invoke-DotnetCommand "Nettoyage" @("clean")
Invoke-DotnetCommand "Restauration" @("restore")
Invoke-DotnetCommand "Construction" @("build")
Invoke-DotnetCommand "Tests unitaires" @("test", "--verbosity", "normal")

# Démarrer l'application et ouvrir Swagger
Invoke-DotnetCommand "Démarrage (hot reload activé)" @("watch", "run", "--project", "src/ProductMicroservice.csproj")

Pop-Location