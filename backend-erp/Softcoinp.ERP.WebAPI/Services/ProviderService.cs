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
                p.TradeName.ToLower().Contains(searchLower) ||
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
                TradeName = p.TradeName,
                ContactName = p.ContactName,
                Email = p.Email,
                Phone = p.Phone,
                City = p.City,
                ServiceType = p.ServiceType,
                IsPreferred = p.IsPreferred,
                Status = p.Status.ToString(),
                ContractCount = p.Contracts.Count,
                ActiveContractCount = p.Contracts.Count(c =>
                    c.Status == ContractStatus.Active ||
                    c.Status == ContractStatus.Draft),
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
                VerificationDigit = p.VerificationDigit,
                BusinessName = p.BusinessName,
                TradeName = p.TradeName,
                ContactName = p.ContactName,
                Email = p.Email,
                Phone = p.Phone,
                Address = p.Address,
                City = p.City,
                EconomicActivity = p.EconomicActivity,
                ServiceType = p.ServiceType,
                RutFilePath = p.RutFilePath,
                LegalRepDocumentType = p.LegalRepDocumentType,
                LegalRepDocumentNumber = p.LegalRepDocumentNumber,
                LegalRepName = p.LegalRepName,
                LegalRepEmail = p.LegalRepEmail,
                IsPreferred = p.IsPreferred,
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
                        Status = c.Status.ToString()
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
            VerificationDigit = request.VerificationDigit,
            BusinessName = request.BusinessName,
            TradeName = request.TradeName,
            ContactName = request.ContactName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            EconomicActivity = request.EconomicActivity,
            ServiceType = request.ServiceType,
            RutFilePath = request.RutFilePath,
            LegalRepDocumentType = request.LegalRepDocumentType,
            LegalRepDocumentNumber = request.LegalRepDocumentNumber,
            LegalRepName = request.LegalRepName,
            LegalRepEmail = request.LegalRepEmail,
            IsPreferred = request.IsPreferred,
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
        if (request.VerificationDigit != null) provider.VerificationDigit = request.VerificationDigit;
        if (request.BusinessName != null) provider.BusinessName = request.BusinessName;
        if (request.TradeName != null) provider.TradeName = request.TradeName;
        if (request.ContactName != null) provider.ContactName = request.ContactName;
        if (request.Email != null) provider.Email = request.Email;
        if (request.Phone != null) provider.Phone = request.Phone;
        if (request.Address != null) provider.Address = request.Address;
        if (request.City != null) provider.City = request.City;
        if (request.EconomicActivity != null) provider.EconomicActivity = request.EconomicActivity;
        if (request.ServiceType != null) provider.ServiceType = request.ServiceType;
        if (request.RutFilePath != null) provider.RutFilePath = request.RutFilePath;
        if (request.LegalRepDocumentType != null) provider.LegalRepDocumentType = request.LegalRepDocumentType;
        if (request.LegalRepDocumentNumber != null) provider.LegalRepDocumentNumber = request.LegalRepDocumentNumber;
        if (request.LegalRepName != null) provider.LegalRepName = request.LegalRepName;
        if (request.LegalRepEmail != null) provider.LegalRepEmail = request.LegalRepEmail;
        if (request.IsPreferred.HasValue) provider.IsPreferred = request.IsPreferred.Value;

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

        var hasActiveContracts = await _context.Contracts
            .AnyAsync(c => c.ProviderId == providerId &&
                (c.Status == ContractStatus.Active || c.Status == ContractStatus.Draft));

        if (hasActiveContracts)
        {
            throw new InvalidOperationException("No se puede eliminar el proveedor porque tiene contratos activos o en borrador.");
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

        if (request.ServiceQualityScore < 1 || request.ServiceQualityScore > 5 ||
            request.ComplianceScore < 1 || request.ComplianceScore > 5 ||
            request.PriceFairnessScore < 1 || request.PriceFairnessScore > 5 ||
            request.AfterSalesScore < 1 || request.AfterSalesScore > 5)
        {
            throw new ArgumentException("Las calificaciones deben estar entre 1 y 5.");
        }

        var averageScore = (decimal)(request.ServiceQualityScore + request.ComplianceScore +
            request.PriceFairnessScore + request.AfterSalesScore) / 4m;

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
            ContractId = request.ContractId,
            EvaluationPeriod = request.EvaluationPeriod,
            ServiceQualityScore = request.ServiceQualityScore,
            ComplianceScore = request.ComplianceScore,
            PriceFairnessScore = request.PriceFairnessScore,
            AfterSalesScore = request.AfterSalesScore,
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

    public async Task<ProviderIndicatorsDto> GetIndicatorsAsync(string tenantId)
    {
        var providerStats = await _context.Providers
            .Where(p => p.TenantId == tenantId)
            .GroupBy(p => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(p => p.Status == ProviderStatus.Active),
                Inactive = g.Count(p => p.Status == ProviderStatus.Inactive),
                Preferred = g.Count(p => p.IsPreferred && p.Status == ProviderStatus.Active)
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
                Pending = g.Count(i => i.Status == InvoiceStatus.Pending),
                PendingAmount = g.Where(i => i.Status == InvoiceStatus.Pending).Sum(i => i.NetAmount),
                Overdue = g.Count(i => i.Status == InvoiceStatus.Overdue)
            })
            .FirstOrDefaultAsync();

        var activeAlerts = await _context.ContractAlerts
            .CountAsync(a => a.TenantId == tenantId && a.IsActive);

        var expiringPolicies = await _context.ContractPolicies
            .CountAsync(p => p.TenantId == tenantId &&
                p.IsActive &&
                p.EndDate <= DateTime.UtcNow.AddDays(30));

        return new ProviderIndicatorsDto
        {
            TotalProviders = providerStats?.Total ?? 0,
            ActiveProviders = providerStats?.Active ?? 0,
            InactiveProviders = providerStats?.Inactive ?? 0,
            PreferredProviders = providerStats?.Preferred ?? 0,
            TotalContracts = contractStats?.Total ?? 0,
            ActiveContracts = contractStats?.Active ?? 0,
            ExpiringContracts = contractStats?.Expiring ?? 0,
            TotalContractValue = contractStats?.TotalValue ?? 0m,
            MonthlyContractValue = contractStats?.MonthlyValue ?? 0m,
            PendingInvoices = invoiceStats?.Pending ?? 0,
            PendingInvoiceAmount = invoiceStats?.PendingAmount ?? 0m,
            OverdueInvoices = invoiceStats?.Overdue ?? 0,
            ActiveAlerts = activeAlerts,
            ExpiringPolicies = expiringPolicies
        };
    }
}
