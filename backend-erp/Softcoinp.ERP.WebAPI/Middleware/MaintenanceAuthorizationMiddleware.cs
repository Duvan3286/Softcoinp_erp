using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace Softcoinp.ERP.WebAPI.Middleware;

/// <summary>
/// Blocks access to Maintenance module endpoints for non-SuperAdmin users.
/// Returns HTTP 404 with a generic message to avoid revealing that the endpoint exists.
/// </summary>
public class MaintenanceAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public MaintenanceAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/v1/maintenance"))
        {
            var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
            if (!isAuthenticated)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    Error = "Not Found",
                    Message = "The requested resource was not found."
                }));
                return;
            }

            var roleClaim = context.User.FindFirstValue(ClaimTypes.Role);
            if (roleClaim != "SuperAdmin")
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    Error = "Not Found",
                    Message = "The requested resource was not found."
                }));
                return;
            }
        }

        await _next(context);
    }
}
