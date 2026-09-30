using System.Text;
using System.Text.Json.Serialization;
using BDMS.Application.Services;
using BDMS.Domain.Models;
using BDMS.Infrastructure.Data;
using BDMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");

builder.Services.AddDbContext<BdmsDbContext>(options =>
{
    if (useInMemoryDatabase)
    {
        options.UseInMemoryDatabase("BdmsDb");
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("BdmsDb"));
    }
});

// Dummy service implementations -- swap for real SMS/email + payment gateway later
// by registering different classes against the same interfaces.
builder.Services.AddScoped<IOtpService, DummyOtpService>();
builder.Services.AddScoped<IPaymentService, DummyPaymentService>(); // not used by the Birth flow itself (legacy has no payment step here); kept for future modules (offline receipts etc.)
builder.Services.AddScoped<IBirthApplicationValidator, BirthApplicationValidator>();
builder.Services.AddScoped<IApplicationNumberService, ApplicationNumberService>();
builder.Services.AddScoped<IPasswordCipher, LegacyPasswordCipher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOfficerVerificationService, OfficerVerificationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PortalClients", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",  // citizen portal (Next.js)
                "http://localhost:3001",  // citizen portal (Next.js alternate dev port)
                "http://localhost:3002",  // citizen portal (Next.js alternate dev port)
                "http://localhost:3003",  // citizen portal (Next.js alternate dev port)
                "https://localhost:3000",
                "https://localhost:3001",
                "https://localhost:3002",
                "https://localhost:3003",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:3001",
                "http://127.0.0.1:3002",
                "http://127.0.0.1:3003",
                "http://localhost:5173",  // admin/clerk portal (React/Vite)
                "http://localhost:5174",  // alternate Vite dev port
                "http://127.0.0.1:5173",
                "http://127.0.0.1:5174")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// JWT auth -- replaces legacy's FormsAuthenticationTicket cookie (which also carried a
// 30-minute expiry and the user's role). Signing key/issuer/audience come from appsettings.json.
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("PortalClients");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-migrate on startup in dev for convenience. Remove for production; use `dotnet ef
// database update` / a proper migrations pipeline instead.
app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BdmsDbContext>();

        try
        {
            await db.Database.EnsureCreatedAsync();
            await RemoveOtpTableAsync(db);
            await EnsureDocumentUploadsTableAsync(db);
            await EnsureBirthApplicationTablesAsync(db);
            await EnsureBirthApplicationColumnsAsync(db);
            await EnsureDeathApplicationTablesAsync(db);
            await EnsureDeathApplicationColumnsAsync(db);

            // Disable retired Officer/Admin accounts and seed the Clerk account for local demo use.
            // Passwords are run through the exact legacy cipher so login behaves identically to the
            // real system. Change/remove these before any real deployment.
            var retiredOfficerAccounts = await db.Users
                .Where(u => u.UserRole == "Admin" && u.IsActive)
                .ToListAsync();
            foreach (var account in retiredOfficerAccounts)
                account.IsActive = false;

            if (await db.Users.AnyAsync(u => u.UserRole == "Operator"))
            {
                await db.SaveChangesAsync();
                return;
            }

            var cipher = scope.ServiceProvider.GetRequiredService<IPasswordCipher>();
            db.Users.Add(new User
            {
                FirstName = "Demo", LastName = "Clerk", UserName = "clerk1",
                PasswordEncrypted = cipher.Encrypt("Clerk@123"),
                UserRole = "Operator", Dept = "Birth & Death Registration"
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Database initialization is unavailable; continuing without database seeding so the API can still serve non-persistence endpoints.");
        }
    });
});

async Task RemoveOtpTableAsync(BdmsDbContext db)
{
    if (db.Database.IsInMemory())
        return;

    await db.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID('dbo.OtpRecords', 'U') IS NOT NULL
    DROP TABLE dbo.OtpRecords;");
}

