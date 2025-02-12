function Run-DotnetCommand {
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

Run-DotnetCommand "Nettoyage" @("clean")
Run-DotnetCommand "Restauration" @("restore")
Run-DotnetCommand "Construction" @("build")
Run-DotnetCommand "Tests unitaires" @("test", "--verbosity", "normal")

# Démarrer l'application et ouvrir Swagger
Run-DotnetCommand "Démarrage (hot reload activé)" @("watch", "run", "--project", "src/ProductMicroservice.csproj")

Pop-Location