using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class ProviderService
{
    private readonly ApplicationDbContext _context;

    public ProviderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProviderListDto>> GetProvidersAsync(
        string tenantId,
        string? status = null,
        string? providerType = null,
        string? serviceType = null,
        string? search = null)
    {
        var query = _context.Providers
            .Where(p => p.TenantId == tenantId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ProviderStatus>(status, true, out var statusEnum))
        {
            query = query.Where(p => p.Status == statusEnum);
        }

        if (!string.IsNullOrEmpty(providerType) && Enum.TryParse<ProviderType>(providerType, true, out var typeEnum))
        {
            query = query.Where(p => p.ProviderType == typeEnum);
        }

        if (!string.IsNullOrEmpty(serviceType))
        {
            query = query.Where(p => p.ServiceType == serviceType);
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p =>
                p.BusinessName.ToLower().Contains(searchLower) ||
                p.DocumentNumber.Contains(search) ||
                p.ContactName.ToLower().Contains(searchLower));
        }

        var providers = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProviderListDto
            {
                Id = p.Id,
                ProviderType = p.ProviderType.ToString(),
                DocumentNumber = p.DocumentNumber,
                BusinessName = p.BusinessName,
                ContactName = p.ContactName,
                Email = p.Email,
                Phone = p.Phone,
                ServiceType = p.ServiceType,
                Status = p.Status.ToString(),
                ContractCount = p.Contracts.Count,
                ActiveContractCount = p.Contracts.Count(c => c.Status == ContractStatus.Active),
                AverageEvaluationScore = p.Evaluations
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(2)
                    .Average(e => (decimal?)e.AverageScore) ?? 0m,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return providers;
    }

    public async Task<ProviderDetailDto> GetProviderByIdAsync(string tenantId, Guid providerId)
    {
        var provider = await _context.Providers
            .Where(p => p.Id == providerId && p.TenantId == tenantId)
            .Select(p => new ProviderDetailDto
            {
                Id = p.Id,
                ProviderType = p.ProviderType.ToString(),
                DocumentType = p.DocumentType,
                DocumentNumber = p.DocumentNumber,
                BusinessName = p.BusinessName,
                ContactName = p.ContactName,
                Email = p.Email,
                Phone = p.Phone,
                Address = p.Address,
                ServiceType = p.ServiceType,
                RutFilePath = p.RutFilePath,
                ChamberOfCommerceFilePath = p.ChamberOfCommerceFilePath,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Contracts = p.Contracts
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new ProviderContractSummaryDto
                    {
                        Id = c.Id,
                        ContractNumber = c.ContractNumber,
                        ContractType = c.ContractType.ToString(),
                        TotalValue = c.TotalValue,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Status = c.Status.ToString(),
                        DaysUntilExpiration = (int)(c.EndDate - DateTime.UtcNow).TotalDays
                    })
                    .ToList(),
                Invoices = p.Invoices
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new ProviderInvoiceSummaryDto
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        ContractNumber = i.Contract != null ? i.Contract.ContractNumber : string.Empty,
                        TotalAmount = i.TotalAmount,
                        AmountPaid = i.AmountPaid,
                        PendingAmount = i.TotalAmount - i.AmountPaid,
                        DueDate = i.DueDate,
                        Status = i.Status.ToString(),
                        BudgetItemName = string.Empty
                    })
                    .ToList(),
                Evaluations = p.Evaluations
                    .OrderByDescending(e => e.CreatedAt)
                    .Select(e => new ProviderEvaluationSummaryDto
                    {
                        Id = e.Id,
                        EvaluationPeriod = e.EvaluationPeriod,
                        AverageScore = e.AverageScore,
                        Recommendation = e.Recommendation.ToString(),
                        EvaluatedByUserName = e.EvaluatedByUserName,
                        CreatedAt = e.CreatedAt
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (provider == null)
        {
            throw new KeyNotFoundException("Proveedor no encontrado.");
        }

        return provider;
    }

    public async Task<ProviderDetailDto> CreateProviderAsync(string tenantId, string userId, CreateProviderRequestDto request)
    {
        if (!Enum.TryParse<ProviderType>(request.ProviderType, true, out var providerType))
        {
            throw new ArgumentException("Tipo de proveedor inválido. Use: Natural o Legal.");
        }

        var existingProvider = await _context.Providers
            .AnyAsync(p => p.TenantId == tenantId && p.DocumentNumber == request.DocumentNumber);

        if (existingProvider)
        {
            throw new InvalidOperationException("Ya existe un proveedor con ese número de documento.");
        }

        var provider = new Provider
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProviderType = providerType,
            DocumentType = request.DocumentType,
            DocumentNumber = request.DocumentNumber,
            BusinessName = request.BusinessName,
            ContactName = request.ContactName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            ServiceType = request.ServiceType,
            RutFilePath = request.RutFilePath,
            ChamberOfCommerceFilePath = request.ChamberOfCommerceFilePath,
            Status = ProviderStatus.Active,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Providers.Add(provider);
        await _context.SaveChangesAsync();

        return await GetProviderByIdAsync(tenantId, provider.Id);
    }

    public async Task<ProviderDetailDto> UpdateProviderAsync(string tenantId, string userId, Guid providerId, UpdateProviderRequestDto request)
    {
        var provider = await _context.Providers
            .FirstOrDefaultAsync(p => p.Id == providerId && p.TenantId == tenantId);

        if (provider == null)
        {
            throw new KeyNotFoundException("Proveedor no encontrado.");
        }

        if (request.ProviderType != null)
        {
            if (!Enum.TryParse<ProviderType>(request.ProviderType, true, out var providerType))
            {
                throw new ArgumentException("Tipo de proveedor inválido.");
            }
            provider.ProviderType = providerType;
        }

        if (request.DocumentType != null) provider.DocumentType = request.DocumentType;
        if (request.DocumentNumber != null) provider.DocumentNumber = request.DocumentNumber;
        if (request.BusinessName != null) provider.BusinessName = request.BusinessName;
        if (request.ContactName != null) provider.ContactName = request.ContactName;
        if (request.Email != null) provider.Email = request.Email;
        if (request.Phone != null) provider.Phone = request.Phone;
        if (request.Address != null) provider.Address = request.Address;
        if (request.ServiceType != null) provider.ServiceType = request.ServiceType;
        if (request.RutFilePath != null) provider.RutFilePath = request.RutFilePath;
        if (request.ChamberOfCommerceFilePath != null) provider.ChamberOfCommerceFilePath = request.ChamberOfCommerceFilePath;

        if (request.Status != null)
        {
            if (!Enum.TryParse<ProviderStatus>(request.Status, true, out var statusEnum))
            {
                throw new ArgumentException("Estado inválido. Use: Active o Inactive.");
            }
            provider.Status = statusEnum;
        }

        provider.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetProviderByIdAsync(tenantId, provider.Id);
    }

    public async Task DeleteProviderAsync(string tenantId, Guid providerId)
    {
        var provider = await _context.Providers
            .FirstOrDefaultAsync(p => p.Id == providerId && p.TenantId == tenantId);

        if (provider == null)
        {
            throw new KeyNotFoundException("Proveedor no encontrado.");
        }

        var hasContracts = await _context.Contracts
            .AnyAsync(c => c.ProviderId == providerId);

        if (hasContracts)
        {
            throw new InvalidOperationException("No se puede eliminar el proveedor porque tiene contratos asociados.");
        }

        var hasInvoices = await _context.ProviderInvoices
            .AnyAsync(i => i.ProviderId == providerId);

        if (hasInvoices)
        {
            throw new InvalidOperationException("No se puede eliminar el proveedor porque tiene facturas asociadas.");
        }

        provider.IsDeleted = true;
        provider.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<List<ProviderEvaluationSummaryDto>> GetProviderEvaluationsAsync(string tenantId, Guid providerId)
    {
        var providerExists = await _context.Providers
            .AnyAsync(p => p.Id == providerId && p.TenantId == tenantId);

        if (!providerExists)
        {
            throw new KeyNotFoundException("Proveedor no encontrado.");
        }

        return await _context.ProviderEvaluations
            .Where(e => e.ProviderId == providerId && e.TenantId == tenantId)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new ProviderEvaluationSummaryDto
            {
                Id = e.Id,
                EvaluationPeriod = e.EvaluationPeriod,
                AverageScore = e.AverageScore,
                Recommendation = e.Recommendation.ToString(),
                EvaluatedByUserName = e.EvaluatedByUserName,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ProviderEvaluationSummaryDto> CreateProviderEvaluationAsync(
        string tenantId, string userId, string userName, Guid providerId, CreateProviderEvaluationRequestDto request)
    {
        var provider = await _context.Providers
            .FirstOrDefaultAsync(p => p.Id == providerId && p.TenantId == tenantId);

        if (provider == null)
        {
            throw new KeyNotFoundException("Proveedor no encontrado.");
        }

        if (request.QualityScore < 1 || request.QualityScore > 5 ||
            request.ComplianceScore < 1 || request.ComplianceScore > 5 ||
            request.PriceScore < 1 || request.PriceScore > 5 ||
            request.AttentionScore < 1 || request.AttentionScore > 5)
        {
            throw new ArgumentException("Las calificaciones deben estar entre 1 y 5.");
        }

        var averageScore = (decimal)(request.QualityScore + request.ComplianceScore +
            request.PriceScore + request.AttentionScore) / 4m;

        EvaluationRecommendation recommendation;
        if (averageScore >= 4.0m)
        {
            recommendation = EvaluationRecommendation.Renew;
        }
        else if (averageScore >= 2.5m)
        {
            recommendation = EvaluationRecommendation.EvaluateOtherOptions;
        }
        else
        {
            recommendation = EvaluationRecommendation.DoNotRenew;
        }

        var evaluation = new ProviderEvaluation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProviderId = providerId,
            EvaluationPeriod = request.EvaluationPeriod,
            QualityScore = request.QualityScore,
            ComplianceScore = request.ComplianceScore,
            PriceScore = request.PriceScore,
            AttentionScore = request.AttentionScore,
            AverageScore = Math.Round(averageScore, 2),
            Comments = request.Comments,
            Recommendation = recommendation,
            EvaluatedByUserId = userId,
            EvaluatedByUserName = userName,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProviderEvaluations.Add(evaluation);
        await _context.SaveChangesAsync();

        return new ProviderEvaluationSummaryDto
        {
            Id = evaluation.Id,
            EvaluationPeriod = evaluation.EvaluationPeriod,
            AverageScore = evaluation.AverageScore,
            Recommendation = evaluation.Recommendation.ToString(),
            EvaluatedByUserName = evaluation.EvaluatedByUserName,
            CreatedAt = evaluation.CreatedAt
        };
    }

    public async Task<decimal> GetAverageLastTwoEvaluationsAsync(string tenantId, Guid providerId)
    {
        var scores = await _context.ProviderEvaluations
            .Where(e => e.ProviderId == providerId && e.TenantId == tenantId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(2)
            .Select(e => e.AverageScore)
            .ToListAsync();

        if (scores.Count == 0)
        {
            return 0m;
        }

        return scores.Average();
    }

    public async Task<ProviderIndicatorsDto> GetIndicatorsAsync(string tenantId)
    {
        var providerStats = await _context.Providers
            .Where(p => p.TenantId == tenantId)
            .GroupBy(p => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(p => p.Status == ProviderStatus.Active),
                Inactive = g.Count(p => p.Status == ProviderStatus.Inactive)
            })
            .FirstOrDefaultAsync();

        var contractStats = await _context.Contracts
            .Where(c => c.TenantId == tenantId)
            .GroupBy(c => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(c => c.Status == ContractStatus.Active),
                Expiring = g.Count(c => c.Status == ContractStatus.Active && c.EndDate <= DateTime.UtcNow.AddDays(90)),
                TotalValue = g.Where(c => c.Status == ContractStatus.Active).Sum(c => c.TotalValue),
                MonthlyValue = g.Where(c => c.Status == ContractStatus.Active).Sum(c => c.MonthlyValue)
            })
            .FirstOrDefaultAsync();

        var invoiceStats = await _context.ProviderInvoices
            .Where(i => i.TenantId == tenantId)
            .GroupBy(i => 1)
            .Select(g => new
            {
                Pending = g.Count(i => i.Status != InvoiceStatus.FullyPaid),
                PendingAmount = g.Where(i => i.Status != InvoiceStatus.FullyPaid)
                    .Sum(i => i.TotalAmount - i.AmountPaid),
                Overdue = g.Count(i => i.Status != InvoiceStatus.FullyPaid && i.DueDate < DateTime.UtcNow)
            })
            .FirstOrDefaultAsync();

        var activeAlerts = await _context.ContractAlerts
            .CountAsync(a => a.TenantId == tenantId && a.IsActive);

        return new ProviderIndicatorsDto
        {
            TotalProviders = providerStats?.Total ?? 0,
            ActiveProviders = providerStats?.Active ?? 0,
            InactiveProviders = providerStats?.Inactive ?? 0,
            TotalContracts = contractStats?.Total ?? 0,
            ActiveContracts = contractStats?.Active ?? 0,
            ExpiringContracts = contractStats?.Expiring ?? 0,
            TotalContractValue = contractStats?.TotalValue ?? 0m,
            MonthlyContractValue = contractStats?.MonthlyValue ?? 0m,
            PendingPaymentInvoices = invoiceStats?.Pending ?? 0,
            PendingPaymentAmount = invoiceStats?.PendingAmount ?? 0m,
            OverdueInvoices = invoiceStats?.Overdue ?? 0,
            ActiveAlerts = activeAlerts
        };
    }
}
