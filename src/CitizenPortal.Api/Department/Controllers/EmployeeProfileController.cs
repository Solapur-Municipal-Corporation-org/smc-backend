using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Data;

namespace CitizenPortal.Api.Department.Controllers;

/// <summary>
/// Composite payload for the combined Employee wizard form: Employee Master (step 1),
/// Address (step 2), Documents (step 3), Education (step 4), Bank Details (step 5),
/// Salary (step 6) and Family (step 7) — all saved/read together in one call so the
/// wizard has a single "record" and the employee list has a single table.
/// </summary>
public class EmployeeProfileDto
{
    public Employee Employee { get; set; } = new();
    public EmployeeAddress? Address { get; set; }
    public EmployeeBankDetail? BankDetail { get; set; }
    public EmployeeSalary? Salary { get; set; }
    public List<Education> Education { get; set; } = new();
    public List<Family> Family { get; set; } = new();
    public List<EmployeeDocument> Documents { get; set; } = new();
}

[ApiController]
[Authorize]
[Route("api/employee-profile")]
public class EmployeeProfileController : ControllerBase
{
    private readonly AppDbContext _db;
    public EmployeeProfileController(AppDbContext db) => _db = db;

    /// <summary>Full profile for one employee — used to prefill the wizard on Edit.</summary>
    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> Get(int employeeId)
    {
        var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        if (employee == null) return NotFound();

        var dto = new EmployeeProfileDto
        {
            Employee = employee,
            Address = await _db.EmployeeAddresses.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeId == employeeId),
            BankDetail = await _db.EmployeeBankDetails.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeId == employeeId),
            Salary = await _db.EmployeeSalaries.AsNoTracking().FirstOrDefaultAsync(x => x.EmployeeId == employeeId),
            Education = await _db.Educations.AsNoTracking().Where(x => x.EmployeeId == employeeId).ToListAsync(),
            Family = await _db.Families.AsNoTracking().Where(x => x.EmployeeId == employeeId).ToListAsync(),
            Documents = await _db.EmployeeDocuments.AsNoTracking().Where(x => x.EmployeeId == employeeId).ToListAsync(),
        };
        return Ok(dto);
    }

    /// <summary>Creates a new employee together with every step's data, in one transaction.</summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] EmployeeProfileDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            dto.Employee.EmployeeId = 0;
            _db.Employees.Add(dto.Employee);
            await _db.SaveChangesAsync(); // need the generated EmployeeId before saving children

            SaveChildren(dto, dto.Employee.EmployeeId);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();
            return CreatedAtAction(nameof(Get), new { employeeId = dto.Employee.EmployeeId }, dto);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    /// <summary>Updates an employee and replaces every step's data, in one transaction.</summary>
    [HttpPut("{employeeId:int}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(int employeeId, [FromBody] EmployeeProfileDto dto)
    {
        var existing = await _db.Employees.FindAsync(employeeId);
        if (existing == null) return NotFound();

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // Step 1: Employee master fields
            dto.Employee.EmployeeId = employeeId;
            _db.Entry(existing).CurrentValues.SetValues(dto.Employee);

            // Steps 2, 5, 6: one-to-one sections — update in place or create on first save
            await UpsertOneToOneAsync(_db.EmployeeAddresses, employeeId, dto.Address);
            await UpsertOneToOneAsync(_db.EmployeeBankDetails, employeeId, dto.BankDetail);
            await UpsertOneToOneAsync(_db.EmployeeSalaries, employeeId, dto.Salary);

            // Steps 3, 4, 7: repeatable lists — replace wholesale with what the wizard submitted
            await ReplaceListAsync(_db.EmployeeDocuments, employeeId, dto.Documents);
            await ReplaceListAsync(_db.Educations, employeeId, dto.Education);
            await ReplaceListAsync(_db.Families, employeeId, dto.Family);

            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return NoContent();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    private void SaveChildren(EmployeeProfileDto dto, int employeeId)
    {
        if (dto.Address != null) { dto.Address.EmployeeAddressId = 0; dto.Address.EmployeeId = employeeId; _db.EmployeeAddresses.Add(dto.Address); }
        if (dto.BankDetail != null) { dto.BankDetail.EmployeeBankDetailId = 0; dto.BankDetail.EmployeeId = employeeId; _db.EmployeeBankDetails.Add(dto.BankDetail); }
        if (dto.Salary != null) { dto.Salary.EmployeeSalaryId = 0; dto.Salary.EmployeeId = employeeId; _db.EmployeeSalaries.Add(dto.Salary); }

        foreach (var d in dto.Documents) { d.DocumentId = 0; d.EmployeeId = employeeId; _db.EmployeeDocuments.Add(d); }
        foreach (var e in dto.Education) { e.EducationId = 0; e.EmployeeId = employeeId; _db.Educations.Add(e); }
        foreach (var f in dto.Family) { f.FamilyId = 0; f.EmployeeId = employeeId; _db.Families.Add(f); }
    }

    private async Task UpsertOneToOneAsync<T>(Microsoft.EntityFrameworkCore.DbSet<T> set, int employeeId, T? incoming) where T : class
    {
        var employeeIdProp = typeof(T).GetProperty("EmployeeId")!;
        var existing = (await set.ToListAsync()).FirstOrDefault(x => (int)employeeIdProp.GetValue(x)! == employeeId);

        if (incoming == null)
        {
            if (existing != null) set.Remove(existing);
            return;
        }

        employeeIdProp.SetValue(incoming, employeeId);
        var idProp = typeof(T).GetProperties().First(p => p.Name.EndsWith("Id") && p.Name != "EmployeeId");

        if (existing == null)
        {
            // Reset the id field to 0 so EF treats it as a new row.
            idProp.SetValue(incoming, 0);
            set.Add(incoming);
        }
        else
        {
            idProp.SetValue(incoming, idProp.GetValue(existing));
            _db.Entry(existing).CurrentValues.SetValues(incoming);
        }
    }

    private async Task ReplaceListAsync<T>(Microsoft.EntityFrameworkCore.DbSet<T> set, int employeeId, List<T> incoming) where T : class
    {
        var employeeIdProp = typeof(T).GetProperty("EmployeeId")!;
        var existingRows = (await set.ToListAsync()).Where(x => (int)employeeIdProp.GetValue(x)! == employeeId).ToList();
        set.RemoveRange(existingRows);

        var idProp = typeof(T).GetProperties().First(p => p.Name.EndsWith("Id") && p.Name != "EmployeeId");
        foreach (var row in incoming)
        {
            idProp.SetValue(row, 0);
            employeeIdProp.SetValue(row, employeeId);
        }
        set.AddRange(incoming);
    }
}
