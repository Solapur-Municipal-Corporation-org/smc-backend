using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>
/// Generic CRUD base for straightforward masters (Country, State, District, Tahsil, City,
/// Location, Address, FinancialYear, Holiday, Employee, Education, Family,
/// EmployeeLeaveBalance, EmployeeDocument). All reads are open to authenticated users;
/// writes require Admin. Concrete controllers just supply the route and the entity's id.
/// </summary>
[ApiController]
[Authorize]
public abstract class GenericCrudController<TEntity> : ControllerBase where TEntity : class
{
    protected readonly IRepository<TEntity> Repo;
    protected GenericCrudController(IRepository<TEntity> repo) => Repo = repo;

    protected abstract object GetId(TEntity entity);

    [HttpGet]
    public virtual async Task<IActionResult> GetAll() => Ok(await Repo.GetAllAsync());

    [HttpGet("{id:int}")]
    public virtual async Task<IActionResult> GetById(int id)
    {
        var entity = await Repo.GetByIdAsync(id);
        return entity == null ? NotFound() : Ok(entity);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public virtual async Task<IActionResult> Create([FromBody] TEntity entity)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await Repo.AddAsync(entity);
        await Repo.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = GetId(entity) }, entity);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public virtual async Task<IActionResult> Update(int id, [FromBody] TEntity entity)
    {
        var existing = await Repo.GetByIdAsync(id);
        if (existing == null) return NotFound();
        Repo.Update(entity);
        await Repo.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var entity = await Repo.GetByIdAsync(id);
        if (entity == null) return NotFound();
        Repo.Remove(entity);
        await Repo.SaveChangesAsync();
        return NoContent();
    }
}
