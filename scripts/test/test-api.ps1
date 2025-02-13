param(
    [int]$Port = $(Read-Host -Prompt "Port de l'API (5000 pour local, 5020 pour IIS, 5040 pour Docker)")
)

$baseUrl = "http://localhost:$Port"

Write-Host "`n=== Test de l'API ===" -ForegroundColor Cyan
Write-Host "URL: $baseUrl`n" -ForegroundColor Cyan

try {
    # Categories
    Write-Host "Test Categories..." -ForegroundColor Yellow
    $categories = Invoke-RestMethod -Uri "$baseUrl/api/Category" -Method Get
    Write-Host "GET /api/Category" -ForegroundColor Green
    $categories | ConvertTo-Json

    $category = Invoke-RestMethod -Uri "$baseUrl/api/Category/1" -Method Get
    Write-Host "`nGET /api/Category/1" -ForegroundColor Green
    $category | ConvertTo-Json

    # Products
    Write-Host "`nTest Products..." -ForegroundColor Yellow
    $products = Invoke-RestMethod -Uri "$baseUrl/api/Product" -Method Get
    Write-Host "GET /api/Product" -ForegroundColor Green
    $products | ConvertTo-Json

    $produit = @{
        name = "Test"
        description = "Test"
        price = 9.99
        categoryId = 1
    }
    
    $nouveau = Invoke-RestMethod -Uri "$baseUrl/api/Product" -Method Post -Body ($produit | ConvertTo-Json) -ContentType "application/json"
    Write-Host "`nPOST /api/Product" -ForegroundColor Green
    $nouveau | ConvertTo-Json

    $produit = Invoke-RestMethod -Uri "$baseUrl/api/Product/$($nouveau.id)" -Method Get
    Write-Host "`nGET /api/Product/$($nouveau.id)" -ForegroundColor Green
    $produit | ConvertTo-Json

    # Mise à jour avec les données exactes reçues
    $updateData = [PSCustomObject]@{
        id = $produit.id
        name = "Test modifié"
        description = $produit.description
        price = $produit.price
        categoryId = $produit.categoryId
    }
    
    Write-Host "`nPUT /api/Product/$($nouveau.id)" -ForegroundColor Green
    $jsonBody = $updateData | ConvertTo-Json -Depth 10
    $update = Invoke-RestMethod -Uri "$baseUrl/api/Product/$($nouveau.id)" -Method Put -Body $jsonBody -ContentType "application/json; charset=utf-8"
    $update | ConvertTo-Json

    $produit = Invoke-RestMethod -Uri "$baseUrl/api/Product/$($nouveau.id)" -Method Get
    Write-Host "`nGET /api/Product/$($nouveau.id)" -ForegroundColor Green
    $produit | ConvertTo-Json

    Invoke-RestMethod -Uri "$baseUrl/api/Product/$($nouveau.id)" -Method Delete
    Write-Host "`nDELETE /api/Product/$($nouveau.id)" -ForegroundColor Green
    
    Write-Host "`nTests terminés!" -ForegroundColor Green
} catch {
    Write-Host "`nERREUR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails.Message) {
        Write-Host "Détails: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}
Write-Host "`nAppuyez sur Entrée pour fermer..."
Read-Host

