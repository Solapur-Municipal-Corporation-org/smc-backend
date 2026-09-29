using CitizenPortal.Api.Data;
using CitizenPortal.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CitizenPortal.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public DepartmentsController(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<ActionResult<List<DepartmentResponse>>> GetAll()
    {
        if (_db.Database.IsSqlite())
        {
            var departments = await _db.Departments
                .Include(d => d.Services)
                .Where(d => d.DepartmentDescription == "Citizen")
                .OrderBy(d => d.DisplayOrder)
                .ToListAsync();

            var legacyResult = departments.Select(d => new DepartmentResponse
            {
                Id = d.Id,
                Code = d.Code,
                Name = d.Name,
                IconName = d.IconName,
                Services = d.Services.Select(s => new ServiceSummaryResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Fee = s.Fee,
                    ProcessingDays = s.ProcessingDays,
                }).ToList(),
            }).ToList();

            return Ok(legacyResult);
        }

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();
        await using var command = new SqlCommand("""
            SELECT d.DepartmentId, d.DepartmentCode, d.DepartmentName, d.IconName,
                   s.ServiceId, s.ServiceName, s.Description, s.Fee, s.ProcessingDays
            FROM dbo.MR_DEPT_Departments AS d
            LEFT JOIN dbo.TR_CFC_Services AS s
                ON s.DepartmentId = d.DepartmentId AND s.IsActive = 1
            WHERE d.DepartmentDescription = 'Citizen' AND d.IsActive = 1
            ORDER BY d.DisplayOrder, d.DepartmentId, s.ServiceName, s.ServiceId
            """, connection);

        await using var reader = await command.ExecuteReaderAsync();
        var departmentsById = new Dictionary<int, DepartmentResponse>();
        while (await reader.ReadAsync())
        {
            var departmentId = reader.GetInt32(0);
            if (!departmentsById.TryGetValue(departmentId, out var department))
            {
                department = new DepartmentResponse
                {
                    Id = GuidFromInt(departmentId),
                    Code = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Name = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    IconName = reader.IsDBNull(3) ? "Landmark" : reader.GetString(3),
                };
                departmentsById.Add(departmentId, department);
            }

            if (!reader.IsDBNull(4))
            {
                department.Services.Add(new ServiceSummaryResponse
                {
                    Id = GuidFromInt(reader.GetInt32(4)),
                    Name = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Description = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    Fee = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7),
                    ProcessingDays = reader.IsDBNull(8) ? 7 : reader.GetInt32(8),
                });
            }
        }

        return Ok(departmentsById.Values.ToList());
    }

    private static Guid GuidFromInt(int value)
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}