async Task EnsureDocumentUploadsTableAsync(BdmsDbContext db)
{
    if (db.Database.IsInMemory())
        return;

    await db.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID('dbo.DocumentUploads', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.DocumentUploads (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TempApplicationNumber NVARCHAR(100) NOT NULL,
        DocumentKey NVARCHAR(50) NOT NULL,
        FileName NVARCHAR(255) NOT NULL,
        ContentType NVARCHAR(100) NOT NULL,
        Content VARBINARY(MAX) NOT NULL,
        UploadedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_DocumentUploads_TempDocument UNIQUE (TempApplicationNumber, DocumentKey)
    );
END;");
}

async Task EnsureBirthApplicationTablesAsync(BdmsDbContext db)
{
    if (db.Database.IsInMemory())
        return;

    await db.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID('dbo.TempBirthApplications', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.TempBirthApplications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TempApplicationNumber NVARCHAR(450) NOT NULL,
        FatherName NVARCHAR(MAX) NOT NULL, FatherAadharNumber NVARCHAR(MAX) NOT NULL, FatherMobileNumber NVARCHAR(MAX) NOT NULL, FatherEmail NVARCHAR(MAX) NOT NULL, FatherAadharDocUploaded BIT NOT NULL DEFAULT 0,
        MotherName NVARCHAR(MAX) NOT NULL, MotherAadharNumber NVARCHAR(MAX) NOT NULL, MotherMobileNumber NVARCHAR(MAX) NOT NULL, MotherAadharDocUploaded BIT NOT NULL DEFAULT 0,
        ChildNameEnglish NVARCHAR(MAX) NOT NULL, ChildNameMarathi NVARCHAR(MAX) NULL, ChildBirthDate DATETIME2 NOT NULL, ChildBirthPlace NVARCHAR(MAX) NOT NULL, ChildGender INT NOT NULL, PermanentAddress NVARCHAR(MAX) NOT NULL, VartaNumber NVARCHAR(MAX) NULL, JanmAhawalDocUploaded BIT NOT NULL DEFAULT 0,
        IsAppliedAfterFifteenYears INT NOT NULL, Navnondni INT NOT NULL,
        LcDocUploaded BIT NOT NULL DEFAULT 0, SscCertDocUploaded BIT NOT NULL DEFAULT 0, PanCardDocUploaded BIT NOT NULL DEFAULT 0, VoterIdDocUploaded BIT NOT NULL DEFAULT 0, AadharCardDocUploaded BIT NOT NULL DEFAULT 0, VahanParvanaDocUploaded BIT NOT NULL DEFAULT 0, GovtIdCardDocUploaded BIT NOT NULL DEFAULT 0,
        EntryDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), UserName NVARCHAR(MAX) NOT NULL DEFAULT 'Online', PaymentMadeYesNo INT NOT NULL DEFAULT 0, MacAddress NVARCHAR(MAX) NULL, ApplicationTypeValue INT NOT NULL DEFAULT 0,
        AckId NVARCHAR(MAX) NULL, AckSubject NVARCHAR(MAX) NOT NULL, PaidAmount DECIMAL(10,2) NOT NULL DEFAULT 0, AmountToPay DECIMAL(10,2) NOT NULL DEFAULT 0, DakhalaFee DECIMAL(10,2) NOT NULL DEFAULT 0, Penalty DECIMAL(10,2) NOT NULL DEFAULT 0,
        OtpVerified BIT NOT NULL DEFAULT 0, FinalizedToPermanent BIT NOT NULL DEFAULT 0
    );
    CREATE UNIQUE INDEX IX_TempBirthApplications_TempApplicationNumber ON dbo.TempBirthApplications(TempApplicationNumber);
END;

IF OBJECT_ID('dbo.[TR_B&D_BirthApplications]', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.[TR_B&D_BirthApplications] (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ApplicationNumber NVARCHAR(450) NOT NULL, TempApplicationNumber NVARCHAR(MAX) NOT NULL,
        FatherName NVARCHAR(200) NOT NULL, FatherAadharNumber NVARCHAR(MAX) NOT NULL, FatherMobileNumber NVARCHAR(MAX) NOT NULL, FatherEmail NVARCHAR(MAX) NOT NULL, FatherAadharDocUploaded BIT NOT NULL DEFAULT 0,
        MotherName NVARCHAR(200) NOT NULL, MotherAadharNumber NVARCHAR(MAX) NOT NULL, MotherMobileNumber NVARCHAR(MAX) NOT NULL, MotherAadharDocUploaded BIT NOT NULL DEFAULT 0,
        ChildNameEnglish NVARCHAR(200) NOT NULL, ChildNameMarathi NVARCHAR(MAX) NULL, ChildBirthDate DATETIME2 NOT NULL, ChildBirthPlace NVARCHAR(300) NOT NULL, ChildGender INT NOT NULL, PermanentAddress NVARCHAR(MAX) NOT NULL, VartaNumber NVARCHAR(MAX) NULL, JanmAhawalDocUploaded BIT NOT NULL DEFAULT 0,
        IsAppliedAfterFifteenYears INT NOT NULL, Navnondni INT NOT NULL,
        LcDocUploaded BIT NOT NULL DEFAULT 0, SscCertDocUploaded BIT NOT NULL DEFAULT 0, PanCardDocUploaded BIT NOT NULL DEFAULT 0, VoterIdDocUploaded BIT NOT NULL DEFAULT 0, AadharCardDocUploaded BIT NOT NULL DEFAULT 0, VahanParvanaDocUploaded BIT NOT NULL DEFAULT 0, GovtIdCardDocUploaded BIT NOT NULL DEFAULT 0,
        EntryDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), UserName NVARCHAR(MAX) NOT NULL DEFAULT 'Online', PaymentMadeYesNo INT NOT NULL DEFAULT 0, MacAddress NVARCHAR(MAX) NULL, ApplicationTypeValue INT NOT NULL DEFAULT 0,
        AckId NVARCHAR(MAX) NULL, AckSubject NVARCHAR(MAX) NOT NULL, PaidAmount DECIMAL(10,2) NOT NULL DEFAULT 0, AmountToPay DECIMAL(10,2) NOT NULL DEFAULT 0, DakhalaFee DECIMAL(10,2) NOT NULL DEFAULT 0, Penalty DECIMAL(10,2) NOT NULL DEFAULT 0,
        VerifiedApplicationStatus INT NOT NULL DEFAULT 0, RecordFound INT NOT NULL DEFAULT 0, CrsMainetRegNo NVARCHAR(MAX) NULL, CrsMainetSourceValue INT NOT NULL DEFAULT 0,
        IsNamePreviouslyReportedByCitizen BIT NULL, IsNamePreviouslyReportedOperatorVerified INT NOT NULL DEFAULT 0, RegisteredChildName NVARCHAR(MAX) NULL, RegisteredChildBirthDate DATETIME2 NULL, OperatorRemark NVARCHAR(MAX) NULL,
        DocVerified BIT NOT NULL DEFAULT 0, AssignedToAbhilekhapalDate DATETIME2 NULL, OfficerVerifiedAt DATETIME2 NULL, SignedCertificateUploaded BIT NOT NULL DEFAULT 0, IssuedCertificateContent VARBINARY(MAX) NULL, IssuedCertificateFileName NVARCHAR(255) NULL, IssuedCertificateContentType NVARCHAR(100) NULL,
        RecordLocked BIT NOT NULL DEFAULT 0, RecordLockedByUserName NVARCHAR(MAX) NULL, RecordLockedAt DATETIME2 NULL
    );
    CREATE UNIQUE INDEX IX_BirthApplications_ApplicationNumber ON dbo.[TR_B&D_BirthApplications](ApplicationNumber);
END;");
}

