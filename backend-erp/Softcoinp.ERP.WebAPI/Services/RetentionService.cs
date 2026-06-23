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

public class RetentionService
{
    private readonly ApplicationDbContext _context;

    public RetentionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public RetentionCalculationDto CalculateRetentions(string tenantId, string serviceType, decimal subtotal)
    {
        var config = _context.RetentionConfigurations
            .FirstOrDefault(r => r.TenantId == tenantId &&
                r.ServiceType == serviceType &&
                r.IsActive);

        if (config == null)
        {
            throw new KeyNotFoundException($"No se encontró configuración de retención para el servicio: {serviceType}");
        }

        var ivaRate = 0.19m;
        var ivaAmount = subtotal * ivaRate;

        var retentionFuelAmount = subtotal * config.RetentionFuelRate;
        var retentionIcaAmount = subtotal * config.RetentionIcaRate;
        var totalRetentions = retentionFuelAmount + retentionIcaAmount;
        var netAmount = subtotal + ivaAmount - totalRetentions;

        return new RetentionCalculationDto
        {
            Subtotal = subtotal,
            IvaAmount = Math.Round(ivaAmount, 2),
            RetentionFuelAmount = Math.Round(retentionFuelAmount, 2),
            RetentionIcaAmount = Math.Round(retentionIcaAmount, 2),
            TotalRetentions = Math.Round(totalRetentions, 2),
            NetAmount = Math.Round(netAmount, 2),
            Details = new List<RetentionDetailDto>
            {
                new RetentionDetailDto
                {
                    ServiceType = "Retención en la Fuente",
                    Rate = config.RetentionFuelRate,
                    BaseAmount = subtotal,
                    RetentionAmount = Math.Round(retentionFuelAmount, 2)
                },
                new RetentionDetailDto
                {
                    ServiceType = "Retención ICA",
                    Rate = config.RetentionIcaRate,
                    BaseAmount = subtotal,
                    RetentionAmount = Math.Round(retentionIcaAmount, 2)
                }
            }
        };
    }

    public async Task<List<RetentionConfigurationDto>> GetRetentionConfigurationsAsync(string tenantId)
    {
        return await _context.RetentionConfigurations
            .Where(r => r.TenantId == tenantId)
            .OrderBy(r => r.ServiceType)
            .Select(r => new RetentionConfigurationDto
            {
                Id = r.Id,
                ServiceType = r.ServiceType,
                ServiceDescription = r.ServiceDescription,
                RetentionFuelRate = r.RetentionFuelRate,
                RetentionIcaRate = r.RetentionIcaRate,
                IsActive = r.IsActive
            })
            .ToListAsync();
    }

    public async Task<RetentionConfigurationDto> CreateRetentionConfigurationAsync(string tenantId, string userId, CreateRetentionConfigurationRequestDto request)
    {
        var existing = await _context.RetentionConfigurations
            .AnyAsync(r => r.TenantId == tenantId && r.ServiceType == request.ServiceType);

        if (existing)
        {
            throw new InvalidOperationException("Ya existe una configuración de retención para este tipo de servicio.");
        }

        var config = new RetentionConfiguration
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ServiceType = request.ServiceType,
            ServiceDescription = request.ServiceDescription,
            RetentionFuelRate = request.RetentionFuelRate,
            RetentionIcaRate = request.RetentionIcaRate,
            IsActive = true,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.RetentionConfigurations.Add(config);
        await _context.SaveChangesAsync();

        return new RetentionConfigurationDto
        {
            Id = config.Id,
            ServiceType = config.ServiceType,
            ServiceDescription = config.ServiceDescription,
            RetentionFuelRate = config.RetentionFuelRate,
            RetentionIcaRate = config.RetentionIcaRate,
            IsActive = config.IsActive
        };
    }

