using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMC.Master.Application.Interfaces;

namespace SMC.Master.API.Controllers;

[ApiController]
[Route("api/citizen")]
[Authorize(Roles = "Citizen")]
public class CitizenController : ControllerBase
{
    private readonly ICitizenService _citizenService;
    public CitizenController(ICitizenService citizenService) => _citizenService = citizenService;

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var citizen = await _citizenService.GetByUserIdAsync(userId);
        return citizen is null ? NotFound() : Ok(citizen);
    }
}
