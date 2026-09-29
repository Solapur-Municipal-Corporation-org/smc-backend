using CitizenPortal.Api.Data;
using CitizenPortal.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitizenPortal.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private readonly AppDbContext _db;

    public CatalogController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Get all application types for a specific service</summary>
    [HttpGet("services/{serviceId}/application-types")]
    public async Task<ActionResult<List<ApplicationTypeResponse>>> GetApplicationTypesByService(Guid serviceId)
    {
        var service = await _db.Services.FirstOrDefaultAsync(s => s.Id == serviceId);
        if (service is null)
            return NotFound(new { message = "Service not found." });

        var applicationTypes = await _db.ApplicationTypes
            .Where(a => a.ServiceId == serviceId)
            .Include(a => a.ApplicantTypes.OrderBy(x => x.DisplayOrder))
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync();

        return Ok(applicationTypes.Select(MapApplicationType).ToList());
    }

    /// <summary>
    /// Gets types configured for one service after verifying its department code.
    /// The department code is part of the route so a citizen cannot accidentally
    /// receive options from a service displayed under a different department.
    /// </summary>
    [HttpGet("departments/{departmentCode}/services/{serviceId:guid}/application-types")]
    public async Task<ActionResult<List<ApplicationTypeResponse>>> GetApplicationTypesByDepartmentAndService(
        string departmentCode, Guid serviceId)
    {
        var types = await _db.ApplicationTypes
            .Where(a => a.ServiceId == serviceId && a.Service!.Department!.Code == departmentCode)
            .Include(a => a.ApplicantTypes)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync();

        return Ok(types.Select(MapApplicationType).ToList());
    }

    /// <summary>Get all applicant types for a specific application type</summary>
    [HttpGet("application-types/{applicationTypeId}/applicant-types")]
    public async Task<ActionResult<List<ApplicantTypeResponse>>> GetApplicantTypesByApplicationType(Guid applicationTypeId)
    {
        var applicationType = await _db.ApplicationTypes.FirstOrDefaultAsync(a => a.Id == applicationTypeId);
        if (applicationType is null)
            return NotFound(new { message = "Application type not found." });

        var applicantTypes = await _db.ApplicantTypes
            .Where(a => a.ApplicationTypeId == applicationTypeId)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync();

        return Ok(applicantTypes.Select(MapApplicantType).ToList());
    }

    /// <summary>Gets applicant types only when the application type belongs to the routed department and service.</summary>
    [HttpGet("departments/{departmentCode}/services/{serviceId:guid}/application-types/{applicationTypeId:guid}/applicant-types")]
    public async Task<ActionResult<List<ApplicantTypeResponse>>> GetApplicantTypesByDepartmentServiceAndApplicationType(
        string departmentCode, Guid serviceId, Guid applicationTypeId)
    {
        var applicantTypes = await _db.ApplicantTypes
            .Where(a => a.ApplicationTypeId == applicationTypeId
                     && a.ApplicationType!.ServiceId == serviceId
                     && a.ApplicationType.Service!.Department!.Code == departmentCode)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync();

        return Ok(applicantTypes.Select(MapApplicantType).ToList());
    }

    private static ApplicationTypeResponse MapApplicationType(Models.ApplicationType a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        Description = a.Description,
        ApplicantTypes = a.ApplicantTypes?.Select(MapApplicantType).ToList() ?? new(),
    };

    private static ApplicantTypeResponse MapApplicantType(Models.ApplicantType a) => new()
    {
        Id = a.Id,
        Name = a.Name,
        Description = a.Description,
    };
}
