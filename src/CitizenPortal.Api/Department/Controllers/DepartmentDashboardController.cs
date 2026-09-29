using CitizenPortal.Api.Data;
using CitizenPortal.Api.Department.Security;
using CitizenPortal.Api.Department.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>Everything the Department Dashboard needs in one call (spec item 8). Department
/// name/services always come from the DB via the JWT's DepartmentId — never hardcoded.</summary>
[ApiController]
[Route("api/department/dashboard")]
public class DepartmentDashboardController : DepartmentScopedControllerBase
{
    private readonly AppDbContext _db;
    private readonly IDepartmentLinkService _deptLink;

    public DepartmentDashboardController(AppDbContext db, IDepartmentLinkService deptLink)
    {
        _db = db;
        _deptLink = deptLink;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (CurrentUserId is null) return Unauthorized();

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
        if (user is null) return Unauthorized();

        object? applicationCounts = null;
        var department = user.DepartmentId is null
            ? null
            : _db.Database.IsSqlite()
                ? await _db.StaffDepartments.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.DepartmentId == user.DepartmentId.Value)
                : await _db.StaffDepartments.FromSqlInterpolated($"""
                    SELECT DepartmentId, OrganizationId, SrNo, DepartmentName,
                           DepartmentNameMarathi, DepartmentCode, PrimaryFunctions,
                           DepartmentHead, Email, MobileNumber, IsActive, CreatedAt, UpdatedAt
                    FROM dbo.MR_DEPT_Departments
                    WHERE DepartmentId = {user.DepartmentId.Value}
                    """).AsNoTracking().FirstOrDefaultAsync();

        // The existing database has no separate Department Portal Services table;
        // service/application counts remain optional until their existing mappings
        // are reconciled with the TR_CFC_* tables.
        const int serviceCount = 0;

        return Ok(new
        {
            user = new
            {
                user.UserId,
                user.FullName,
                Role = user.Role.ToString(),
            },
            department = department is null ? null : new
            {
                department.DepartmentId,
                department.DepartmentName,
                department.DepartmentNameMarathi,
                department.DepartmentCode,
            },
            serviceCount,
            applicationCounts,
        });
    }
}
