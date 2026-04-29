using Softcoinp.ERP.Domain.External;

namespace Softcoinp.ERP.Domain.Interfaces;

/// <summary>
/// Client interface for interacting with the Core system (Project A).
/// </summary>
public interface ICoreIntegrationClient
{
    Task<CoreUnitDto?> GetUnitByIdAsync(Guid unitId);
    Task<IEnumerable<CoreUnitDto>> GetAllUnitsAsync();
}
