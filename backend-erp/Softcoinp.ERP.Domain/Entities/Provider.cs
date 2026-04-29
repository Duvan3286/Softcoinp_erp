using Softcoinp.ERP.Domain.Common;

namespace Softcoinp.ERP.Domain.Entities;

/// <summary>
/// Represents a third-party provider or service supplier.
/// </summary>
public class Provider : BaseEntity
{
    /// <summary>
    /// National Tax ID (NIT).
    /// </summary>
    public string NIT { get; set; } = string.Empty;

    /// <summary>
    /// Legal name of the provider.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Contact email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Category or type of service provided.
    /// </summary>
    public string ServiceType { get; set; } = string.Empty;
}