async Task EnsureBirthApplicationColumnsAsync(BdmsDbContext db)
{
    if (db.Database.IsInMemory())
        return;

    var sql = @"
IF COL_LENGTH('dbo.TR_B&D_BirthApplications', 'IsNamePreviouslyReportedByCitizen') IS NULL
    ALTER TABLE dbo.[TR_B&D_BirthApplications] ADD IsNamePreviouslyReportedByCitizen BIT NULL;

IF COL_LENGTH('dbo.TR_B&D_BirthApplications', 'IsNamePreviouslyReportedOperatorVerified') IS NULL
    ALTER TABLE dbo.[TR_B&D_BirthApplications] ADD IsNamePreviouslyReportedOperatorVerified INT NOT NULL CONSTRAINT DF_BirthApplications_IsNamePreviouslyReportedOperatorVerified DEFAULT 0;

IF COL_LENGTH('dbo.TR_B&D_BirthApplications', 'RegisteredChildName') IS NULL
    ALTER TABLE dbo.[TR_B&D_BirthApplications] ADD RegisteredChildName NVARCHAR(MAX) NULL;

IF COL_LENGTH('dbo.TR_B&D_BirthApplications', 'RegisteredChildBirthDate') IS NULL
    ALTER TABLE dbo.[TR_B&D_BirthApplications] ADD RegisteredChildBirthDate DATETIME2 NULL;

IF COL_LENGTH('dbo.TR_B&D_BirthApplications', 'IssuedCertificateContent') IS NULL
    ALTER TABLE dbo.[TR_B&D_BirthApplications] ADD IssuedCertificateContent VARBINARY(MAX) NULL;

IF COL_LENGTH('dbo.TR_B&D_BirthApplications', 'IssuedCertificateFileName') IS NULL
    ALTER TABLE dbo.[TR_B&D_BirthApplications] ADD IssuedCertificateFileName NVARCHAR(255) NULL;

IF COL_LENGTH('dbo.TR_B&D_BirthApplications', 'IssuedCertificateContentType') IS NULL
    ALTER TABLE dbo.[TR_B&D_BirthApplications] ADD IssuedCertificateContentType NVARCHAR(100) NULL;
";

    await db.Database.ExecuteSqlRawAsync(sql);
}

