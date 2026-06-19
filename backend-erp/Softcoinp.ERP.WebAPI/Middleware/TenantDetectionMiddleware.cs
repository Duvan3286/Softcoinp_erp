using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Softcoinp.ERP.Domain.Interfaces;

namespace Softcoinp.ERP.WebAPI.Middleware;

/// <summary>
/// Middleware to detect the current tenant from the request subdomain and block unauthorized access.
/// </summary>
public class TenantDetectionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantDetectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver)
    {
        // Skip tenant detection ONLY for global admin routes (management of tenants themselves)
        if (context.Request.Path.StartsWithSegments("/api/v1/admin/tenants"))
        {
            await _next(context);
            return;
        }

        var tenant = await tenantResolver.GetCurrentTenantAsync();

        if (tenant == null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/json";
            
            var response = new 
            { 
                Error = "Tenant Not Found", 
                Message = "The requested business subdomain is not registered or is inactive. Please contact Softcoinp Support." 
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        // Add tenant info to items for downstream usage if needed
        context.Items["Tenant"] = tenant;

        await _next(context);
    }
}
