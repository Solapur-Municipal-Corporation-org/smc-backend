using CitizenPortal.Api.Data;
using CitizenPortal.Api.Department.Entities;
using Microsoft.EntityFrameworkCore;

namespace CitizenPortal.Api.Department.Services;

/// <summary>
/// Links the two Department representations that live side-by-side in smc_db:
///   - StaffDepartment (int PK, table "Departments") — HR/admin side, what Users/Employees belong to.
///   - CitizenPortal.Api.Models.Department (Guid PK, table "MR_DEPT_Departments") — citizen-facing,
///     what TR_CFC_Services/TR_CFC_Applications belong to.
/// They are matched by their shared `Code` column rather than a foreign key, because forcing one
/// physical key system onto the other would require a destructive PK-type migration on live data.
/// See INTEGRATION_REPORT.md, "The one decision this whole integration hinges on" follow-up.
/// </summary>
public interface IDepartmentLinkService
{
    /// <summary>Given a staff user's (int) DepartmentId, returns the matching citizen-facing
    /// Department's Guid Id — or null if no department with that Code exists on the citizen side yet.</summary>
    Task<Guid?> GetCitizenDepartmentIdAsync(int staffDepartmentId);

    /// <summary>Reverse lookup — used when showing a citizen Application's department in a
    /// staff-facing screen and you need the int DepartmentId to compare against a JWT claim.</summary>
    Task<int?> GetStaffDepartmentIdAsync(Guid citizenDepartmentId);
}

public class DepartmentLinkService : IDepartmentLinkService
{
    private readonly AppDbContext _db;
    public DepartmentLinkService(AppDbContext db) => _db = db;

    public async Task<Guid?> GetCitizenDepartmentIdAsync(int staffDepartmentId)
    {
        var staffDept = await _db.StaffDepartments.AsNoTracking()
            .FirstOrDefaultAsync(d => d.DepartmentId == staffDepartmentId);
        if (staffDept == null || string.IsNullOrWhiteSpace(staffDept.DepartmentCode)) return null;

        var citizenDept = await _db.Departments.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Code == staffDept.DepartmentCode);
        return citizenDept?.Id;
    }

    public async Task<int?> GetStaffDepartmentIdAsync(Guid citizenDepartmentId)
    {
        var citizenDept = await _db.Departments.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == citizenDepartmentId);
        if (citizenDept == null) return null;

        var staffDept = await _db.StaffDepartments.AsNoTracking()
            .FirstOrDefaultAsync(d => d.DepartmentCode == citizenDept.Code);
        return staffDept?.DepartmentId;
    }
}