async Task EnsureDeathApplicationTablesAsync(BdmsDbContext db)
{
    if (db.Database.IsInMemory()) return;
    await db.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID('dbo.DeathApplications', 'U') IS NULL
CREATE TABLE dbo.DeathApplications (
 Id INT IDENTITY(1,1) PRIMARY KEY, ApplicationNumber NVARCHAR(450) NOT NULL, TempApplicationNumber NVARCHAR(MAX) NOT NULL,
 ApplicantName NVARCHAR(200) NOT NULL, ApplicantAadharNumber NVARCHAR(MAX) NOT NULL, ApplicantMobileNumber NVARCHAR(MAX) NOT NULL, PermanentAddress NVARCHAR(MAX) NOT NULL, Email NVARCHAR(MAX) NOT NULL,
 DeadPersonName NVARCHAR(200) NOT NULL, Gender INT NOT NULL, DeathDate DATETIME2 NOT NULL, DeathPlace NVARCHAR(300) NOT NULL, DeadPersonAadharNumber NVARCHAR(MAX) NOT NULL, MotherName NVARCHAR(200) NOT NULL, FatherHusbandName NVARCHAR(200) NOT NULL,
 ApplicantAadharDocUploaded BIT NOT NULL DEFAULT 0, DeathProofDocUploaded BIT NOT NULL DEFAULT 0, EntryDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), UserName NVARCHAR(MAX) NOT NULL DEFAULT 'Online', PaymentMadeYesNo INT NOT NULL DEFAULT 0, ApplicationTypeValue INT NOT NULL DEFAULT 1, AckSubject NVARCHAR(MAX) NOT NULL DEFAULT 'Death Certificate', VerifiedApplicationStatus INT NOT NULL DEFAULT 0, RecordFound INT NOT NULL DEFAULT 0, CrsMainetRegNo NVARCHAR(MAX) NULL, CrsMainetSourceValue INT NOT NULL DEFAULT 0, OperatorRemark NVARCHAR(MAX) NULL, SignedCertificateUploaded BIT NOT NULL DEFAULT 0, OfficerVerifiedAt DATETIME2 NULL, RecordLocked BIT NOT NULL DEFAULT 0, RecordLockedByUserName NVARCHAR(MAX) NULL, RecordLockedAt DATETIME2 NULL);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DeathApplications_ApplicationNumber') CREATE UNIQUE INDEX IX_DeathApplications_ApplicationNumber ON dbo.DeathApplications(ApplicationNumber);
