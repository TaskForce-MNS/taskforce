using Api.Back.Data;
using Api.Back.IRepositories;
using Api.Back.Repositories;
using Api.Back.Services;
using Api.Back.Services.Interface;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICES (La boîte à outils) ---

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ✅ Configurer Swagger pour qu'il lise les annotations [SwaggerOperation]
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});

// ✅ Injection des dépendances (Le câblage)
// Indispensable pour que le Controller trouve le Service, et le Service trouve le Repo
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ✅ FluentValidation
// Scanne tout le projet pour trouver tes validateurs (comme RegisterUserDtoValidator)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Base de données PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:4321")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// --- 2. PIPELINE ---

// Middleware d'erreur global (si le fichier existe bien chez toi)
// app.UseMiddleware<Api.Back.Middleware.ExceptionMiddleware>(); 
// (Je l'ai commenté par sécurité, décommente-le si tu as bien créé le fichier)

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // L'interface visuelle classique
}

app.UseHttpsRedirection();

app.UseCors("DevPolicy");
app.UseAuthorization();

// --- 3. POINTS D'ENTRÉE ---

// --- REMPLACE app.MapControllers(); PAR CECI ---

try
{
    app.MapControllers();
}
catch (System.Reflection.ReflectionTypeLoadException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;

    foreach (var error in ex.LoaderExceptions.Where(error => error != null))
    {
        Console.WriteLine($"- ERREUR : {error!.Message}");
    }
    Console.ResetColor();
    throw; // On arrête le programme
}
// Petit test healthcheck
app.MapGet("/api/test", () => new { message = "L'API TaskForce est en ligne !" });

app.Run();