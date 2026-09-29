using CitizenPortal.Api.Department.DTOs;
using CitizenPortal.Api.Department.Security;
using CitizenPortal.Api.Department.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>Department Master (HR/admin side — table "Departments"). Was "DepartmentController"
/// in the standalone Department Portal; renamed only to avoid colliding with
/// CitizenPortal.Api's own citizen-facing Department (MR_DEPT_Departments).</summary>
[ApiController]
[Route("api/department")]
public class StaffDepartmentController : DepartmentScopedControllerBase
{
    private readonly IDepartmentService _service;
    public StaffDepartmentController(IDepartmentService service) => _service = service;

    /// <summary>Full list — used by Admin screens (department picker, Department Master table).
    /// Deliberately NOT department-restricted: an org-wide department directory is not
    /// sensitive, and DepartmentAdmin/Employee UIs only ever call GetMine below for "my dept".</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllWithServiceCountAsync());

    [HttpGet("cards")]
    public async Task<IActionResult> GetCards() => Ok(await _service.GetAllWithServiceCountAsync());

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        => Ok(await _service.GetPagedAsync(pageNumber, pageSize, search));

    /// <summary>The logged-in user's own department (spec item 8/6 — never let the frontend pick).</summary>
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        if (CurrentDepartmentId is null) return NotFound(new { message = "No department assigned to this account." });
        var result = await _service.GetByIdAsync(CurrentDepartmentId.Value);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var forbidden = EnsureDepartmentAccess(id);
        if (forbidden != null) return forbidden;

        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] DepartmentDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.DepartmentId }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] DepartmentDto dto)
    {
        var success = await _service.UpdateAsync(id, dto);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
