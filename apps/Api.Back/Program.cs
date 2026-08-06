using Api.Back.Data;
using Api.Back.Repositories;
using Api.Back.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Fido2NetLib;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Api.Back.Common;
using System.Data.Common;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

#region SERVICES 
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c => c.EnableAnnotations());
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("PostgreSQL Database");
// .AddRedis(redisConnection, name: "Redis Cache");
// dotnet add package AspNetCore.HealthChecks.Redis
// Repos & Services
builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddScoped<IProjectMemberRepository, ProjectMemberRepository>();
builder.Services.AddScoped<IInvitationRepository, InvitationRepository>();
builder.Services.AddScoped<IInvitationService, InvitationService>();

builder.Services.AddMemoryCache();

// Redis for Refresh Tokens
var redisConnection = builder.Configuration["Redis:ConnectionString"]
    ?? throw new InvalidOperationException("Redis:ConnectionString manquant");
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(redisConnection);
    configuration.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
// Validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddFido2(options =>
{
    options.ServerDomain = "taskforce.local";
    options.ServerName = "TaskForce Zero-Knowledge";
    options.Origins = builder.Environment.IsDevelopment()
          ? new HashSet<string>
          {
            "https://app.taskforce.local",
            "http://localhost:5173",
            "tauri://localhost",
          }
          : new HashSet<string>
          {
            "https://app.taskforce.local",
            "https://taskforce.local",
          };
    options.TimestampDriftTolerance = 300000;
});

#endregion

#region AUTHENTICATION
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key manquant")))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies[SharedConstants.SessionCookieName];
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                ctx.HandleResponse();
                ctx.Response.StatusCode = 401;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync("{\"error\":\"Non autorisé\"}");
            },
            OnForbidden = ctx =>
            {
                ctx.Response.StatusCode = 403;
                ctx.Response.ContentType = "application/json";
                return ctx.Response.WriteAsync("{\"error\":\"Accès refusé\"}");
            }
        };
    });
#endregion

#region DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

#region CORS POLICY
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
    {
        policy.WithOrigins("https://app.taskforce.local",
            "https://taskforce.local",
            "http://localhost:5173",
            "http://localhost:4321")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
    options.AddPolicy("ProdPolicy", policy =>
        policy.WithOrigins("https://app.taskforce.local",
                "https://taskforce.local")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});
#endregion

#region BUILD
var app = builder.Build();

// Middleware
app.UseMiddleware<Api.Back.Middleware.ExceptionMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevPolicy");
}
else
{
    app.UseHsts();
    app.UseCors("ProdPolicy");
}

// Security
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/api/v1/back/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        var result = new
        {
            status = report.Status.ToString(),           // Healthy / Unhealthy / Degraded
            totalDuration = report.TotalDuration,
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description ?? "OK",
                duration = e.Value.Duration,
                error = e.Value.Exception?.Message
            }).ToList()
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(result);
    }
});
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();

        var logMigrationError = LoggerMessage.Define(
            LogLevel.Error,
            new EventId(1, "DbMigrationError"),
            "Une erreur est survenue pendant la migration de la base de données."
        );

        logMigrationError(logger, ex);

        throw;
    }
}

// Endpoint temporaire pour tester le flux Docker
app.MapGet("/api/v1/back/debug/test-db", async (AppDbContext db) =>
{
    try
    {
        var data = await db.Identities.Take(3).ToListAsync();
        return Results.Ok(new { message = "Connexion DB réussie !", data });
    }
    catch (DbException ex)
    {
        return Results.Problem($"Erreur de DB : {ex.Message}");
    }
});

app.Run();
#endregion