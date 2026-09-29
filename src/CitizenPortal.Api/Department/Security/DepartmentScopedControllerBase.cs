using CitizenPortal.Api.Department.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CitizenPortal.Api.Department.Security;

/// <summary>
/// Base for every Department Portal controller that must enforce "a department user can only
/// touch their own department's data" (spec item 6). DepartmentId always comes from the JWT
/// claim set at login (DepartmentAuthController), never from the request — controllers must
/// call <see cref="EnsureDepartmentAccessAsync"/> (or the sync <see cref="EnsureDepartmentAccess"/>)
/// against the resource's OWN department id before returning or mutating it, and return the
/// 403 it produces as-is.
/// </summary>
[Authorize]
public abstract class DepartmentScopedControllerBase : ControllerBase
{
    /// <summary>Int UserId from the JWT `sub`/NameIdentifier claim.</summary>
    protected int? CurrentUserId =>
        int.TryParse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <summary>The staff DepartmentId (int, "Departments" table) from the JWT `departmentId`
    /// claim. Null for SystemAdmin users, who are not scoped to one department.</summary>
    protected int? CurrentDepartmentId =>
        int.TryParse(User.FindFirstValue("departmentId"), out var id) ? id : null;

    protected UserRole? CurrentRole =>
        Enum.TryParse<UserRole>(User.FindFirstValue(System.Security.Claims.ClaimTypes.Role), out var r) ? r : null;

    protected bool IsSystemAdmin => CurrentRole == UserRole.SystemAdmin;

    /// <summary>Call this before returning/mutating any record that belongs to a specific
    /// department. Returns null if access is allowed; otherwise returns the 403 to return
    /// immediately from the action (`return forbidden;`).</summary>
    protected IActionResult? EnsureDepartmentAccess(int? resourceDepartmentId)
    {
        if (IsSystemAdmin) return null; // item 7: SystemAdmin/MasterAdmin sees everything

        if (CurrentDepartmentId is null)
            return Forbid(); // no department on the token at all — nothing to scope to

        if (resourceDepartmentId is null || resourceDepartmentId != CurrentDepartmentId)
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "You do not have access to this department's data."
            });

        return null;
    }
}