IF OBJECT_ID('dbo.TempDeathApplications', 'U') IS NULL
CREATE TABLE dbo.TempDeathApplications (
 Id INT IDENTITY(1,1) PRIMARY KEY, ApplicationNumber NVARCHAR(MAX) NOT NULL, TempApplicationNumber NVARCHAR(MAX) NOT NULL, TempApplicationNumberValue NVARCHAR(450) NOT NULL,
 ApplicantName NVARCHAR(200) NOT NULL, ApplicantAadharNumber NVARCHAR(MAX) NOT NULL, ApplicantMobileNumber NVARCHAR(MAX) NOT NULL, PermanentAddress NVARCHAR(MAX) NOT NULL, Email NVARCHAR(MAX) NOT NULL,
 DeadPersonName NVARCHAR(200) NOT NULL, Gender INT NOT NULL, DeathDate DATETIME2 NOT NULL, DeathPlace NVARCHAR(300) NOT NULL, DeadPersonAadharNumber NVARCHAR(MAX) NOT NULL, MotherName NVARCHAR(200) NOT NULL, FatherHusbandName NVARCHAR(200) NOT NULL,
 ApplicantAadharDocUploaded BIT NOT NULL DEFAULT 0, DeathProofDocUploaded BIT NOT NULL DEFAULT 0, EntryDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(), UserName NVARCHAR(MAX) NOT NULL DEFAULT 'Online', PaymentMadeYesNo INT NOT NULL DEFAULT 0, ApplicationTypeValue INT NOT NULL DEFAULT 1, AckSubject NVARCHAR(MAX) NOT NULL DEFAULT 'Death Certificate', VerifiedApplicationStatus INT NOT NULL DEFAULT 0, RecordFound INT NOT NULL DEFAULT 0, CrsMainetRegNo NVARCHAR(MAX) NULL, CrsMainetSourceValue INT NOT NULL DEFAULT 0, OperatorRemark NVARCHAR(MAX) NULL, SignedCertificateUploaded BIT NOT NULL DEFAULT 0, OfficerVerifiedAt DATETIME2 NULL, RecordLocked BIT NOT NULL DEFAULT 0, RecordLockedByUserName NVARCHAR(MAX) NULL, RecordLockedAt DATETIME2 NULL, OtpVerified BIT NOT NULL DEFAULT 0, FinalizedToPermanent BIT NOT NULL DEFAULT 0);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TempDeathApplications_TempApplicationNumberValue') CREATE UNIQUE INDEX IX_TempDeathApplications_TempApplicationNumberValue ON dbo.TempDeathApplications(TempApplicationNumberValue);");
}

async Task EnsureDeathApplicationColumnsAsync(BdmsDbContext db)
{
    if (db.Database.IsInMemory()) return;
    await db.Database.ExecuteSqlRawAsync(@"
IF COL_LENGTH('dbo.DeathApplications', 'RegisteredDeathDate') IS NULL ALTER TABLE dbo.DeathApplications ADD RegisteredDeathDate DATETIME2 NULL;
IF COL_LENGTH('dbo.TempDeathApplications', 'RegisteredDeathDate') IS NULL ALTER TABLE dbo.TempDeathApplications ADD RegisteredDeathDate DATETIME2 NULL;");
}

app.Run();
