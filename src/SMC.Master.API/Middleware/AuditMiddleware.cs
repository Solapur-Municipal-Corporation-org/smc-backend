namespace SMC.Master.API.Middleware;

// Logs who did what and when, for the citizen/department/integrated-services
// audit trail required by SMC compliance.
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User?.Identity?.Name ?? "anonymous";
        _logger.LogInformation("{Method} {Path} by {User}", context.Request.Method, context.Request.Path, user);
        await _next(context);
    }
}
