using Api.Back.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICES ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ajoute la connexion à PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Configuration CORS pour React et Astro
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

// --- 2. PIPELINE (L'ordre compte !) ---

// On attrape les erreurs en premier
app.UseMiddleware<Api.Back.Middleware.ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Le CORS doit être AVANT l'autorisation et les routes
app.UseCors("DevPolicy");

app.UseAuthorization();

// --- 3. POINTS D'ENTRÉE ---
app.MapControllers();

// Petit test pour vérifier que tout roule
app.MapGet("/api/test", () => new { message = "L'API TaskForce est en ligne !" });

app.Run();