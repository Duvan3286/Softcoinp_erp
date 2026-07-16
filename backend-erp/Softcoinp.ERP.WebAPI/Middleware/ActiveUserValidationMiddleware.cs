using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Softcoinp.ERP.Domain.Entities;

namespace Softcoinp.ERP.WebAPI.Middleware;

/// <summary>
/// Rejects requests carrying a still-valid JWT for a user that has since been
/// suspended or deactivated, closing the window between admin action and token expiry.
/// </summary>
public class ActiveUserValidationMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveUserValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<User> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null || user.IsSuspended || !user.IsActive)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(
                        "{\"error\":\"Account Suspended\",\"message\":\"La sesión ha sido invalidada porque el usuario fue suspendido o desactivado.\"}");
                    return;
                }
            }
        }

        await _next(context);
    }
}
