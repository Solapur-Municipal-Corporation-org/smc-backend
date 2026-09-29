using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SMC.Master.API.Controllers;

[ApiController]
[Route("api/report")]
[Authorize(Roles = "DepartmentStaff,DepartmentAdmin,SuperAdmin")]
public class ReportController : ControllerBase
{
    [HttpGet("department/{departmentId:guid}/summary")]
    public IActionResult DepartmentSummary(Guid departmentId)
    {
        // Aggregate application counts by status for this department.
        return Ok(new { departmentId, message = "Wire up aggregation query here." });
    }
}
