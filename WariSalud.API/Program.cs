using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WariSalud.API.Middleware;
using WariSalud.Core.Interfaces;
using WariSalud.Core.Services;
using WariSalud.Infrastructure.Persistence;
using WariSalud.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ─── Base de datos — PostgreSQL (Supabase) ────────────────────────────────────
// En producción (Render) la variable de entorno "ConnectionStrings__DefaultConnection"
// sobrescribe el valor de appsettings.json automáticamente.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no está configurada.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ─── Repositorios (Infrastructure → Core) ────────────────────────────────────
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IMedicoRepository, MedicoRepository>();
builder.Services.AddScoped<ICitaRepository, CitaRepository>();
builder.Services.AddScoped<IEspecialidadRepository, EspecialidadRepository>();
builder.Services.AddScoped<IConfiguracionClinicaRepository, ConfiguracionClinicaRepository>();

// ─── Servicios de dominio (Core) ─────────────────────────────────────────────
builder.Services.AddScoped<ICitaService, CitaService>();

// ─── JWT Bearer Authentication ────────────────────────────────────────────────
// En Render: Environment Variable → Jwt__Key = <tu-clave-secreta>
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key no está configurada. Agrégala como variable de entorno en Render.");

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ─── Controladores ────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─── Swagger / OpenAPI ────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "WariSalud API",
        Version = "v1",
        Description = "Sistema de Gestión de Citas Médicas — Backend REST API"
    });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese el token JWT: Bearer {token}"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─── CORS ─────────────────────────────────────────────────────────────────────
// En Render: Environment Variable → AllowedOrigins = https://tu-frontend.onrender.com
// En desarrollo: se usan localhost automáticamente
var allowedOrigins = builder.Configuration["AllowedOrigins"]
    ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? [];

var devOrigins = new[]
{
    "http://localhost:5173",
    "http://localhost:4173",
    "https://localhost:5173",
    "https://localhost:4173"
};

var allOrigins = builder.Environment.IsDevelopment()
    ? devOrigins.Concat(allowedOrigins).ToArray()
    : allowedOrigins;

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        if (allOrigins.Length > 0)
            policy.WithOrigins(allOrigins);
        else
            policy.AllowAnyOrigin(); // fallback solo si no hay origenes configurados

        policy.AllowAnyHeader().AllowAnyMethod();
        // AllowCredentials() es incompatible con AllowAnyOrigin()
        if (allOrigins.Length > 0)
            policy.AllowCredentials();
    });
});

// ─── Build ────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ─── Middleware de excepciones — PRIMERO en el pipeline ───────────────────────
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// ─── CORS ────────────────────────────────────────────────────────────────────
app.UseCors("FrontendPolicy");

// ─── Swagger (solo en desarrollo) ────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WariSalud API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// ─── Auth pipeline ────────────────────────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

// ─── Controladores ────────────────────────────────────────────────────────────
app.MapControllers();

// ─── Migración y Seed automáticos (desarrollo Y producción) ──────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedAsync(db);
}

app.Run();

// Exposición del tipo para integration tests
public partial class Program { }

