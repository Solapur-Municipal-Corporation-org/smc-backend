using CitizenPortal.Api.Data;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>Employee master (table "Employees") — department-scoped list/detail (item 6):
/// a DepartmentAdmin/Employee only ever sees their own department's staff; SystemAdmin sees all.</summary>
[ApiController]
[Route("api/masters/employee")]
public class EmployeeController : DepartmentScopedControllerBase
{
    private readonly AppDbContext _db;
    public EmployeeController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = _db.Employees.AsNoTracking().AsQueryable();
        if (!IsSystemAdmin)
            query = CurrentDepartmentId is null
                ? query.Where(e => false)
                : query.Where(e => e.DepartmentId == CurrentDepartmentId);
        return Ok(await query.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeId == id);
        if (employee is null) return NotFound();

        var forbidden = EnsureDepartmentAccess(employee.DepartmentId);
        if (forbidden != null) return forbidden;

        return Ok(employee);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] Employee entity)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        entity.EmployeeId = 0;
        _db.Employees.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.EmployeeId }, entity);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(int id, [FromBody] Employee entity)
    {
        var existing = await _db.Employees.FindAsync(id);
        if (existing is null) return NotFound();
        entity.EmployeeId = id;
        _db.Entry(existing).CurrentValues.SetValues(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Employees.FindAsync(id);
        if (existing is null) return NotFound();
        _db.Employees.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
