using CitizenPortal.Api.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CitizenPortal.Api.Controllers;

[ApiController]
[Route("api/treecutting")]
[Authorize]
public class TreeCuttingOptionsController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public TreeCuttingOptionsController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("options")]
    public async Task<ActionResult<Dictionary<string, List<string>>>> GetOptions()
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["applicationType"] = await ReadOptions(connection, "Application_type", "ApplicationTypeName", "DeparmentCode"),
            ["applicantType"] = await ReadOptions(connection, "Applicant_type", "ApplicantTypeName", "DepartmentCode"),
        };

        return Ok(result);
    }

    private static async Task<List<string>> ReadOptions(
        SqlConnection connection, string tableName, string nameColumn, string departmentColumn)
    {
        await using var command = new SqlCommand($"""
            SELECT {nameColumn}
            FROM dbo.{tableName}
            WHERE {departmentColumn} = 'GPD'
              AND ServiceCode LIKE 'Tree Cutting%'
              AND IsActive = 1
            ORDER BY id
            """, connection);
        await using var reader = await command.ExecuteReaderAsync();
        var options = new List<string>();
        while (await reader.ReadAsync()) options.Add(reader.GetString(0));
        return options;
    }
}
