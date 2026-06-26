namespace Softcoinp.ERP.WebAPI.DTOs;

public record CreateTenantDto
{
    public string Name { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public string ConnectionString { get; init; } = string.Empty;
}
