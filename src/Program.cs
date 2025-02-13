using Microsoft.EntityFrameworkCore;
using ProductMicroservice.API.Data;
using ProductMicroservice.Repository;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Récupérer le port depuis la configuration ou la variable d'environnement
var port = builder.Configuration.GetValue<int>("ApiSettings:Port");
if (port > 0)
{
    builder.WebHost.UseUrls($"http://localhost:{port}");
}

// Configuration de Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .CreateLogger();

builder.Host.UseSerilog();

// Ajouter les services au conteneur
builder.Services.AddControllers()
    .AddJsonOptions(options => 
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configurer SQLite
builder.Services.AddDbContext<ProductContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurer le pattern Repository
builder.Services.AddScoped<IProductRepository, ProductRepository>();

#if !RELEASE
// Configurer Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endif

// Configurer CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy => policy
            .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

// S'assurer que la base de données est créée
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ProductContext>();
    context.Database.EnsureCreated();
}

// Configurer le pipeline de requêtes HTTP
if (app.Environment.IsDevelopment())
{
#if !RELEASE
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Microservice API V1");
        c.RoutePrefix = "swagger";
    });
#endif
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Ajouter les en-têtes de sécurité
app.Use((context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
    return next();
});

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");
app.UseAuthorization();
app.MapControllers();

try
{
    Log.Information("Démarrage de l'application web - Environnement: {Environment}", 
        app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "L'application s'est terminée de manière inattendue");
}
finally
{
    Log.CloseAndFlush();
}
