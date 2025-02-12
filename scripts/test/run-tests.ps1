param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [Parameter()]
    [switch]$Coverage
)

$solutionRoot = (Get-Item $PSScriptRoot).Parent.Parent.FullName
$projectPath = Join-Path $solutionRoot "src"
$testProjectPath = Join-Path $solutionRoot "tests"

Write-Host "Exécution des tests en configuration $Configuration..." -ForegroundColor Green

# Construction des arguments de test
$testArgs = @(
    'test'
    $testProjectPath
    "--configuration", $Configuration
    '--verbosity', 'normal'
)

if ($Coverage) {
    Write-Host "Génération du rapport de couverture de code..." -ForegroundColor Yellow
    $testArgs += @(
        '/p:CollectCoverage=true'
        '/p:CoverletOutputFormat=cobertura'
        '/p:CoverletOutput=./TestResults/coverage.cobertura.xml'
    )
}

# Exécution des tests
try {
    dotnet @testArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nTous les tests ont réussi!" -ForegroundColor Green
    } else {
        Write-Host "`nCertains tests ont échoué." -ForegroundColor Red
        exit $LASTEXITCODE
    }
} catch {
    Write-Host "Erreur lors de l'exécution des tests: $_" -ForegroundColor Red
    exit 1
} finally {
    Write-Host "`nAppuyez sur une touche pour continuer..." -ForegroundColor Yellow
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
} 