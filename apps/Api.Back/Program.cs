using Api.Back.DTOs;
using FluentValidation;
using Scalar.AspNetCore; // Indispensable pour l'interface

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICES ---
builder.Services.AddControllers();

// Active le nouveau générateur OpenAPI de .NET 9
builder.Services.AddOpenApi();

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

// Middleware d'erreur (si tu l'as recréé)
// app.UseMiddleware<Api.Back.Middleware.ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    // Génère le JSON sur /openapi/v1.json
    app.MapOpenApi();
    // Affiche l'interface visuelle sur /scalar/v1
    app.MapScalarApiReference();
}

// app.UseHttpsRedirection();
app.UseCors("DevPolicy");
app.UseAuthorization();

app.MapControllers();
app.MapGet("/api/test", () => new { message = "L'API TaskForce est en ligne (Mode .NET 9) !" });

app.Run();