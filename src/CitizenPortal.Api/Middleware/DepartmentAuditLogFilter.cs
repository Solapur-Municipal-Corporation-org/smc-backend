using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using CitizenPortal.Api.Department.Entities;
using CitizenPortal.Api.Data;

namespace CitizenPortal.Api.Middleware;

/// <summary>Records every write operation (POST/PUT/DELETE) on masters/transactions to the AuditLog table.</summary>
public class AuditLogFilter : IAsyncActionFilter
{
    private readonly AppDbContext _context;
    public AuditLogFilter(AppDbContext context) => _context = context;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = await next();

        var method = context.HttpContext.Request.Method;
        if (method is "GET" or "OPTIONS") return;
        if (result.Exception != null) return;

        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // Citizen accounts use GUID identities while Department Portal accounts use int IDs.
        // The shared audit table's UserId is nullable int, so record null for non-integer
        // identities instead of throwing after the action has already completed successfully.
        int? userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : null;
        var entry = new AuditLog
        {
            UserId = userId,
            Action = method,
            EntityName = context.Controller.GetType().Name.Replace("Controller", ""),
            EntityId = context.ActionArguments.ContainsKey("id") ? context.ActionArguments["id"]?.ToString() : null,
            IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        _context.AuditLogs.Add(entry);
        await _context.SaveChangesAsync();
    }
}
