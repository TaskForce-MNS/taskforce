using Api.Back.Data;
using Api.Back.Repositories;
using Api.Back.Services;
// using Api.Back.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region SERVICES 
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c => c.EnableAnnotations());
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("PostgreSQL Database");
// Repos & Services
builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
// builder.Services.AddScoped<IPasswordValidator, PasswordValidator>();

builder.Services.AddMemoryCache();
builder.Services.AddFido2(options =>
{
    options.ServerDomain = "taskforce.local";
    options.ServerName = "TaskForce Zero-Knowledge";

    options.Origins = new HashSet<string>
    {
        "https://app.taskforce.local",
        "https://taskforce.local",
        "http://localhost:5173",
        "http://localhost:4321",
        "tauri://localhost",
    };
    options.TimestampDriftTolerance = 300000;
});
// Validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
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
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key manquant")))
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
        policy.WithOrigins("https://{prod-domaine}", "https://{prod-domaine}")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});
#endregion

#region BUILD
var app = builder.Build();

// Middleware
app.UseMiddleware<Api.Back.Middleware.ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ProdPolicy");
    app.UseHsts();
}
app.UseCors("DevPolicy");


// // Utilise la bonne politique selon l'environnement
// if (app.Environment.IsDevelopment())
// {
//     app.UseCors("DevPolicy");
// }
// else
// {
//     app.UseCors("ProdPolicy");
// }
// Security
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/api/back/v1/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
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

app.Run();
#endregion