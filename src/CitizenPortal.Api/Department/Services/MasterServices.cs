using Microsoft.EntityFrameworkCore;
using CitizenPortal.Api.Department.DTOs;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Department.Interfaces;
using CitizenPortal.Api.Data;

namespace CitizenPortal.Api.Department.Services;

public interface IOrganizationService
{
    Task<IEnumerable<OrganizationDto>> GetAllAsync();
    Task<OrganizationDto?> GetByIdAsync(int id);
    Task<OrganizationDto> CreateAsync(OrganizationDto dto);
    Task<bool> UpdateAsync(int id, OrganizationDto dto);
    Task<bool> DeleteAsync(int id);
}

/// <summary>Organization Master — the root master. Every department/user/service is scoped under it.</summary>
public class OrganizationService : IOrganizationService
{
    private readonly IRepository<Organization> _repo;

    public OrganizationService(IRepository<Organization> repo) => _repo = repo;

    private static OrganizationDto ToDto(Organization o) => new()
    {
        OrganizationId = o.OrganizationId,
        OrganizationName = o.OrganizationName,
        OrganizationNameMarathi = o.OrganizationNameMarathi,
        PrintingHeader = o.PrintingHeader,
        OrganizationType = o.OrganizationType,
        Email = o.Email,
        PhoneNumber1 = o.PhoneNumber1,
        PhoneNumber2 = o.PhoneNumber2,
        Website = o.Website,
        GstNumber = o.GstNumber,
        PanNumber = o.PanNumber,
        RegistrationNumber = o.RegistrationNumber,
        ActiveCommissioner = o.ActiveCommissioner,
        ActiveMayor = o.ActiveMayor,
        ActiveDyMayor = o.ActiveDyMayor,
        ActiveStandingChairman = o.ActiveStandingChairman,
        Location = o.Location,
        IsActive = o.IsActive
    };

    public async Task<IEnumerable<OrganizationDto>> GetAllAsync() =>
        (await _repo.GetAllAsync()).Select(ToDto);

    public async Task<OrganizationDto?> GetByIdAsync(int id)
    {
        var o = await _repo.GetByIdAsync(id);
        return o == null ? null : ToDto(o);
    }

    public async Task<OrganizationDto> CreateAsync(OrganizationDto dto)
    {
        var entity = new Organization
        {
            OrganizationName = dto.OrganizationName,
            OrganizationNameMarathi = dto.OrganizationNameMarathi,
            PrintingHeader = dto.PrintingHeader,
            OrganizationType = dto.OrganizationType,
            Email = dto.Email,
            PhoneNumber1 = dto.PhoneNumber1,
            PhoneNumber2 = dto.PhoneNumber2,
            Website = dto.Website,
            GstNumber = dto.GstNumber,
            PanNumber = dto.PanNumber,
            RegistrationNumber = dto.RegistrationNumber,
            ActiveCommissioner = dto.ActiveCommissioner,
            ActiveMayor = dto.ActiveMayor,
            ActiveDyMayor = dto.ActiveDyMayor,
            ActiveStandingChairman = dto.ActiveStandingChairman,
            Location = dto.Location,
            IsActive = dto.IsActive
        };
        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, OrganizationDto dto)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return false;

        entity.OrganizationName = dto.OrganizationName;
        entity.OrganizationNameMarathi = dto.OrganizationNameMarathi;
        entity.PrintingHeader = dto.PrintingHeader;
        entity.OrganizationType = dto.OrganizationType;
        entity.Email = dto.Email;
        entity.PhoneNumber1 = dto.PhoneNumber1;
        entity.PhoneNumber2 = dto.PhoneNumber2;
        entity.Website = dto.Website;
        entity.GstNumber = dto.GstNumber;
        entity.PanNumber = dto.PanNumber;
        entity.RegistrationNumber = dto.RegistrationNumber;
        entity.ActiveCommissioner = dto.ActiveCommissioner;
        entity.ActiveMayor = dto.ActiveMayor;
        entity.ActiveDyMayor = dto.ActiveDyMayor;
        entity.ActiveStandingChairman = dto.ActiveStandingChairman;
        entity.Location = dto.Location;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _repo.Update(entity);
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return false;
        _repo.Remove(entity);
        await _repo.SaveChangesAsync();
        return true;
    }
}

public interface IDepartmentService
{
    Task<PagedResultDto<DepartmentDto>> GetPagedAsync(int pageNumber, int pageSize, string? search);
    Task<IEnumerable<DepartmentDto>> GetAllWithServiceCountAsync();
    Task<DepartmentDto?> GetByIdAsync(int id);
    Task<DepartmentDto> CreateAsync(DepartmentDto dto);
    Task<bool> UpdateAsync(int id, DepartmentDto dto);
    Task<bool> DeleteAsync(int id);
}

