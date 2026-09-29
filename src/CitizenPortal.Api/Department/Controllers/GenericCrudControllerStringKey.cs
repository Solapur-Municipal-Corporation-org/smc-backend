using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CitizenPortal.Api.Department.Interfaces;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>
/// Generic CRUD base for legacy masters whose primary key is a string code
/// (e.g. BldGrpCode, BankCode, CastCode — as defined in tbl_scripts.txt),
/// rather than an auto-incrementing int.
/// </summary>
[ApiController]
[Authorize]
public abstract class GenericCrudControllerStringKey<TEntity> : ControllerBase where TEntity : class
{
    protected readonly IRepository<TEntity> Repo;
    protected GenericCrudControllerStringKey(IRepository<TEntity> repo) => Repo = repo;

    protected abstract object GetId(TEntity entity);

    [HttpGet]
    public virtual async Task<IActionResult> GetAll() => Ok(await Repo.GetAllAsync());

    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetById(string id)
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

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public virtual async Task<IActionResult> Update(string id, [FromBody] TEntity entity)
    {
        var existing = await Repo.GetByIdAsync(id);
        if (existing == null) return NotFound();
        Repo.Update(entity);
        await Repo.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public virtual async Task<IActionResult> Delete(string id)
    {
        var entity = await Repo.GetByIdAsync(id);
        if (entity == null) return NotFound();
        Repo.Remove(entity);
        await Repo.SaveChangesAsync();
        return NoContent();
    }
}
