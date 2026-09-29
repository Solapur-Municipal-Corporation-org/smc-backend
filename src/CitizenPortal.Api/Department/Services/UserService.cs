using CitizenPortal.Api.Data;
using CitizenPortal.Api.Department.DTOs;
using CitizenPortal.Api.Department.Entities;
using Microsoft.EntityFrameworkCore;

namespace CitizenPortal.Api.Department.Services;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync(int? restrictToDepartmentId);
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(UserDto dto);
    Task<bool> UpdateAsync(int id, UserDto dto);
    Task<bool> DeleteAsync(int id);
}

/// <summary>
/// User Master (staff accounts). Not a plain GenericCrudController for two reasons: the entry
/// form's plain-text password must be hashed before storage and PasswordHash/OtpCode must never
/// be returned to the client, AND list reads must be department-scoped (item 6/7 — a
/// DepartmentAdmin managing their own department's user list must not see other departments'
/// users; pass null for SystemAdmin, who sees everyone).
/// </summary>
public class UserService : IUserService
{
    private readonly AppDbContext _context;
    public UserService(AppDbContext context) => _context = context;

    private static UserDto ToDto(AppUser u) => new()
    {
        UserId = u.UserId,
        DepartmentId = u.DepartmentId,
        FullName = u.FullName,
        MobileNumber = u.MobileNumber,
        Email = u.Email,
        Role = u.Role.ToString(),
        IsActive = u.IsActive
        // Password / PasswordHash / OtpCode intentionally omitted
    };

    public async Task<IEnumerable<UserDto>> GetAllAsync(int? restrictToDepartmentId)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();
        if (restrictToDepartmentId is not null)
            query = query.Where(u => u.DepartmentId == restrictToDepartmentId);
        return (await query.ToListAsync()).Select(ToDto);
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var u = await _context.Users.FindAsync(id);
        return u == null ? null : ToDto(u);
    }

    public async Task<UserDto> CreateAsync(UserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new InvalidOperationException("Password is required when creating a user.");
        if (!Enum.TryParse<UserRole>(dto.Role, out var role))
            throw new InvalidOperationException($"Unknown role '{dto.Role}'.");
        if (await _context.Users.AnyAsync(u => u.MobileNumber == dto.MobileNumber))
            throw new InvalidOperationException("A user with this mobile number already exists.");

        var entity = new AppUser
        {
            DepartmentId = dto.DepartmentId,
            FullName = dto.FullName,
            MobileNumber = dto.MobileNumber,
            Email = dto.Email,
            Role = role,
            IsActive = dto.IsActive,
            // Password hash kept as an emergency/admin-console fallback credential even though
            // the Department Portal's normal login is OTP-only (item 5) — never exposed to the
            // OTP flow itself.
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };
        _context.Users.Add(entity);
        await _context.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, UserDto dto)
    {
        var entity = await _context.Users.FindAsync(id);
        if (entity == null) return false;
        if (!Enum.TryParse<UserRole>(dto.Role, out var role))
            throw new InvalidOperationException($"Unknown role '{dto.Role}'.");

        entity.DepartmentId = dto.DepartmentId;
        entity.FullName = dto.FullName;
        entity.MobileNumber = dto.MobileNumber;
        entity.Email = dto.Email;
        entity.Role = role;
        entity.IsActive = dto.IsActive;

        if (!string.IsNullOrWhiteSpace(dto.Password))
            entity.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Users.FindAsync(id);
        if (entity == null) return false;
        _context.Users.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
