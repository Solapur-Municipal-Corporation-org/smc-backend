using System.Text;
using CitizenPortal.Api.Data;
using CitizenPortal.Api.Department.Interfaces;
using CitizenPortal.Api.Department.Repositories;
using CitizenPortal.Api.Department.Services;
using CitizenPortal.Api.Middleware;
using CitizenPortal.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Keep local runs independent of Windows Event Log permissions. Console output is
// already captured by dotnet run and works for both developer terminals and CI.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ---- Services ----
builder.Services.AddScoped<AuditLogFilter>();
builder.Services.AddControllers(options =>
{
    // Records every write (POST/PUT/DELETE) under /api/department/* to AuditLogs (spec: keep
    // an audit trail of who changed what). GET requests are ignored inside the filter itself.
    options.Filters.Add<AuditLogFilter>();
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SMC Citizen Portal API", Version = "v1" });
    var jwtScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Enter your JWT token as: Bearer {token}",
        Reference = new OpenApiReference { Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme }
    };
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtScheme, Array.Empty<string>() } });
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Connection string 'DefaultConnection' is missing.");
    }

    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
    });

});

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IApplicationNumberGenerator, ApplicationNumberGenerator>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<CitizenPortal.Api.Department.Services.ISmsService, CitizenPortal.Api.Department.Services.ConsoleSmsService>();

// ---- Department Portal (merged in) ----
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDepartmentLinkService, DepartmentLinkService>();
builder.Services.AddScoped<ISmsService, ConsoleSmsService>(); // swap for a real SMS gateway later

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSection = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!)),
        ClockSkew = TimeSpan.FromMinutes(2),
    };
});

builder.Services.AddAuthorization(options =>
{
    // Used by every write endpoint under Department/Controllers (masters, users, employees).
    // "AdminOnly" here means SystemAdmin OR DepartmentAdmin — i.e. any staff role above plain
    // DepartmentEmployee. Endpoints that must be SystemAdmin-only (cross-department writes)
    // additionally call EnsureDepartmentAccess()/IsSystemAdmin themselves — see
    // DepartmentScopedControllerBase.
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(
            CitizenPortal.Api.Department.Entities.UserRole.SystemAdmin.ToString(),
            CitizenPortal.Api.Department.Entities.UserRole.DepartmentAdmin.ToString()));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CitizenPortalCors", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                              ?? new[] { "http://localhost:3000" };
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ---- Seed database on startup (safe to run repeatedly) ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

// Employee document uploads (UploadsController) need this folder to exist.
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads", "employee-documents"));

// ---- Middleware pipeline ----
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SMC Citizen Portal API v1"));
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("CitizenPortalCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
