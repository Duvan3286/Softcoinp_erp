using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Softcoinp.ERP.WebAPI.Middleware;

public class CsrfValidationFilter : IActionFilter
{
    private static readonly string[] _excludedPaths =
    {
        "/api/auth/login",
        "/api/auth/refresh"
    };

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var path = context.HttpContext.Request.Path.Value ?? "";
        if (_excludedPaths.Any(p => path.StartsWith(p)))
            return;

        var method = context.HttpContext.Request.Method;
        if (method != "POST" && method != "PUT" && method != "DELETE" && method != "PATCH")
            return;

        if (!context.HttpContext.Request.Headers.ContainsKey("X-Requested-With"))
        {
            context.Result = new BadRequestObjectResult(new
            {
                message = "Solicitud invalida. Falta proteccion CSRF."
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
