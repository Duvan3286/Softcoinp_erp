namespace Softcoinp.ERP.Domain.External;

/// <summary>
/// Data Transfer Object for Core Unit information received from Project A.
/// </summary>
public class CoreUnitDto
{
    public Guid Id { get; set; }
    public string Tower { get; set; } = string.Empty;
    public string Apartment { get; set; } = string.Empty;
    public decimal Coefficient { get; set; }
}
