using Microsoft.EntityFrameworkCore;
using ProductMicroservice.Data;
using ProductMicroservice.Repository;
using Serilog;
using System.Text.Json.Serialization;

Console.WriteLine("Starting ProductMicroservice...");

var builder = WebApplication.CreateBuilder(args);

// Configure console logging first
Console.WriteLine("Configuring logging...");
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

builder.Host.UseSerilog();

try
{
    Console.WriteLine("Configuring services...");
    builder.Services.AddControllers()
        .AddJsonOptions(options => 
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });

    // Add Swagger for development environment
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
    }

    // Configure SQLite
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Connection string: {connectionString}");
    
    builder.Services.AddDbContext<ProductContext>(options =>
    {
        options.UseSqlite(connectionString);
    });

    builder.Services.AddScoped<IProductRepository, ProductRepository>();

    var app = builder.Build();

    // Ensure database exists and is seeded
    Console.WriteLine("Checking database...");
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ProductContext>();
        context.Database.EnsureCreated();
    }

    app.UseExceptionHandler("/error");

    // Configure Swagger for development environment
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.MapControllers();

    Console.WriteLine("Starting web host...");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application terminated unexpectedly: {ex}");
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
