using CitizenPortal.Api.Department.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CitizenPortal.Api.Department.Entities;

// These map to tables that ALREADY EXIST in smc_db (created previously by the standalone
// Department Portal package's own EF Core migrations under its original class names
// Department/Service/AppUser/Employee). Renaming the C# classes here (StaffDepartment,
// StaffService) does NOT change the physical table — ToTable() below points explicitly back
// at the original table names so no migration is required for the rename itself.

public class StaffDepartmentConfiguration : IEntityTypeConfiguration<StaffDepartment>
{
    public void Configure(EntityTypeBuilder<StaffDepartment> e)
    {
        e.ToTable("MR_DEPT_Departments", "dbo");
        e.HasKey(d => d.DepartmentId);
        e.HasIndex(d => d.DepartmentCode);
    }
}

public class StaffServiceConfiguration : IEntityTypeConfiguration<StaffService>
{
    public void Configure(EntityTypeBuilder<StaffService> e)
    {
        e.ToTable("Services"); // pre-existing table — unchanged
        e.HasKey(s => s.ServiceId);
        e.HasOne(s => s.Department).WithMany(d => d.Services).HasForeignKey(s => s.DepartmentId);
    }
}

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> e)
    {
        e.ToTable("Users"); // pre-existing table — unchanged
        e.HasKey(u => u.UserId);
        e.HasIndex(u => u.MobileNumber).IsUnique();
        e.HasOne(u => u.Department).WithMany(d => d.Users).HasForeignKey(u => u.DepartmentId);
        // OtpCode / OtpExpiresAt / OtpAttemptCount are new nullable columns — see
        // database/scripts/2026-xx-xx_add_otp_columns.sql. EF will expect them once that
        // script (or an equivalent migration) has been run against smc_db.
    }
}

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> e)
    {
        e.ToTable("Employees"); // pre-existing table — unchanged
        e.HasKey(x => x.EmployeeId);
        e.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId);
        e.HasOne(x => x.Address).WithOne(x => x.Employee).HasForeignKey<EmployeeAddress>(x => x.EmployeeId);
        e.HasOne(x => x.BankDetail).WithOne(x => x.Employee).HasForeignKey<EmployeeBankDetail>(x => x.EmployeeId);
        e.HasOne(x => x.Salary).WithOne(x => x.Employee).HasForeignKey<EmployeeSalary>(x => x.EmployeeId);
    }
}

public class CitizenApplicationConfiguration : IEntityTypeConfiguration<CitizenApplication>
{
    public void Configure(EntityTypeBuilder<CitizenApplication> e)
    {
        // NOT used by the ported Applications/Transactions screens (those read the real
        // citizen data from TR_CFC_Applications instead — see DepartmentApplicationsController).
        // Mapped here only so EF recognizes the table and doesn't try to create a duplicate.
        e.ToTable("Applications");
        e.HasKey(x => x.ApplicationId);
    }
}

public class EmployeeDocumentConfiguration : IEntityTypeConfiguration<EmployeeDocument>
{
    public void Configure(EntityTypeBuilder<EmployeeDocument> e)
    {
        e.ToTable("EmployeeDocuments");
        e.HasKey(x => x.DocumentId);
        e.HasOne(x => x.Employee).WithMany(x => x.Documents).HasForeignKey(x => x.EmployeeId);
    }
}

public class EmployeeLeaveBalanceConfiguration : IEntityTypeConfiguration<EmployeeLeaveBalance>
{
    public void Configure(EntityTypeBuilder<EmployeeLeaveBalance> e)
    {
        e.ToTable("EmployeeLeaveBalances");
        e.HasKey(x => x.LeaveBalanceId);
        e.HasOne(x => x.Employee).WithMany(x => x.LeaveBalances).HasForeignKey(x => x.EmployeeId);
    }
}
