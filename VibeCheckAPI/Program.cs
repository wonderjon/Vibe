using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VibeCheck.DataAcces.Persistence;
using VibeCheck.DataAcces.Repositories;
using VibeCheck.Domain.Entities;
using VibeCheck.Service.Implementations;
using VibeCheck.Service.Interfaces;
using VibeCheck.Service.Security;
using VibeCheck.Service.Seeding;
using VibeCheck.Service.Storage;
using VibeCheck.Service.Validators;
using VibeCheckAPI.Middleware;

// wwwroot must exist before WebApplication.CreateBuilder resolves the static-file provider,
// otherwise it falls back to a null provider for the lifetime of the app. The content root at
// this point is the current directory, matching WebApplicationBuilder's own default.
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads"));

var builder = WebApplication.CreateBuilder(args);

// ---------- Options ----------
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<StorageOptions>(options =>
{
    options.RootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads");
    options.PublicBaseUrl = "/uploads";
});

// ---------- Database ----------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// ---------- Data access ----------
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ---------- Security ----------
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();

// ---------- Application services ----------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVenueService, VenueService>();
builder.Services.AddScoped<IVibeCheckService, VibeCheckService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// ---------- Validation ----------
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// ---------- Auth ----------
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                string.IsNullOrEmpty(jwtOptions.Key) ? "dev-only-placeholder-key-replace-me-1234567890" : jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// ---------- CORS ----------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (allowedOrigins.Length > 0)
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        else
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// ---------- MVC / Swagger ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "VibeCheck API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Paste just the access token — the 'Bearer' prefix is added automatically."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------- Dev-time migrate & seed (never fails startup if no DB is reachable yet) ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        await DbSeeder.SeedAsync(db);

        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
        var superAdminEmail = builder.Configuration["SuperAdmin:Email"] ?? string.Empty;
        var superAdminPassword = builder.Configuration["SuperAdmin:Password"] ?? string.Empty;
        await SuperAdminSeeder.SeedAsync(db, passwordHasher, superAdminEmail, superAdminPassword);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Skipping database migration/seed — no database connection available.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseApiExceptionHandling();

app.UseHttpsRedirection();

app.UseCors("Frontend");

// Serve uploaded media (avatars, venue covers, vibe check photos) as static files.
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
