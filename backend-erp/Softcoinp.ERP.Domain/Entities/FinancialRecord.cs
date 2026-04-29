using System;
using Softcoinp.ERP.Domain.Common;

namespace Softcoinp.ERP.Domain.Entities;

/// <summary>
/// Represents a financial transaction within the ERP.
/// </summary>
public class FinancialRecord : BaseEntity
{
    /// <summary>
    /// Transaction amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Type of transaction (e.g., Income, Expense).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the financial record.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Reference to an external system ID (e.g., Project A's ID) to maintain traceability.
    /// </summary>
    public string? ExternalReferenceId { get; set; }
}
