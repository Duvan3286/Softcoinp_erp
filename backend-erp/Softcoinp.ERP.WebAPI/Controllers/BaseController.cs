using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Authorize]
public abstract class BaseController : ControllerBase
{
    protected string GetTenantId() => User.FindFirstValue("tenant_id") ?? string.Empty;

    protected string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}
