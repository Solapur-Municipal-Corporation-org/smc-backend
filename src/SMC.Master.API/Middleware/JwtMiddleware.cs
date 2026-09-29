namespace SMC.Master.API.Middleware;

// Placeholder for extra per-request JWT context enrichment beyond
// what the built-in JwtBearer handler already does (e.g. attaching a
// resolved Citizen/Employee record to HttpContext.Items).
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    public JwtMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);
    }
}
