using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SMC.Master.Application.Interfaces;
using SMC.Master.Application.Services;
using SMC.Master.Infrastructure.Authentication;
using SMC.Master.Infrastructure.Data;
using SMC.Master.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Database — SQL Server via EF Core, Windows Authentication (Trusted_Connection)
// matching the rest of the SMC portal stack.
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MasterDb")));

// Auth
var jwtSecret = builder.Configuration["Jwt:SecretKey"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtExpiry = int.Parse(builder.Configuration["Jwt:ExpiryMinutes"] ?? "1440");

builder.Services.AddSingleton(new JwtTokenService(jwtSecret, jwtIssuer, jwtExpiry));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtIssuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime = true,
        };
    });

builder.Services.AddAuthorization();

// Repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<DepartmentRepository>();
builder.Services.AddScoped<ServiceRepository>();
builder.Services.AddScoped<ApplicationRepository>();
builder.Services.AddScoped<TransactionRepository>();

// Application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICitizenService, CitizenService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");

app.UseMiddleware<SMC.Master.API.Middleware.ExceptionMiddleware>();
app.UseMiddleware<SMC.Master.API.Middleware.AuditMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
