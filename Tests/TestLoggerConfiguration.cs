using Microsoft.Extensions.Configuration;
using Serilog;

namespace ProductMicroservice.Tests;

/// <summary>
/// Configuration du logger pour l'environnement de test
/// Assure une séparation entre les logs de test et de production
/// </summary>
public static class TestLoggerConfiguration
{
    /// <summary>
    /// Configure le logger pour l'environnement de test
    /// Utilise un fichier de configuration dédié et un répertoire distinct
    /// </summary>
    public static void ConfigureTestLogging()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }

    /// <summary>
    /// Nettoie les ressources du logger après l'exécution des tests
    /// </summary>
    public static void CleanupTestLogging()
    {
        Log.CloseAndFlush();
    }
} 