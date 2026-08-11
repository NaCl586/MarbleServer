using MarbleServer.Configuration;
using MarbleServer.Data;
using MarbleServer.Middleware;
using MarbleServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MarbleServer API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Masukkan JWT Token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<MarbleDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuration

builder.Services.Configure<ReplaySettings>(
    builder.Configuration.GetSection("ReplayStorage"));

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

JwtSettings jwtSettings =
    builder.Configuration
        .GetSection("Jwt")
        .Get<JwtSettings>()!;

// Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret))
            };
    });

builder.Services.AddAuthorization();

// Dependency Injection
builder.Services.AddScoped<ScoreService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<ReplayService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<LeaderboardService>();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS
app.UseHttpsRedirection();

// Authentication & Authorization

app.UseAuthentication();
app.UseAuthorization();

// Exception Middleware
app.UseMiddleware<ExceptionMiddleware>();

// Controllers
app.MapControllers();

app.Run();