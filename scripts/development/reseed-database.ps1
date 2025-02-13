# Script de reseeding de la base de données
param(
    [Parameter(Mandatory=$false)]
    [ValidateRange(1,65535)]
    [int]$Port = $(Read-Host -Prompt "Port de l'API (5000 pour local, 5020 pour IIS, 5040 pour Docker)")
)

# Prevent the script from closing on errors
trap {
    Write-Host "`nUne erreur est survenue: $_" -ForegroundColor Red
    Write-Host "`nAppuyez sur Entrée pour quitter..." -ForegroundColor Yellow
    Read-Host
    break
}

Write-Host @"
=== Réinitialisation de la base de données ===
Ce script va:
1. Supprimer tous les produits existants
2. Vérifier les catégories disponibles
3. Créer des produits de test pour chaque catégorie

Port sélectionné: $Port
"@ -ForegroundColor Cyan

$baseUrl = "http://localhost:$Port"
Write-Host "`nConnexion à $baseUrl`n" -ForegroundColor Yellow

try {
    # Nettoyage des produits
    Write-Host "`nNettoyage des produits..." -ForegroundColor Cyan
    $products = Invoke-RestMethod -Uri "$baseUrl/api/Product"
    foreach ($product in $products) {
        Write-Host "  - Suppression: $($product.name)" -ForegroundColor Gray
        Invoke-RestMethod -Uri "$baseUrl/api/Product/$($product.id)" -Method DELETE
    }

    # Récupération des catégories existantes
    Write-Host "`nRécupération des catégories..." -ForegroundColor Cyan
    $categories = Invoke-RestMethod -Uri "$baseUrl/api/Category"
    foreach ($category in $categories) {
        Write-Host "  - $($category.name) (ID: $($category.id))" -ForegroundColor Gray
    }

    # Produits de test
    $produits = @(
        @{
            name = "Laptop Dell XPS"
            description = "Laptop haut de gamme pour les professionnels"
            price = 1299.99
            categoryId = ($categories | Where-Object { $_.name -eq "Electronics" }).id
        },
        @{
            name = "T-shirt Coton Bio"
            description = "T-shirt confortable en coton biologique"
            price = 29.99
            categoryId = ($categories | Where-Object { $_.name -eq "Clothes" }).id
        },
        @{
            name = "Cafe Bio"
            description = "Cafe biologique premium"
            price = 12.99
            categoryId = ($categories | Where-Object { $_.name -eq "Grocery" }).id
        }
    )

    # Ajout des produits
    Write-Host "`nAjout des produits de test..." -ForegroundColor Cyan
    foreach ($produit in $produits) {
        Write-Host "  - $($produit.name)" -ForegroundColor Gray
        Invoke-RestMethod -Uri "$baseUrl/api/Product" -Method POST -Body ($produit | ConvertTo-Json) -ContentType "application/json"
    }

    Write-Host "`nBase de données réinitialisée avec succès!" -ForegroundColor Green
} catch {
    Write-Host "`nErreur lors de la réinitialisation: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nAppuyez sur Entrée pour quitter..." -ForegroundColor Yellow
$null = Read-Host
