using CitizenPortal.Api.Department.DTOs;
using CitizenPortal.Api.Department.Security;
using CitizenPortal.Api.Department.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>User Master — staff accounts (Admin / DepartmentAdmin / DepartmentEmployee).
/// Full CRUD, admin-only writes, department-scoped reads (item 6/7).</summary>
[ApiController]
[Route("api/user")]
public class UserController : DepartmentScopedControllerBase
{
    private readonly IUserService _service;
    public UserController(IUserService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync(IsSystemAdmin ? null : CurrentDepartmentId));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result is null) return NotFound();
        var forbidden = EnsureDepartmentAccess(result.DepartmentId);
        if (forbidden != null) return forbidden;
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] UserDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.UserId }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] UserDto dto)
    {
        try
        {
            var success = await _service.UpdateAsync(id, dto);
            return success ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
