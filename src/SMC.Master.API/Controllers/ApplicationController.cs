using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Master.Application.DTOs.Application;
using SMC.Master.Application.Interfaces;

namespace SMC.Master.API.Controllers;

[ApiController]
[Route("api/application")]
[Authorize]
public class ApplicationController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    public ApplicationController(IApplicationService applicationService) => _applicationService = applicationService;

    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var citizenId = Guid.Parse(User.FindFirst("sub")!.Value);
        return Ok(await _applicationService.GetForCitizenAsync(citizenId));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var application = await _applicationService.GetByIdAsync(id);
        return application is null ? NotFound() : Ok(application);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateApplicationRequest request)
    {
        var citizenId = Guid.Parse(User.FindFirst("sub")!.Value);
        var id = await _applicationService.CreateAsync(citizenId, request.ServiceId, request.FormDataJson);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "DepartmentStaff,DepartmentAdmin,SuperAdmin")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateApplicationStatusDto request)
    {
        await _applicationService.UpdateStatusAsync(id, request.Status);
        return NoContent();
    }
}

public record CreateApplicationRequest(Guid ServiceId, string FormDataJson);
