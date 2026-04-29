using Softcoinp.ERP.Domain.Interfaces;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.External;

namespace Softcoinp.ERP.Application.Services;

/// <summary>
/// Service for managing billing operations and integration with the Core system.
/// </summary>
public class BillingService
{
    private readonly ICoreIntegrationClient _coreClient;
    private readonly IUnitOfWork _unitOfWork;

    public BillingService(ICoreIntegrationClient coreClient, IUnitOfWork unitOfWork)
    {
        _coreClient = coreClient;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Calculates the administration fee for a unit based on a base value and its coefficient.
    /// </summary>
    /// <param name="unitId">The external unit ID from Project A.</param>
    /// <param name="baseValue">The base monthly administration value.</param>
    /// <returns>The calculated administration fee.</returns>
    public async Task<decimal> CalculateAdministrationFeeAsync(Guid unitId, decimal baseValue)
    {
        var unit = await _coreClient.GetUnitByIdAsync(unitId);
        
        if (unit == null)
        {
            throw new KeyNotFoundException($"Unit with ID {unitId} was not found in the Core system.");
        }

        // Fee = Base Value * Coefficient
        return baseValue * unit.Coefficient;
    }

    /// <summary>
    /// Generates a pending financial transaction for a unit's administration fee.
    /// </summary>
    public async Task<FinancialTransaction> GenerateAdministrationChargeAsync(Guid unitId, decimal baseValue)
    {
        var amount = await CalculateAdministrationFeeAsync(unitId, baseValue);
        
        var transaction = new FinancialTransaction
        {
            Amount = amount,
            Type = TransactionType.Income,
            Date = DateTime.UtcNow,
            Reference = $"Admin Charge - {DateTime.UtcNow:MMMM yyyy}",
            Status = TransactionStatus.Pending,
            ExternalUnitId = unitId
        };

        var repository = _unitOfWork.GetRepository<FinancialTransaction>();
        await repository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return transaction;
    }
}
