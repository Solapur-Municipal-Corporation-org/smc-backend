using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Master.Application.Interfaces;

namespace SMC.Master.API.Controllers;

[ApiController]
[Route("api/department")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    public DepartmentController(IDepartmentService departmentService) => _departmentService = departmentService;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _departmentService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var department = await _departmentService.GetByIdAsync(id);
        return department is null ? NotFound() : Ok(department);
    }
}
