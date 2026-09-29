using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;
using CitizenPortal.Api.Department.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>Internal Service Master (table "Services", scoped to a Department). This is the
/// Department Portal's own HR/admin service catalog — kept separate from the citizen-facing
/// TR_CFC_Services catalog (see INTEGRATION_REPORT.md "Service and Department Data" for why
/// full unification of the two catalogs is flagged as follow-up work, not done here).</summary>
[ApiController]
[Route("api/service")]
public class StaffServiceController : DepartmentScopedControllerBase
{
    private readonly IRepository<StaffService> _repo;
    public StaffServiceController(IRepository<StaffService> repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var all = await _repo.GetAllAsync();
        if (IsSystemAdmin) return Ok(all);
        if (CurrentDepartmentId is null) return Ok(Array.Empty<StaffService>());
        return Ok(all.Where(s => s.DepartmentId == CurrentDepartmentId));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null) return NotFound();
        var forbidden = EnsureDepartmentAccess(((StaffService)entity).DepartmentId);
        if (forbidden != null) return forbidden;
        return Ok(entity);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] StaffService entity)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.ServiceId }, entity);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] StaffService entity)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();
        _repo.Update(entity);
        await _repo.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return NotFound();
        _repo.Remove(entity);
        await _repo.SaveChangesAsync();
        return NoContent();
    }
}
