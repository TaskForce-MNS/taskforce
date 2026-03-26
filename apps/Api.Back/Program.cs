using Api.Back.Data;
using Api.Back.Repositories;
using Api.Back.Services;
using Api.Back.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

#region SERVICES 
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c => c.EnableAnnotations());

// Repos & Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordValidator, PasswordValidator>();

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
#endregion

#region AUTHENTICATION
builder.Services.AddAuthentication("JwtBearer") // ou "Cookies" selon ton choix
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
        policy.WithOrigins("http://localhost:5173", "http://localhost:4321")
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevPolicy");
}
else
{
    app.UseCors("ProdPolicy");
    app.UseHsts();
}
// Security
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/back/v1/health", () => new { message = "L'API TaskForce est en bonne santé !" });

app.Run();
#endregion