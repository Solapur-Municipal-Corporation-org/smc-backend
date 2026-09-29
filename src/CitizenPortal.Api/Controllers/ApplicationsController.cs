using System.Data.Common;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using CitizenPortal.Api.Data;
using CitizenPortal.Api.DTOs;
using CitizenPortal.Api.Models;
using CitizenPortal.Api.Services;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitizenPortal.Api.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IApplicationNumberGenerator _numberGenerator;
    private readonly IFileStorageService _fileStorage;
    private readonly ICertificateService _certificateService;
    private readonly IConfiguration _configuration;

    public ApplicationsController(
        AppDbContext db,
        IApplicationNumberGenerator numberGenerator,
        IFileStorageService fileStorage,
        ICertificateService certificateService,
        IConfiguration configuration)
    {
        _db = db;
        _numberGenerator = numberGenerator;
        _fileStorage = fileStorage;
        _certificateService = certificateService;
        _configuration = configuration;
    }

    private Guid CurrentCitizenId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize]
    [HttpPost]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<ApplicationResponse>> Create([FromForm] Guid serviceId, [FromForm] string formDataJson, [FromForm] List<IFormFile>? documents)
    {
        var service = await _db.Services.Include(s => s.Department).Include(s => s.Fields).FirstOrDefaultAsync(s => s.Id == serviceId);
        if (service is null) return NotFound(new { message = "Service not found." });

        var mobileNumber = User.FindFirstValue("mobileNumber") ?? User.FindFirstValue(ClaimTypes.Name);
        var citizen = string.IsNullOrWhiteSpace(mobileNumber) ? null : await GetCitizenFromSqlServer(mobileNumber);
        if (citizen is null) return BadRequest(new { message = "Citizen is not registered in CitizenPortalDb." });
        formDataJson = ApplyCitizenValues(formDataJson, service, citizen);

        if (service.Name.Equals("Tree Cutting", StringComparison.OrdinalIgnoreCase))
            return await CreateTreeCuttingInSqlServer(formDataJson, documents);

        JsonDocument formData;
        try
        {
            formData = JsonDocument.Parse(formDataJson);
        }
        catch (JsonException)
        {
            return BadRequest(new { message = "Form data is not valid JSON." });
        }

        using (formData)
        {
            if (formData.RootElement.ValueKind != JsonValueKind.Object)
                return BadRequest(new { message = "Form data must be a JSON object." });

            var applicationTypeId = GetGuid(formData.RootElement, "applicationType");
            var applicantTypeId = GetGuid(formData.RootElement, "applicantType");

            if (!service.Name.Equals("Tree Cutting", StringComparison.OrdinalIgnoreCase) && applicationTypeId.HasValue)
            {
                var applicationTypeExists = await _db.ApplicationTypes
                    .AnyAsync(type => type.Id == applicationTypeId && type.ServiceId == service.Id);
                if (!applicationTypeExists)
                    return BadRequest(new { message = "The selected application type is not configured for this service." });

                if (applicantTypeId.HasValue)
                {
                    var applicantTypeExists = await _db.ApplicantTypes
                        .AnyAsync(type => type.Id == applicantTypeId && type.ApplicationTypeId == applicationTypeId);
                    if (!applicantTypeExists)
                        return BadRequest(new { message = "The selected applicant type is not configured for the selected application type." });
                }
            }
            else if (!service.Name.Equals("Tree Cutting", StringComparison.OrdinalIgnoreCase) && applicantTypeId.HasValue)
            {
                return BadRequest(new { message = "An applicant type requires a valid application type." });
            }
        }

        var application = new Application
        {
            ApplicationNumber = _numberGenerator.Generate(service.Department!.Code),
            CitizenId = CurrentCitizenId,
            ServiceId = service.Id,
            FinancialYear = _numberGenerator.CurrentFinancialYear(),
            FormDataJson = formDataJson,
            Status = ApplicationStatus.Pending,
        };

        _db.Applications.Add(application);
        await _db.SaveChangesAsync();

        if (documents is not null)
        {
            foreach (var file in documents)
            {
                if (file.Length == 0) continue;
                var path = await _fileStorage.SaveAsync(file, $"applications/{application.Id}");
                _db.ApplicationDocuments.Add(new ApplicationDocument
                {
                    ApplicationId = application.Id,
                    DocumentName = file.FileName,
                    FilePath = path,
                });
            }
            await _db.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetByNumber), new { applicationNumber = application.ApplicationNumber }, await ToResponse(application.Id));
    }

    [AllowAnonymous]
    [HttpGet("track/{applicationNumber}")]
    public async Task<ActionResult<ApplicationResponse>> GetByNumber(string applicationNumber)
    {
        var sqlApplication = await GetTreeCuttingFromSqlServer(applicationNumber);
        if (sqlApplication is not null) return Ok(sqlApplication);

        var application = await _db.Applications
            .Include(a => a.Service).ThenInclude(s => s!.Department)
            .Include(a => a.Payment)
            .FirstOrDefaultAsync(a => a.ApplicationNumber == applicationNumber);

        if (application is null) return NotFound(new { message = "No application found with this number." });

        return Ok(Map(application));
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<List<ApplicationResponse>>> GetMine()
    {
        if (_db.Database.IsSqlite())
        {
            var sqliteApplications = await _db.Applications
                .Include(a => a.Service).ThenInclude(s => s!.Department)
                .Include(a => a.Payment)
                .Where(a => a.CitizenId == CurrentCitizenId)
                .OrderByDescending(a => a.SubmittedOn)
                .ToListAsync();

            return Ok(sqliteApplications.Select(Map).ToList());
        }

        var citizenId = GuidToInt(CurrentCitizenId);
        var connection = GetOpenConnection();
        await using var _ = connection;
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT a.Id, a.ApplicationNumber, a.Status, a.FinancialYear, a.FormDataJson,
                a.SubmittedOn, a.UpdatedOn, s.ServiceName, d.DepartmentName
            FROM dbo.TR_CFC_Applications a
            JOIN dbo.TR_CFC_Services s ON s.ServiceId = a.ServiceId
            JOIN dbo.MR_DEPT_Departments d ON d.DepartmentId = s.DepartmentId
            WHERE a.CitizenId = @citizenId
            ORDER BY a.SubmittedOn DESC";

        AddParameter(command, "@citizenId", citizenId);
        await using var reader = await command.ExecuteReaderAsync();

        var applications = new List<ApplicationResponse>();
        while (await reader.ReadAsync())
        {
            applications.Add(new ApplicationResponse
            {
                Id = reader.GetGuid(0),
                ApplicationNumber = reader.GetString(1),
                Status = reader.GetString(2),
                FinancialYear = reader.GetString(3),
                FormDataJson = reader.IsDBNull(4) ? "{}" : reader.GetString(4),
                SubmittedOn = reader.GetDateTime(5),
                UpdatedOn = reader.GetDateTime(6),
                ServiceName = reader.GetString(7),
                DepartmentName = reader.GetString(8),
            });
        }

        return Ok(applications);
    }

    [Authorize]
    [HttpGet("{id:guid}/certificate")]
    public async Task<IActionResult> DownloadCertificate(Guid id)
    {
        var application = await _db.Applications
            .Include(a => a.Service).ThenInclude(s => s!.Department)
            .Include(a => a.Citizen)
            .FirstOrDefaultAsync(a => a.Id == id && a.CitizenId == CurrentCitizenId);

        if (application is null) return NotFound();

        if (application.Status != ApplicationStatus.Approved)
            return BadRequest(new { message = "Certificate is only available for approved applications." });

        if (application.FinancialYear != _numberGenerator.CurrentFinancialYear())
            return BadRequest(new { message = "This certificate is no longer valid — it belonged to a previous financial year." });

        var pdfBytes = _certificateService.GenerateCertificatePdf(
            application, application.Citizen!, application.Service!.Name, application.Service.Department!.Name);

        return File(pdfBytes, "application/pdf", $"{application.ApplicationNumber.Replace("/", "_")}_certificate.pdf");
    }

    private async Task<ApplicationResponse> ToResponse(Guid applicationId)
    {
        var application = await _db.Applications
            .Include(a => a.Service).ThenInclude(s => s!.Department)
            .Include(a => a.Payment)
            .FirstAsync(a => a.Id == applicationId);
        return Map(application);
    }

    private static ApplicationResponse Map(Application a) => new()
    {
        Id = a.Id,
        ApplicationNumber = a.ApplicationNumber,
        ServiceId = a.ServiceId,
        ServiceName = a.Service?.Name ?? string.Empty,
        DepartmentName = a.Service?.Department?.Name ?? string.Empty,
        Status = a.Status.ToString(),
        FinancialYear = a.FinancialYear,
        Fee = a.Service?.Fee ?? 0,
        PaymentStatus = a.Payment?.Status == PaymentStatus.Success ? "Paid" : "Unpaid",
        FormDataJson = a.FormDataJson,
        SubmittedOn = a.SubmittedOn,
        UpdatedOn = a.UpdatedOn,
        Remarks = a.Remarks,
    };

    private static Guid? GetGuid(JsonElement formData, string propertyName)
    {
        if (!formData.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
            return null;
        return Guid.TryParse(property.GetString(), out var id) ? id : null;
    }

    private static string GetFormValue(JsonElement formData, string propertyName) =>
        formData.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;

    private async Task<SqlCitizen?> GetCitizenFromSqlServer(string mobileNumber)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();
        await using var command = new SqlCommand("""
            SELECT Id, FirstName, MiddleName, LastName, MobileNumber, Email, AddressLine1,
                   AddressLine2, NearestLocation, City, State, Pincode, AadhaarNumber
            FROM dbo.MR_DEPT_Citizens
            WHERE MobileNumber = @mobileNumber
            """, connection);
        command.Parameters.AddWithValue("@mobileNumber", mobileNumber);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new SqlCitizen(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.GetString(6),
            reader.IsDBNull(7) ? null : reader.GetString(7),
            reader.IsDBNull(8) ? null : reader.GetString(8),
            reader.GetString(9),
            reader.GetString(10),
            reader.GetString(11),
            reader.GetString(12));
    }

    private static string ApplyCitizenValues(string formDataJson, Service service, SqlCitizen citizen)
    {
        var formData = JsonNode.Parse(formDataJson)?.AsObject()
            ?? throw new JsonException("Form data must be a JSON object.");
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["full name"] = citizen.FullName,
            ["address"] = citizen.AddressLine1,
            ["email id"] = citizen.Email,
            ["mobile number"] = citizen.MobileNumber,
            ["aadhar number"] = citizen.AadhaarNumber,
            ["aadhaar number"] = citizen.AadhaarNumber,
            ["city"] = citizen.City,
            ["state"] = citizen.State,
            ["pincode"] = citizen.Pincode,
        };

        foreach (var field in service.Fields)
        {
            if (values.TryGetValue(field.Label.Trim(), out var value))
                formData[field.Id.ToString()] = value;
        }

        var treeValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["fullName"] = citizen.FullName,
            ["address"] = citizen.AddressLine1,
            ["emailId"] = citizen.Email,
            ["mobileNo"] = citizen.MobileNumber,
            ["aadharNo"] = citizen.AadhaarNumber,
        };
        foreach (var (key, value) in treeValues) formData[key] = value;

        return formData.ToJsonString();
    }

    private sealed record SqlCitizen(
        int Id,
        string FirstName,
        string? MiddleName,
        string LastName,
        string MobileNumber,
        string Email,
        string AddressLine1,
        string? AddressLine2,
        string? NearestLocation,
        string City,
        string State,
        string Pincode,
        string AadhaarNumber)
    {
        public string FullName => string.Join(" ", new[] { FirstName, MiddleName, LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private async Task<ActionResult<ApplicationResponse>> CreateTreeCuttingInSqlServer(
        string formDataJson, List<IFormFile>? documents)
    {
        using var formData = JsonDocument.Parse(formDataJson);
        var mobileNumber = User.FindFirstValue("mobileNumber") ?? User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(mobileNumber)) return BadRequest(new { message = "Citizen mobile number is missing." });

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();
        var applicationNumber = _numberGenerator.Generate("GPD");
        await using var command = new SqlCommand("""
            INSERT INTO dbo.Applications
                (ApplicationNumber, CitizenId, ServiceId, Status, FinancialYear, FormDataJson, Remarks, SubmittedOn, UpdatedOn)
            OUTPUT INSERTED.Id
            SELECT @applicationNumber, c.Id, '005', 0, @financialYear, @formDataJson, NULL, SYSUTCDATETIME(), SYSUTCDATETIME()
            FROM dbo.Citizens c
            WHERE c.MobileNumber = @mobileNumber;
            """, connection);
        command.Parameters.AddWithValue("@applicationNumber", applicationNumber);
        command.Parameters.AddWithValue("@mobileNumber", mobileNumber);
        command.Parameters.AddWithValue("@financialYear", _numberGenerator.CurrentFinancialYear());
        command.Parameters.AddWithValue("@formDataJson", formDataJson);
        var insertedId = await command.ExecuteScalarAsync();
        if (insertedId is null) return BadRequest(new { message = "Citizen is not registered in CitizenPortalDb." });

        await using (var treeApplicationCommand = new SqlCommand("""
            INSERT INTO dbo.GPD_TreeCuttingApplications
                (ApplicationId, PlotSurveyNumber, PropertySiteAddress, WardNumber, ReasonForCutting, IsReplantationProposed, ReplantationCount)
            VALUES
                (@applicationId, @plotSurveyNumber, @propertySiteAddress, @wardNumber, @reasonForCutting, 0, 0)
            """, connection))
        {
            treeApplicationCommand.Parameters.AddWithValue("@applicationId", insertedId);
            treeApplicationCommand.Parameters.AddWithValue("@plotSurveyNumber", GetFormValue(formData.RootElement, "propertyTaxNo"));
            treeApplicationCommand.Parameters.AddWithValue("@propertySiteAddress", GetFormValue(formData.RootElement, "treeAddress"));
            treeApplicationCommand.Parameters.AddWithValue("@wardNumber", GetFormValue(formData.RootElement, "zoneNo"));
            treeApplicationCommand.Parameters.AddWithValue("@reasonForCutting", GetFormValue(formData.RootElement, "treeCuttingReason"));
            await treeApplicationCommand.ExecuteNonQueryAsync();
        }

        await using (var treeDetailCommand = new SqlCommand("""
            INSERT INTO dbo.GPD_TreeCuttingTreeDetails
                (TreeCuttingApplicationId, SpeciesName, Quantity)
            VALUES
                ((SELECT TOP 1 Id FROM dbo.GPD_TreeCuttingApplications WHERE ApplicationId = @applicationId ORDER BY Id DESC), @speciesName, @quantity)
            """, connection))
        {
            treeDetailCommand.Parameters.AddWithValue("@applicationId", insertedId);
            treeDetailCommand.Parameters.AddWithValue("@speciesName", GetFormValue(formData.RootElement, "treeSpecies"));
            treeDetailCommand.Parameters.AddWithValue("@quantity", int.TryParse(GetFormValue(formData.RootElement, "numberOfTreeCutting"), out var quantity) ? quantity : 0);
            await treeDetailCommand.ExecuteNonQueryAsync();
        }

        if (documents is not null)
        {
            foreach (var file in documents.Where(x => x.Length > 0))
            {
                var path = await _fileStorage.SaveAsync(file, $"applications/{insertedId}");
                await using var documentCommand = new SqlCommand("""
                    INSERT INTO dbo.ApplicationDocuments (ApplicationId, DocumentName, FilePath, UploadedOn)
                    VALUES (@applicationId, @documentName, @filePath, SYSUTCDATETIME())
                    """, connection);
                documentCommand.Parameters.AddWithValue("@applicationId", insertedId);
                documentCommand.Parameters.AddWithValue("@documentName", file.FileName);
                documentCommand.Parameters.AddWithValue("@filePath", path);
                await documentCommand.ExecuteNonQueryAsync();
            }
        }

        return CreatedAtAction(nameof(GetByNumber), new { applicationNumber }, new ApplicationResponse
        {
            ApplicationNumber = applicationNumber,
            ServiceName = "Tree Cutting",
            DepartmentName = "Garden & Parks Department",
            Status = "Pending",
            FinancialYear = _numberGenerator.CurrentFinancialYear(),
            FormDataJson = formData.RootElement.GetRawText(),
            SubmittedOn = DateTime.UtcNow,
            UpdatedOn = DateTime.UtcNow,
        });
    }

    private async Task<ApplicationResponse?> GetTreeCuttingFromSqlServer(string applicationNumber)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();
        await using var command = new SqlCommand("""
            SELECT a.Id, a.ApplicationNumber, a.Status, a.FinancialYear, a.FormDataJson, a.SubmittedOn, a.UpdatedOn,
                   s.Name, d.Name
            FROM dbo.Applications a
            JOIN dbo.Services s ON s.Id = a.ServiceId
            JOIN dbo.Departments d ON d.Id = s.DepartmentId
            WHERE a.ApplicationNumber = @applicationNumber AND s.Name LIKE 'Tree Cutting%'
            """, connection);
        command.Parameters.AddWithValue("@applicationNumber", applicationNumber);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadSqlApplication(reader) : null;
    }

    private async Task<List<ApplicationResponse>> GetCitizenApplicationsFromSqlServer(string mobileNumber)
    {
        var result = new List<ApplicationResponse>();
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();
        await using var command = new SqlCommand("""
                 SELECT a.Id, a.ApplicationNumber, a.Status, a.FinancialYear, a.FormDataJson, a.SubmittedOn, a.UpdatedOn,
                     s.ServiceName, d.DepartmentName
                 FROM dbo.TR_CFC_Applications a
                 JOIN dbo.MR_DEPT_Citizens c ON c.Id = a.CitizenId
                 JOIN dbo.TR_CFC_Services s ON s.ServiceId = a.ServiceId
                 JOIN dbo.MR_DEPT_Departments d ON d.Id = s.DepartmentId
            WHERE c.MobileNumber = @mobileNumber
            ORDER BY a.SubmittedOn DESC
            """, connection);
        command.Parameters.AddWithValue("@mobileNumber", mobileNumber);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) result.Add(ReadSqlApplication(reader));
        return result;
    }

    private static ApplicationResponse ReadSqlApplication(SqlDataReader reader) => new()
    {
        Id = reader.GetGuid(0),
        ApplicationNumber = reader.GetString(1),
        Status = reader.GetString(2),
        FinancialYear = reader.GetString(3),
        FormDataJson = reader.IsDBNull(4) ? "{}" : reader.GetString(4),
        SubmittedOn = reader.GetDateTime(5),
        UpdatedOn = reader.GetDateTime(6),
        ServiceName = reader.GetString(7),
        DepartmentName = reader.GetString(8),
    };

    private static Guid ToLegacyGuid(int id) => new(id, 0, 0, new byte[8]);

    private static int GuidToInt(Guid value)
    {
        var bytes = value.ToByteArray();
        return BitConverter.ToInt32(bytes, 0);
    }

    private DbConnection GetOpenConnection()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var connection = !string.IsNullOrWhiteSpace(connectionString)
            ? new SqlConnection(connectionString)
            : _db.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
            connection.Open();

        return connection;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }
}
