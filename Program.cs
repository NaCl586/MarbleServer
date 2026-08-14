using MarbleServer.Configuration;
using MarbleServer.Data;
using MarbleServer.Middleware;
using MarbleServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// Controllers
// =========================================================

builder.Services.AddControllers();

// =========================================================
// Swagger
// =========================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MarbleServer API",
        Version = "v1"
    });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Description =
                "Masukkan JWT Token",

            Name =
                "Authorization",

            In =
                ParameterLocation.Header,

            Type =
                SecuritySchemeType.Http,

            Scheme =
                "bearer",

            BearerFormat =
                "JWT"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                },

                Array.Empty<string>()
            }
        });
});

// =========================================================
// Databases
// =========================================================

// =========================================================
// Main database
// =========================================================

builder.Services.AddDbContext<MarbleDbContext>(
    options =>
        options.UseSqlite(
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection")));

// =========================================================
// Leaderboard / rating reference database
//
// RatingReferenceData is a singleton, so it uses a
// DbContextFactory rather than directly consuming a
// scoped LeaderboardDbContext.
// =========================================================

builder.Services.AddPooledDbContextFactory<
    LeaderboardDbContext>(
    options =>
        options.UseSqlite(
            builder.Configuration
                .GetConnectionString(
                    "LeaderboardConnection")));

// =========================================================
// Configuration
// =========================================================

builder.Services.Configure<ReplaySettings>(
    builder.Configuration
        .GetSection("ReplayStorage"));

builder.Services.Configure<JwtSettings>(
    builder.Configuration
        .GetSection("Jwt"));

JwtSettings jwtSettings =
    builder.Configuration
        .GetSection("Jwt")
        .Get<JwtSettings>()!;

// =========================================================
// Authentication
// =========================================================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer =
                    true,

                ValidateAudience =
                    true,

                ValidateLifetime =
                    true,

                ValidateIssuerSigningKey =
                    true,

                ValidIssuer =
                    jwtSettings.Issuer,

                ValidAudience =
                    jwtSettings.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings.Secret))
            };

        options.Events =
            new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    string? accessToken =
                        context.Request.Query[
                            "access_token"];

                    PathString path =
                        context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(
                            accessToken) &&
                        path.StartsWithSegments(
                            "/hubs/chat"))
                    {
                        context.Token =
                            accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
    });

builder.Services.AddAuthorization();

// =========================================================
// Application Services
// =========================================================

// Main application services
builder.Services.AddScoped<ScoreService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<ReplayService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<IntegrityService>();

// =========================================================
// Rating services
// =========================================================

// Loads reference data from leaderboards.db once at startup
// and keeps it in memory.
builder.Services.AddSingleton<RatingReferenceData>();

// Performs rating calculations using the cached reference data.
builder.Services.AddSingleton<RatingService>();

// Uses MarbleDbContext, so this remains scoped.
builder.Services.AddScoped<GlobalRatingService>();

// =========================================================
// SignalR
// =========================================================

builder.Services.AddSignalR();

builder.Services.AddSingleton<PresenceService>();

builder.Services.AddSingleton<
    ChatMessageHistoryService>();

// =========================================================
// Build
// =========================================================

var app = builder.Build();

// =========================================================
// Database Initialization
// =========================================================

using (var scope =
    app.Services.CreateScope())
{
    // =====================================================
    // Main database
    // =====================================================

    var db =
        scope.ServiceProvider
            .GetRequiredService<MarbleDbContext>();

    db.Database.Migrate();

    // =====================================================
    // Load leaderboard/rating reference data
    // =====================================================

    var ratingReferenceData =
        scope.ServiceProvider
            .GetRequiredService<RatingReferenceData>();

    await ratingReferenceData
        .InitializeAsync();

    // =====================================================
    // Recalculate stored mission ratings
    //
    // This runs ONCE when the server process starts.
    //
    // It does NOT run for every player/session.
    // =====================================================

    var globalRatingService =
        scope.ServiceProvider
            .GetRequiredService<GlobalRatingService>();

    await globalRatingService
        .BackfillMissionRatingsAsync();
}

// =========================================================
// Swagger
// =========================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =========================================================
// HTTPS
// =========================================================

app.UseHttpsRedirection();

// =========================================================
// Authentication & Authorization
// =========================================================

app.UseAuthentication();
app.UseAuthorization();

// =========================================================
// Exception Middleware
// =========================================================

app.UseMiddleware<ExceptionMiddleware>();

// =========================================================
// SignalR
// =========================================================

app.MapHub<ChatHub>(
    "/hubs/chat");

// =========================================================
// Controllers
// =========================================================

app.MapControllers();

// =========================================================
// Run
// =========================================================

app.Run();