    public async Task<RetentionConfigurationDto> UpdateRetentionConfigurationAsync(string tenantId, string userId, Guid configId, UpdateRetentionConfigurationRequestDto request)
    {
        var config = await _context.RetentionConfigurations
            .FirstOrDefaultAsync(r => r.Id == configId && r.TenantId == tenantId);

        if (config == null)
        {
            throw new KeyNotFoundException("Configuración de retención no encontrada.");
        }

        if (request.ServiceDescription != null) config.ServiceDescription = request.ServiceDescription;
        if (request.RetentionFuelRate.HasValue) config.RetentionFuelRate = request.RetentionFuelRate.Value;
        if (request.RetentionIcaRate.HasValue) config.RetentionIcaRate = request.RetentionIcaRate.Value;
        if (request.IsActive.HasValue) config.IsActive = request.IsActive.Value;

        config.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new RetentionConfigurationDto
        {
            Id = config.Id,
            ServiceType = config.ServiceType,
            ServiceDescription = config.ServiceDescription,
            RetentionFuelRate = config.RetentionFuelRate,
            RetentionIcaRate = config.RetentionIcaRate,
            IsActive = config.IsActive
        };
    }

    public async Task<List<ApprovalThresholdDto>> GetApprovalThresholdsAsync(string tenantId)
    {
        return await _context.ApprovalThresholds
            .Where(a => a.TenantId == tenantId)
            .OrderBy(a => a.MinValue)
            .Select(a => new ApprovalThresholdDto
            {
                Id = a.Id,
                ApprovalLevel = a.ApprovalLevel.ToString(),
                MinValue = a.MinValue,
                MaxValue = a.MaxValue,
                Description = a.Description,
                IsActive = a.IsActive
            })
            .ToListAsync();
    }

    public async Task<ApprovalThresholdDto> CreateApprovalThresholdAsync(string tenantId, string userId, CreateApprovalThresholdRequestDto request)
    {
        if (!Enum.TryParse<ApprovalLevel>(request.ApprovalLevel, true, out var approvalLevel))
        {
            throw new ArgumentException("Nivel de aprobación inválido. Use: Administrator, Council o Assembly.");
        }

        var existing = await _context.ApprovalThresholds
            .AnyAsync(a => a.TenantId == tenantId && a.ApprovalLevel == approvalLevel);

        if (existing)
        {
            throw new InvalidOperationException("Ya existe un umbral de aprobación para este nivel.");
        }

        if (request.MinValue > request.MaxValue)
        {
            throw new ArgumentException("El valor mínimo no puede ser mayor que el valor máximo.");
        }

        var threshold = new ApprovalThreshold
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApprovalLevel = approvalLevel,
            MinValue = request.MinValue,
            MaxValue = request.MaxValue,
            Description = request.Description,
            IsActive = true,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApprovalThresholds.Add(threshold);
        await _context.SaveChangesAsync();

        return new ApprovalThresholdDto
        {
            Id = threshold.Id,
            ApprovalLevel = threshold.ApprovalLevel.ToString(),
            MinValue = threshold.MinValue,
            MaxValue = threshold.MaxValue,
            Description = threshold.Description,
            IsActive = threshold.IsActive
        };
    }

    public async Task<ApprovalThresholdDto> UpdateApprovalThresholdAsync(string tenantId, string userId, Guid thresholdId, UpdateApprovalThresholdRequestDto request)
    {
        var threshold = await _context.ApprovalThresholds
            .FirstOrDefaultAsync(a => a.Id == thresholdId && a.TenantId == tenantId);

        if (threshold == null)
        {
            throw new KeyNotFoundException("Umbral de aprobación no encontrado.");
        }

        if (request.MinValue.HasValue) threshold.MinValue = request.MinValue.Value;
        if (request.MaxValue.HasValue) threshold.MaxValue = request.MaxValue.Value;
        if (request.Description != null) threshold.Description = request.Description;
        if (request.IsActive.HasValue) threshold.IsActive = request.IsActive.Value;

        if (threshold.MinValue > threshold.MaxValue)
        {
            throw new ArgumentException("El valor mínimo no puede ser mayor que el valor máximo.");
        }

        threshold.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ApprovalThresholdDto
        {
            Id = threshold.Id,
            ApprovalLevel = threshold.ApprovalLevel.ToString(),
            MinValue = threshold.MinValue,
            MaxValue = threshold.MaxValue,
            Description = threshold.Description,
            IsActive = threshold.IsActive
        };
    }
}
