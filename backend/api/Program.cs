using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using QRAlbums.API.Data;
using QRAlbums.API.Services;
using QRAlbums.API.Controllers;
using QRAlbums.API.Options;
using DotNetEnv;

// Load environment variables
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add configuration from environment variables
builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<BunnyOptions>(builder.Configuration.GetSection("Bunny"));

// Add services
builder.Services.AddDbContext<QRAlbumsContext>(opts =>
{
    var cs = builder.Configuration.GetConnectionString("Default") ??
             Environment.GetEnvironmentVariable("ConnectionStrings__Default");
    if (string.IsNullOrWhiteSpace(cs))
        throw new InvalidOperationException("DB connection string not configured");
    opts.UseMySql(cs, ServerVersion.AutoDetect(cs));
});

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IMediaService, MediaService>();
builder.Services.AddScoped<ISelectionService, SelectionService>();
builder.Services.AddHttpClient<IBunnyService, BunnyService>();
builder.Services.AddScoped<IQRService, QRService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobile", policy =>
    {
        policy.WithOrigins(
                "capacitor://localhost",
                "http://localhost",
                "http://localhost:8100",
                "https://localhost:8100",
                builder.Configuration["Frontend:ViewerBaseUrl"] ?? "https://viewer.qralbums.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowMobile");
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapAuthEndpoints();
app.MapProjectEndpoints();
app.MapAlbumEndpoints();
app.MapMediaEndpoints();
app.MapViewerEndpoints();
app.MapSelectionEndpoints();

app.Run();