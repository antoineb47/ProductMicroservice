using Microsoft.EntityFrameworkCore;
using ProductMicroservice.Data;
using ProductMicroservice.Repository;
using Serilog;
using System.Text.Json.Serialization;

// Point d'entrée de l'application
Console.WriteLine("Démarrage de ProductMicroservice...");

var builder = WebApplication.CreateBuilder(args);

// Configuration du système de journalisation
Console.WriteLine("Configuration de la journalisation...");
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

builder.Host.UseSerilog();

try
{
    // Configuration des services de l'application
    Console.WriteLine("Configuration des services...");
    builder.Services.AddControllers()
        .AddJsonOptions(options => 
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

    // Configuration de Swagger uniquement pour l'environnement de développement
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    // Configuration de la base de données SQLite
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Chaîne de connexion : {connectionString}");
    
    builder.Services.AddDbContext<ProductContext>(options =>
    {
        options.UseSqlite(connectionString);
    });

    // Enregistrement du repository dans le conteneur d'injection de dépendances
    builder.Services.AddScoped<IProductRepository, ProductRepository>();

    // Construction de l'application
    var app = builder.Build();

    // Création et initialisation de la base de données si nécessaire
    Console.WriteLine("Vérification de la base de données...");
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ProductContext>();
        context.Database.EnsureCreated();
    }

    // Configuration du middleware de gestion des erreurs
    app.UseExceptionHandler("/error");

    // Configuration de Swagger pour l'environnement de développement
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Configuration des routes de l'API
    app.MapControllers();

    // Démarrage du serveur web
    Console.WriteLine("Démarrage du serveur web...");
    app.Run();
}
catch (Exception ex)
{
    // Gestion des erreurs fatales
    Console.WriteLine($"L'application s'est arrêtée de manière inattendue : {ex}");
    Log.Fatal(ex, "L'application s'est arrêtée de manière inattendue");
    throw;
}
finally
{
    // Nettoyage des ressources de journalisation
    Log.CloseAndFlush();
}
