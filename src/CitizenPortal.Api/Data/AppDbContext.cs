using System.Reflection;
using CitizenPortal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CitizenPortal.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ---- Core / master tables (owned by the platform, don't edit casually) ----
    public DbSet<Citizen> Citizens => Set<Citizen>();
    public DbSet<Models.Department> Departments => Set<Models.Department>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceField> ServiceFields => Set<ServiceField>();
    public DbSet<ServiceDocument> ServiceDocuments => Set<ServiceDocument>();
    public DbSet<ApplicationType> ApplicationTypes => Set<ApplicationType>();
    public DbSet<ApplicantType> ApplicantTypes => Set<ApplicantType>();
    public DbSet<Application> Applications => Set<Application>();
    public DbSet<ApplicationDocument> ApplicationDocuments => Set<ApplicationDocument>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();

    // ---- MODULES: department-specific tables go here ----
    // Every new department module that needs its own custom table (beyond the
    // generic Service/ServiceField/Application.FormDataJson system) registers its
    // DbSet here, one line per module. See /Modules/_TEMPLATE for the pattern and
    // /INTEGRATION_GUIDE.md for the full explanation.
    //
    // Example (real, working — see Modules/WTD):
    public DbSet<Modules.Wtd.WaterConnectionInspection> WaterConnectionInspections => Set<Modules.Wtd.WaterConnectionInspection>();

    // ================================================================================
    // DEPARTMENT PORTAL (staff/HR/masters side) — merged in from the standalone
    // Department Portal package. See INTEGRATION_REPORT.md for the full rationale.
    // Every table below ALREADY EXISTED in smc_db before this merge (created previously
    // by that package's own EF Core migrations) — nothing here creates a new table
    // except the ones explicitly called out as new below.
    // ================================================================================

    // -- Org / department / service / user masters (staff side, int-keyed) --
    public DbSet<Department.Entities.Organization> Organizations => Set<Department.Entities.Organization>();
    public DbSet<Department.Entities.StaffDepartment> StaffDepartments => Set<Department.Entities.StaffDepartment>();
    public DbSet<Department.Entities.StaffService> StaffServices => Set<Department.Entities.StaffService>();
    public DbSet<Department.Entities.AppUser> Users => Set<Department.Entities.AppUser>();
    public DbSet<Department.Entities.CitizenApplication> StaffApplications => Set<Department.Entities.CitizenApplication>();
    public DbSet<Department.Entities.AuditLog> AuditLogs => Set<Department.Entities.AuditLog>();

    // -- Employee / HR module --
    public DbSet<Department.Entities.Employee> Employees => Set<Department.Entities.Employee>();
    public DbSet<Department.Entities.EmployeeAddress> EmployeeAddresses => Set<Department.Entities.EmployeeAddress>(); // NEW table
    public DbSet<Department.Entities.EmployeeBankDetail> EmployeeBankDetails => Set<Department.Entities.EmployeeBankDetail>(); // NEW table
    public DbSet<Department.Entities.EmployeeSalary> EmployeeSalaries => Set<Department.Entities.EmployeeSalary>(); // NEW table
    public DbSet<Department.Entities.Education> Educations => Set<Department.Entities.Education>();
    public DbSet<Department.Entities.Family> Families => Set<Department.Entities.Family>();
    public DbSet<Department.Entities.EmployeeLeaveBalance> EmployeeLeaveBalances => Set<Department.Entities.EmployeeLeaveBalance>();
    public DbSet<Department.Entities.EmployeeDocument> EmployeeDocuments => Set<Department.Entities.EmployeeDocument>();

    // -- Generic masters (Country/State/District/Tahsil/City/Location/Address/FinancialYear/Holiday) --
    public DbSet<Department.Entities.Country> Countries => Set<Department.Entities.Country>();
    public DbSet<Department.Entities.State> States => Set<Department.Entities.State>();
    public DbSet<Department.Entities.District> Districts => Set<Department.Entities.District>();
    public DbSet<Department.Entities.Tahsil> Tahsils => Set<Department.Entities.Tahsil>();
    public DbSet<Department.Entities.City> Cities => Set<Department.Entities.City>();
    public DbSet<Department.Entities.Location> Locations => Set<Department.Entities.Location>(); // NEW table
    public DbSet<Department.Entities.Address> Addresses => Set<Department.Entities.Address>();
    public DbSet<Department.Entities.FinancialYear> FinancialYears => Set<Department.Entities.FinancialYear>();
    public DbSet<Department.Entities.Holiday> Holidays => Set<Department.Entities.Holiday>();

    // -- Legacy lookup masters (tbl_*) — already-real tables from the original SMC schema --
    public DbSet<Department.Entities.BloodGroupMaster> BloodGroupMasters => Set<Department.Entities.BloodGroupMaster>();
    public DbSet<Department.Entities.BankMaster> BankMasters => Set<Department.Entities.BankMaster>();
    public DbSet<Department.Entities.ClassMaster> ClassMasters => Set<Department.Entities.ClassMaster>();
    public DbSet<Department.Entities.CasteMaster> CasteMasters => Set<Department.Entities.CasteMaster>();
    public DbSet<Department.Entities.CountryMaster> CountryMasters => Set<Department.Entities.CountryMaster>();
    public DbSet<Department.Entities.CityMaster> CityMasters => Set<Department.Entities.CityMaster>();
    public DbSet<Department.Entities.DistrictMaster> DistrictMasters => Set<Department.Entities.DistrictMaster>();
    public DbSet<Department.Entities.EducationTypeMaster> EducationTypeMasters => Set<Department.Entities.EducationTypeMaster>();
    public DbSet<Department.Entities.GenderMaster> GenderMasters => Set<Department.Entities.GenderMaster>();
    public DbSet<Department.Entities.LocationMaster> LocationMasters => Set<Department.Entities.LocationMaster>();
    public DbSet<Department.Entities.MaritalStatusMaster> MaritalStatusMasters => Set<Department.Entities.MaritalStatusMaster>();
    public DbSet<Department.Entities.OccupationMaster> OccupationMasters => Set<Department.Entities.OccupationMaster>();
    public DbSet<Department.Entities.RequiredDocumentMaster> RequiredDocumentMasters => Set<Department.Entities.RequiredDocumentMaster>();
    public DbSet<Department.Entities.ReligionMaster> ReligionMasters => Set<Department.Entities.ReligionMaster>();
    public DbSet<Department.Entities.RelationTypeMaster> RelationTypeMasters => Set<Department.Entities.RelationTypeMaster>();
    public DbSet<Department.Entities.StateMaster> StateMasters => Set<Department.Entities.StateMaster>();
    public DbSet<Department.Entities.TahsilMaster> TahsilMasters => Set<Department.Entities.TahsilMaster>();
    public DbSet<Department.Entities.TitleMaster> TitleMasters => Set<Department.Entities.TitleMaster>();
    public DbSet<Department.Entities.UserStatusMaster> UserStatusMasters => Set<Department.Entities.UserStatusMaster>();
    public DbSet<Department.Entities.WardMaster> WardMasters => Set<Department.Entities.WardMaster>();
    public DbSet<Department.Entities.ZoneMaster> ZoneMasters => Set<Department.Entities.ZoneMaster>();
    public DbSet<Department.Entities.OrgMaster> OrgMasters => Set<Department.Entities.OrgMaster>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Every IEntityTypeConfiguration<T> in this assembly is picked up automatically
        // (core tables live in Data/Configurations/*, module tables live in
        // Modules/{Dept}/*Configuration.cs, Department Portal tables live in
        // Department/Entities/StaffEntitiesConfiguration.cs). New devs never need to touch this file.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

