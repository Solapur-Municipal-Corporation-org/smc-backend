using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Master.Application.Interfaces;

namespace SMC.Master.API.Controllers;

[ApiController]
[Route("api/service")]
[Authorize]
public class ServiceController : ControllerBase
{
    private readonly IServiceService _serviceService;
    public ServiceController(IServiceService serviceService) => _serviceService = serviceService;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _serviceService.GetAllAsync());

    [HttpGet("department/{departmentId:guid}")]
    public async Task<IActionResult> GetByDepartment(Guid departmentId) =>
        Ok(await _serviceService.GetByDepartmentAsync(departmentId));
}
