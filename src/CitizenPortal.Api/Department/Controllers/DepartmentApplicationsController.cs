using CitizenPortal.Api.Data;
using CitizenPortal.Api.Department.Security;
using CitizenPortal.Api.Department.Services;
using CitizenPortal.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>
/// The Department Portal's view onto real citizen applications (spec item 10 — citizen submits
/// -> Level 1 -> Level 2 -> Final Approval -> certificate). Deliberately reads/writes
/// CitizenPortal.Api.Models.Application (TR_CFC_Applications) directly — NOT the standalone
/// Department Portal's own orphaned CitizenApplication/Applications table, which never holds
/// real citizen submissions. See INTEGRATION_REPORT.md.
/// </summary>
[ApiController]
[Route("api/department/applications")]
public class DepartmentApplicationsController : DepartmentScopedControllerBase
{
    private readonly AppDbContext _db;
    private readonly IDepartmentLinkService _deptLink;

    public DepartmentApplicationsController(AppDbContext db, IDepartmentLinkService deptLink)
    {
        _db = db;
        _deptLink = deptLink;
    }

    private async Task<Guid?> ResolveAllowedCitizenDepartmentIdAsync()
    {
        if (IsSystemAdmin) return null; // null here means "no filter" for SystemAdmin
        if (CurrentDepartmentId is null) return Guid.Empty; // no department -> matches nothing
        return await _deptLink.GetCitizenDepartmentIdAsync(CurrentDepartmentId.Value) ?? Guid.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var citizenDeptId = await ResolveAllowedCitizenDepartmentIdAsync();

        var query = _db.Applications
            .Include(a => a.Citizen)
            .Include(a => a.Service)
            .AsQueryable();

        if (citizenDeptId is not null) // SystemAdmin -> null -> unfiltered
            query = query.Where(a => a.Service!.DepartmentId == citizenDeptId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ApplicationStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a =>
                a.ApplicationNumber.Contains(search) ||
                (a.Citizen != null && (a.Citizen.FirstName + " " + a.Citizen.LastName).Contains(search)));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.SubmittedOn)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.ApplicationNumber,
                CitizenName = a.Citizen!.FirstName + " " + a.Citizen.LastName,
                a.Citizen.MobileNumber,
                ServiceName = a.Service!.Name,
                Status = a.Status.ToString(),
                a.SubmittedOn,
                a.UpdatedOn,
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var application = await _db.Applications
            .Include(a => a.Citizen)
            .Include(a => a.Service)
            .Include(a => a.Documents)
            .Include(a => a.Payment)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application is null) return NotFound();

        var citizenDeptId = await ResolveAllowedCitizenDepartmentIdAsync();
        if (citizenDeptId is not null && application.Service?.DepartmentId != citizenDeptId)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have access to this application." });

        return Ok(application);
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty; // Level1Scrutiny | Level2Verification | Approved | Rejected
        public string? Remarks { get; set; }
    }

    /// <summary>Advances an application through Level 1 -> Level 2 -> Approved/Rejected. Any
    /// transition is allowed here (workflow ordering enforcement is a good next-iteration
    /// addition — flagged in INTEGRATION_REPORT.md); what IS enforced unconditionally is that
    /// the acting user's department owns the application (item 6/17: never trust the client).</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        if (!Enum.TryParse<ApplicationStatus>(request.Status, true, out var newStatus))
            return BadRequest(new { message = "Invalid status." });

        var application = await _db.Applications.Include(a => a.Service).FirstOrDefaultAsync(a => a.Id == id);
        if (application is null) return NotFound();

        var citizenDeptId = await ResolveAllowedCitizenDepartmentIdAsync();
        if (citizenDeptId is not null && application.Service?.DepartmentId != citizenDeptId)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have access to this application." });

        application.Status = newStatus;
        application.Remarks = request.Remarks;
        application.UpdatedOn = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { application.Id, Status = application.Status.ToString(), application.UpdatedOn });
    }
}
