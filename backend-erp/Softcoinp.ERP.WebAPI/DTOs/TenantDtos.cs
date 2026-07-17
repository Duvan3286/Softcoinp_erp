namespace Softcoinp.ERP.WebAPI.DTOs;

public record CreateTenantDto
{
    public string Subdomain { get; init; } = string.Empty;
}

public record ToggleTenantStatusResponse
{
    public Guid Id { get; init; }
    public bool IsActive { get; init; }
}
