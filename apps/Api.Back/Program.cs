var builder = WebApplication.CreateBuilder(args);

// Ajoute les services pour Swagger/OpenAPI (pour tester tes futurs points d'entrée)
builder.Services.AddOpenApi();

// CONFIGURATION CORS : Pour autoriser ton React (port 5173) et Astro (port 4321)
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

// Active l'interface de test Swagger en mode développement
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Sécurité de base
app.UseHttpsRedirection();
app.UseCors("DevPolicy");
app.UseMiddleware<Api.Back.Middleware.ExceptionMiddleware>();
// --- ZONE POUR TES FUTURS POINTS D'ENTRÉE ---

app.MapGet("/api/test", () => new { message = "L'API TaskForce est en ligne !" });

// --------------------------------------------

app.Run();