/// <summary>Department master, scoped to an Organization. Powers the department cards on the dashboard.</summary>
public class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _context;

    public DepartmentService(AppDbContext context) => _context = context;

    private static DepartmentDto ToDto(StaffDepartment d, int serviceCount) => new()
    {
        DepartmentId = d.DepartmentId,
        OrganizationId = d.OrganizationId,
        SrNo = d.SrNo,
        DepartmentName = d.DepartmentName,
        DepartmentNameMarathi = d.DepartmentNameMarathi,
        DepartmentCode = d.DepartmentCode,
        PrimaryFunctions = d.PrimaryFunctions,
        DepartmentHead = d.DepartmentHead,
        Email = d.Email,
        MobileNumber = d.MobileNumber,
        IsActive = d.IsActive,
        ServiceCount = serviceCount
    };

    public async Task<IEnumerable<DepartmentDto>> GetAllWithServiceCountAsync()
    {
        return await _context.StaffDepartments
            .OrderBy(d => d.SrNo)
            .Select(d => new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                OrganizationId = d.OrganizationId,
                SrNo = d.SrNo,
                DepartmentName = d.DepartmentName,
                DepartmentNameMarathi = d.DepartmentNameMarathi ?? string.Empty,
                DepartmentCode = d.DepartmentCode ?? string.Empty,
                PrimaryFunctions = d.PrimaryFunctions ?? string.Empty,
                DepartmentHead = d.DepartmentHead ?? string.Empty,
                Email = d.Email ?? string.Empty,
                MobileNumber = d.MobileNumber ?? string.Empty,
                IsActive = d.IsActive,
                ServiceCount = d.Services.Count(s => s.IsActive)
            })
            .ToListAsync();
    }

    public async Task<PagedResultDto<DepartmentDto>> GetPagedAsync(int pageNumber, int pageSize, string? search)
    {
        var query = _context.StaffDepartments.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.DepartmentName.Contains(search) || d.DepartmentCode.Contains(search));

        var total = await query.CountAsync();
        var items = await query.OrderBy(d => d.SrNo)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(d => new DepartmentDto
            {
                DepartmentId = d.DepartmentId,
                OrganizationId = d.OrganizationId,
                SrNo = d.SrNo,
                DepartmentName = d.DepartmentName,
                DepartmentNameMarathi = d.DepartmentNameMarathi ?? string.Empty,
                DepartmentCode = d.DepartmentCode ?? string.Empty,
                PrimaryFunctions = d.PrimaryFunctions ?? string.Empty,
                DepartmentHead = d.DepartmentHead ?? string.Empty,
                Email = d.Email ?? string.Empty,
                MobileNumber = d.MobileNumber ?? string.Empty,
                IsActive = d.IsActive,
                ServiceCount = d.Services.Count(s => s.IsActive)
            }).ToListAsync();

        return new PagedResultDto<DepartmentDto> { Items = items, TotalCount = total, PageNumber = pageNumber, PageSize = pageSize };
    }

    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var d = await _context.StaffDepartments.Include(x => x.Services).FirstOrDefaultAsync(x => x.DepartmentId == id);
        return d == null ? null : ToDto(d, d.Services.Count(s => s.IsActive));
    }

    public async Task<DepartmentDto> CreateAsync(DepartmentDto dto)
    {
        var entity = new StaffDepartment
        {
            OrganizationId = dto.OrganizationId,
            SrNo = dto.SrNo,
            DepartmentName = dto.DepartmentName,
            DepartmentNameMarathi = dto.DepartmentNameMarathi,
            DepartmentCode = dto.DepartmentCode,
            PrimaryFunctions = dto.PrimaryFunctions,
            DepartmentHead = dto.DepartmentHead,
            Email = dto.Email,
            MobileNumber = dto.MobileNumber,
            IsActive = dto.IsActive
        };
        _context.StaffDepartments.Add(entity);
        await _context.SaveChangesAsync();
        return ToDto(entity, 0);
    }

    public async Task<bool> UpdateAsync(int id, DepartmentDto dto)
    {
        var entity = await _context.StaffDepartments.FindAsync(id);
        if (entity == null) return false;

        entity.DepartmentName = dto.DepartmentName;
        entity.DepartmentNameMarathi = dto.DepartmentNameMarathi;
        entity.DepartmentCode = dto.DepartmentCode;
        entity.PrimaryFunctions = dto.PrimaryFunctions;
        entity.DepartmentHead = dto.DepartmentHead;
        entity.Email = dto.Email;
        entity.MobileNumber = dto.MobileNumber;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.StaffDepartments.FindAsync(id);
        if (entity == null) return false;
        _context.StaffDepartments.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
