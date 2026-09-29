using System.Security.Claims;
using CitizenPortal.Api.Controllers;
using CitizenPortal.Api.Data;
using CitizenPortal.Api.DTOs;
using CitizenPortal.Api.Models;
using CitizenPortal.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace SMC.Master.API.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task GetMine_ReturnsApplicationsForTheAuthenticatedCitizen()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();

        var citizenId = Guid.NewGuid();
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = "ENG",
            Name = "Engineering",
            DepartmentDescription = "Citizen"
        };
        var service = new Service
        {
            Id = Guid.NewGuid(),
            DepartmentId = department.Id,
            Department = department,
            Name = "Water Connection",
            Fee = 100m,
            Description = "Test service"
        };

        db.Citizens.Add(new Citizen
        {
            Id = citizenId,
            FirstName = "Jane",
            LastName = "Doe",
            MobileNumber = "9999999999",
            Email = "jane@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Female",
            AddressLine1 = "Main Street",
            City = "Pune",
            State = "MH",
            Pincode = "411001",
            AadhaarNumber = "123456789012"
        });

        db.Departments.Add(department);
        db.Services.Add(service);
        db.Applications.Add(new Application
        {
            Id = Guid.NewGuid(),
            ApplicationNumber = "ENG-2025-001",
            CitizenId = citizenId,
            ServiceId = service.Id,
            Service = service,
            Status = ApplicationStatus.Pending,
            FinancialYear = "2025-26",
            FormDataJson = "{\"field\":\"value\"}",
            SubmittedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var controller = new ApplicationsController(
            db,
            new FakeApplicationNumberGenerator(),
            new FakeFileStorageService(),
            new FakeCertificateService(),
            new ConfigurationBuilder().AddInMemoryCollection().Build());

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, citizenId.ToString()),
                    new Claim("mobileNumber", "9999999999")
                ], "Test"))
            }
        };

        var result = await controller.GetMine();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var applications = Assert.IsType<List<ApplicationResponse>>(ok.Value);
        Assert.Single(applications);
        Assert.Equal("Water Connection", applications[0].ServiceName);
        Assert.Equal("ENG-2025-001", applications[0].ApplicationNumber);
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyCitizenDepartmentEntries()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();

        db.Departments.AddRange(
            new Department { Code = "CIT", Name = "Citizen Services", DepartmentDescription = "Citizen", DisplayOrder = 1 },
            new Department { Code = "ADM", Name = "Admin Services", DepartmentDescription = "Internal", DisplayOrder = 2 }
        );

        await db.SaveChangesAsync();

        var controller = new DepartmentsController(
            db,
            new ConfigurationBuilder().AddInMemoryCollection().Build());
        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var departments = Assert.IsType<List<DepartmentResponse>>(ok.Value);
        var departmentNames = departments.Select(d => d.Name).ToList();

        Assert.Single(departments);
        Assert.Contains("Citizen Services", departmentNames);
        Assert.DoesNotContain("Admin Services", departmentNames);
    }

    [Fact]
    public async Task RegisterThenLogin_FindsCitizenByMobileNumberInAppDatabase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();

        var controller = new AuthController(
            db,
            new FakeJwtTokenService(),
            new FakeFileStorageService(),
            new ConfigurationBuilder().AddInMemoryCollection().Build());

        var registerRequest = new RegisterRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            MobileNumber = "9876543210",
            Email = "jane@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Female",
            AddressLine1 = "Main Street",
            City = "Pune",
            State = "MH",
            Pincode = "411001",
            AadhaarNumber = "123456789012"
        };

        var registerResult = await controller.Register(registerRequest);
        Assert.IsType<CreatedAtActionResult>(registerResult.Result);

        var loginResult = await controller.Login(new LoginRequest { MobileNumber = "9876543210" });
        var ok = Assert.IsType<OkObjectResult>(loginResult.Result);
        var auth = Assert.IsType<AuthResponse>(ok.Value);
        Assert.Equal("9876543210", auth.Citizen.MobileNumber);
    }

    [Fact]
    public void EntityMappings_UseRealSmcDbTables()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        using var db = new AppDbContext(options);

        var citizenEntity = db.Model.FindEntityType(typeof(Citizen));
        var departmentEntity = db.Model.FindEntityType(typeof(Department));
        var serviceEntity = db.Model.FindEntityType(typeof(Service));
        var applicationEntity = db.Model.FindEntityType(typeof(Application));

        Assert.NotNull(citizenEntity);
        Assert.Equal("MR_DEPT_Citizens", citizenEntity!.GetTableName());
        Assert.Equal("MR_DEPT_Departments", departmentEntity!.GetTableName());
        Assert.Equal("TR_CFC_Services", serviceEntity!.GetTableName());
        Assert.Equal("TR_CFC_Applications", applicationEntity!.GetTableName());
    }

    private sealed class FakeApplicationNumberGenerator : IApplicationNumberGenerator
    {
        public string Generate(string departmentCode) => "ENG-2025-001";
        public string CurrentFinancialYear() => "2025-26";
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public (string token, DateTime expiresAt) GenerateToken(Citizen citizen) =>
            ($"test-token-{citizen.MobileNumber}", DateTime.UtcNow.AddHours(1));
    }

    private sealed class FakeFileStorageService : IFileStorageService
    {
        public Task<string> SaveAsync(IFormFile file, string subFolder) => Task.FromResult("uploads/test-file.pdf");
    }

    private sealed class FakeCertificateService : ICertificateService
    {
        public byte[] GenerateCertificatePdf(Application application, Citizen citizen, string serviceName, string departmentName) => [];
    }
}
