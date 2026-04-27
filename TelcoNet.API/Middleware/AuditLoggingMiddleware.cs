using System.Diagnostics;
using System.Security.Claims;
using TelcoNet.Data;
using TelcoNet.Data.Entities;

namespace TelcoNet.API.Middleware;

/// <summary>
/// Logs every API request to the AuditLog table for security compliance.
/// </summary>
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public AuditLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        // Don't log Swagger or static file requests
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/swagger") || path.StartsWith("/favicon"))
            return;

        try
        {
            using var scope = context.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var auditLog = new AuditLog
            {
                UserEmail = context.User?.FindFirst(ClaimTypes.Email)?.Value,
                HttpMethod = context.Request.Method,
                Endpoint = path,
                StatusCode = context.Response.StatusCode,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                DurationMs = stopwatch.ElapsedMilliseconds,
                Timestamp = DateTime.UtcNow
            };

            db.AuditLogs.Add(auditLog);
            await db.SaveChangesAsync();
        }
        catch
        {
            // Don't let audit logging failures crash the app
        }
    }
}
