using Softcoinp.ERP.Domain.Common;

namespace Softcoinp.ERP.Domain.Entities;

public enum TransactionType
{
    Income,
    Expense,
    Adjustment
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Cancelled
}

/// <summary>
/// Represents a financial transaction linked to an external unit from Project A.
/// </summary>
public class FinancialTransaction : BaseEntity
{
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; }
    public string Reference { get; set; } = string.Empty;
    public TransactionStatus Status { get; set; }
    
    /// <summary>
    /// ID of the unit in the Core system (Project A).
    /// </summary>
    public Guid ExternalUnitId { get; set; }
}
