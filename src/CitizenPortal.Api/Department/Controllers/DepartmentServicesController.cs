using CitizenPortal.Api.Data;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CitizenPortal.Api.Department.Controllers;

[ApiController]
[Route("api/department/services")]
public sealed class DepartmentServicesController : DepartmentScopedControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;

    public DepartmentServicesController(AppDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetGroupedServices()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        var isSystemAdmin = user.Role == UserRole.SystemAdmin;
        if (!isSystemAdmin && user.DepartmentId is null)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "No department is assigned to this account." });

        var sql = """
            SELECT d.DepartmentId, d.DepartmentName, d.DepartmentNameMarathi,
                   s.ServiceId, s.ServiceName, s.ServiceNameMarathi
            FROM dbo.MR_DEPT_Departments AS d
            LEFT JOIN dbo.TR_CFC_Services AS s
                ON s.DepartmentId = d.DepartmentId AND s.IsActive = 1
            WHERE d.IsActive = 1
            """;
        if (!isSystemAdmin)
            sql += " AND d.DepartmentId = @DepartmentId";
        sql += " ORDER BY d.DisplayOrder, d.DepartmentId, s.ServiceName, s.ServiceId";

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        if (!isSystemAdmin)
            command.Parameters.AddWithValue("@DepartmentId", user.DepartmentId!.Value);

        var departments = new Dictionary<int, DepartmentServicesGroup>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var departmentId = reader.GetInt32(0);
            if (!departments.TryGetValue(departmentId, out var department))
            {
                department = new DepartmentServicesGroup
                {
                    DepartmentId = departmentId,
                    DepartmentName = reader.GetString(1),
                    DepartmentNameMarathi = reader.IsDBNull(2) ? null : reader.GetString(2),
                };
                departments.Add(departmentId, department);
            }

            if (!reader.IsDBNull(3))
            {
                department.Services.Add(new DepartmentServiceSummary
                {
                    ServiceId = reader.GetInt32(3),
                    ServiceName = reader.GetString(4),
                    ServiceNameMarathi = reader.IsDBNull(5) ? null : reader.GetString(5),
                });
            }
        }

        // Keep active departments visible even if no active services are currently assigned.
        return Ok(departments.Values);
    }

    [HttpGet("{serviceId:int}")]
    public async Task<IActionResult> GetService(int serviceId)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        const string sql = """
            SELECT s.ServiceId, s.DepartmentId, s.ServiceName, s.ServiceNameMarathi,
                   s.ServiceCode, s.Description, s.Fee, s.ProcessingDays,
                   d.DepartmentName, d.DepartmentNameMarathi
            FROM dbo.TR_CFC_Services AS s
            INNER JOIN dbo.MR_DEPT_Departments AS d ON d.DepartmentId = s.DepartmentId
            WHERE s.ServiceId = @ServiceId AND s.IsActive = 1 AND d.IsActive = 1
            """;

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ServiceId", serviceId);

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return NotFound(new { message = "Service was not found." });

        var departmentId = reader.GetInt32(1);
        if (user.Role != UserRole.SystemAdmin && user.DepartmentId != departmentId)
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have access to this service." });

        return Ok(new DepartmentServiceDetail
        {
            ServiceId = reader.GetInt32(0),
            DepartmentId = departmentId,
            ServiceName = reader.GetString(2),
            ServiceNameMarathi = reader.IsDBNull(3) ? null : reader.GetString(3),
            ServiceCode = reader.GetString(4),
            Description = reader.IsDBNull(5) ? null : reader.GetString(5),
            Fee = reader.GetDecimal(6),
            ProcessingDays = reader.GetInt32(7),
            DepartmentName = reader.GetString(8),
            DepartmentNameMarathi = reader.IsDBNull(9) ? null : reader.GetString(9),
        });
    }

    private async Task<AppUser?> GetCurrentUserAsync()
    {
        if (CurrentUserId is not int userId) return null;

        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.UserId == userId);
        return user is { IsActive: true } ? user : null;
    }

}

public sealed class DepartmentServicesGroup
{
    public int DepartmentId { get; init; }
    public string DepartmentName { get; init; } = string.Empty;
    public string? DepartmentNameMarathi { get; init; }
    public List<DepartmentServiceSummary> Services { get; } = new();
}

public sealed class DepartmentServiceSummary
{
    public int ServiceId { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public string? ServiceNameMarathi { get; init; }
}

public sealed class DepartmentServiceDetail
{
    public int ServiceId { get; init; }
    public int DepartmentId { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public string? ServiceNameMarathi { get; init; }
    public string ServiceCode { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Fee { get; init; }
    public int ProcessingDays { get; init; }
    public string DepartmentName { get; init; } = string.Empty;
    public string? DepartmentNameMarathi { get; init; }
}
