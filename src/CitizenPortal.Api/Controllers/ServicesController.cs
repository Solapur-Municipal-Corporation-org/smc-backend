using CitizenPortal.Api.Data;
using CitizenPortal.Api.DTOs;
using CitizenPortal.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitizenPortal.Api.Controllers;

[ApiController]
[Route("api/services")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ServicesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceDetailResponse>> GetById(Guid id)
    {
        var service = await _db.Services
            .Include(s => s.Department)
            .Include(s => s.Fields)
            .Include(s => s.RequiredDocuments)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service is null) return NotFound();

        var response = new ServiceDetailResponse
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            Fee = service.Fee,
            ProcessingDays = service.ProcessingDays,
            DepartmentCode = service.Department?.Code ?? string.Empty,
            DepartmentName = service.Department?.Name ?? string.Empty,
            DocumentsRequired = service.RequiredDocuments.Select(d => d.DocumentName).ToList(),
            Fields = service.Fields.OrderBy(f => f.DisplayOrder).Select(f => new ServiceFieldResponse
            {
                Id = f.Id,
                Label = f.Label,
                Type = f.FieldType.ToString().ToLowerInvariant(),
                Required = f.Required,
                Options = f.Options?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            }).ToList(),
        };

        return Ok(response);
    }
}
