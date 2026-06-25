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

public class ContractService
{
    private readonly ApplicationDbContext _context;

    public ContractService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContractListDto>> GetContractsAsync(
        string tenantId,
        string? status = null,
        string? contractType = null,
        Guid? providerId = null,
        string? search = null)
    {
        var query = _context.Contracts
            .Include(c => c.Provider)
            .Where(c => c.TenantId == tenantId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ContractStatus>(status, true, out var statusEnum))
        {
            query = query.Where(c => c.Status == statusEnum);
        }

        if (!string.IsNullOrEmpty(contractType) && Enum.TryParse<ContractType>(contractType, true, out var typeEnum))
        {
            query = query.Where(c => c.ContractType == typeEnum);
        }

        if (providerId.HasValue)
        {
            query = query.Where(c => c.ProviderId == providerId.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(c =>
                c.ContractNumber.ToLower().Contains(searchLower) ||
                c.ObjectDescription.ToLower().Contains(searchLower) ||
                (c.Provider != null && c.Provider.BusinessName.ToLower().Contains(searchLower)));
        }

        var contractsRaw = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.ContractNumber,
                ContractType = c.ContractType.ToString(),
                ProviderBusinessName = c.Provider != null ? c.Provider.BusinessName : string.Empty,
                c.TotalValue,
                c.MonthlyValue,
                c.StartDate,
                c.EndDate,
                c.HasAutoRenewal,
                ApprovalLevel = c.ApprovalLevel.ToString(),
                Status = c.Status.ToString(),
                AlertCount = c.Alerts.Count(a => a.IsActive)
            })
            .ToListAsync();

        var contracts = contractsRaw.Select(c => new ContractListDto
        {
            Id = c.Id,
            ContractNumber = c.ContractNumber,
            ContractType = c.ContractType,
            ProviderBusinessName = c.ProviderBusinessName,
            TotalValue = c.TotalValue,
            MonthlyValue = c.MonthlyValue,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            HasAutoRenewal = c.HasAutoRenewal,
            ApprovalLevel = c.ApprovalLevel,
            Status = c.Status,
            DaysUntilExpiration = (int)(c.EndDate - DateTime.UtcNow).TotalDays,
            AlertCount = c.AlertCount
        }).ToList();

        return contracts;
    }

    public async Task<ContractDetailDto> GetContractByIdAsync(string tenantId, Guid contractId)
    {
        var contract = await _context.Contracts
            .Include(c => c.Provider)
            .Include(c => c.Policies)
            .Include(c => c.Alerts.Where(a => a.IsActive))
            .Include(c => c.Invoices)
                .ThenInclude(i => i.Payments)
            .Where(c => c.Id == contractId && c.TenantId == tenantId)
            .FirstOrDefaultAsync();

        if (contract == null)
        {
            throw new KeyNotFoundException("Contrato no encontrado.");
        }

        var daysUntilExpiration = (int)(contract.EndDate - DateTime.UtcNow).TotalDays;

        return new ContractDetailDto
        {
            Id = contract.Id,
            ProviderId = contract.ProviderId,
            ProviderBusinessName = contract.Provider?.BusinessName ?? string.Empty,
            ProviderDocumentNumber = contract.Provider?.DocumentNumber ?? string.Empty,
            ContractNumber = contract.ContractNumber,
            ContractType = contract.ContractType.ToString(),
            ObjectDescription = contract.ObjectDescription,
            TotalValue = contract.TotalValue,
            MonthlyValue = contract.MonthlyValue,
            IsRecurrent = contract.IsRecurrent,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            HasAutoRenewal = contract.HasAutoRenewal,
            AutoRenewalNoticeDays = contract.AutoRenewalNoticeDays,
            ApprovalLevel = contract.ApprovalLevel.ToString(),
            CouncilMeetingActNumber = contract.CouncilMeetingActNumber,
            AssemblyMeetingActNumber = contract.AssemblyMeetingActNumber,
            BudgetAccountId = contract.BudgetAccountId?.ToString() ?? string.Empty,
            Status = contract.Status.ToString(),
            SignedContractFilePath = contract.SignedContractFilePath,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt,
            DaysUntilExpiration = daysUntilExpiration,
            Policies = contract.Policies.Select(p => new ContractPolicyDto
            {
                Id = p.Id,
                PolicyNumber = p.PolicyNumber,
                InsuranceCompany = p.InsuranceCompany,
                PolicyType = p.PolicyType,
                InsuredAmount = p.InsuredAmount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                FilePath = p.FilePath,
                IsActive = p.IsActive,
                DaysUntilExpiration = (int)(p.EndDate - DateTime.UtcNow).TotalDays
            }).ToList(),
            Alerts = contract.Alerts.Select(a => new ContractAlertDto
            {
                Id = a.Id,
                AlertType = a.AlertType.ToString(),
                Message = a.Message,
                GeneratedAt = a.GeneratedAt,
                IsActive = a.IsActive,
                ResolvedAt = a.ResolvedAt,
                ResolvedByUserId = a.ResolvedByUserId,
                EscalatedToCouncil = a.EscalatedToCouncil
            }).ToList(),
            Invoices = contract.Invoices.Select(i => new ContractInvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                Subtotal = i.Subtotal,
                IvaAmount = i.IvaAmount,
                RetentionFuelAmount = i.RetentionFuelAmount,
                RetentionIcaAmount = i.RetentionIcaAmount,
                NetAmount = i.NetAmount,
                Status = i.Status.ToString(),
                PendingAmount = i.NetAmount - i.Payments
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .Sum(p => p.Amount),
                Payments = i.Payments.Select(p => new ProviderPaymentDto
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    ReferenceNumber = p.ReferenceNumber,
                    Status = p.Status.ToString()
                }).ToList()
            }).ToList()
        };
    }

    public async Task<ContractDetailDto> CreateContractAsync(string tenantId, string userId, CreateContractRequestDto request)
    {
        if (!Enum.TryParse<ContractType>(request.ContractType, true, out var contractType))
        {
            throw new ArgumentException("Tipo de contrato inválido. Use: ServiceAgreement, Supply, CivilWorks o Lease.");
        }

        var providerExists = await _context.Providers
            .AnyAsync(p => p.Id == request.ProviderId && p.TenantId == tenantId);

        if (!providerExists)
        {
            throw new KeyNotFoundException("El proveedor especificado no existe.");
        }

        var existingContract = await _context.Contracts
            .AnyAsync(c => c.TenantId == tenantId && c.ContractNumber == request.ContractNumber);

        if (existingContract)
        {
            throw new InvalidOperationException("Ya existe un contrato con ese número.");
        }

        var approvalLevel = DetermineApprovalLevel(request.TotalValue, tenantId);

        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProviderId = request.ProviderId,
            ContractNumber = request.ContractNumber,
            ContractType = contractType,
            ObjectDescription = request.ObjectDescription,
            TotalValue = request.TotalValue,
            MonthlyValue = request.MonthlyValue,
            IsRecurrent = request.IsRecurrent,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            HasAutoRenewal = request.HasAutoRenewal,
            AutoRenewalNoticeDays = request.AutoRenewalNoticeDays,
            ApprovalLevel = approvalLevel,
            BudgetAccountId = request.BudgetAccountId,
            Status = ContractStatus.Draft,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        return await GetContractByIdAsync(tenantId, contract.Id);
    }

    public async Task<ContractDetailDto> UpdateContractAsync(string tenantId, string userId, Guid contractId, ChangeContractStatusRequestDto request)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.TenantId == tenantId);

        if (contract == null)
        {
            throw new KeyNotFoundException("Contrato no encontrado.");
        }

        if (!Enum.TryParse<ContractStatus>(request.NewStatus, true, out var newStatus))
        {
            throw new ArgumentException("Estado inválido. Use: Draft, Active, Suspended, Completed, Terminated o Cancelled.");
        }

        if (newStatus == ContractStatus.Active && contract.Status != ContractStatus.Draft)
        {
            throw new InvalidOperationException("Solo se pueden activar contratos en estado Borrador.");
        }

        if (newStatus == ContractStatus.Active)
        {
            var hasApprovedAct = !string.IsNullOrEmpty(contract.CouncilMeetingActNumber) ||
                                 !string.IsNullOrEmpty(contract.AssemblyMeetingActNumber);

            if (contract.ApprovalLevel == ApprovalLevel.Council && !hasApprovedAct)
            {
                throw new InvalidOperationException("Se requiere número de acta de consejo para activar este contrato.");
            }

            if (contract.ApprovalLevel == ApprovalLevel.Assembly && !hasApprovedAct)
            {
                throw new InvalidOperationException("Se requiere número de acta de asamblea para activar este contrato.");
            }
        }

        contract.Status = newStatus;
        contract.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetContractByIdAsync(tenantId, contract.Id);
    }

    public async Task<ContractDetailDto> UpdateContractDetailsAsync(string tenantId, string userId, Guid contractId, UpdateContractRequestDto request)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.TenantId == tenantId);

        if (contract == null)
        {
            throw new KeyNotFoundException("Contrato no encontrado.");
        }

        if (contract.Status != ContractStatus.Draft)
        {
            throw new InvalidOperationException("Solo se pueden editar contratos en estado Borrador.");
        }

        if (request.ContractType != null)
        {
            if (!Enum.TryParse<ContractType>(request.ContractType, true, out var contractType))
            {
                throw new ArgumentException("Tipo de contrato inválido.");
            }
            contract.ContractType = contractType;
        }

        if (request.ObjectDescription != null) contract.ObjectDescription = request.ObjectDescription;
        if (request.TotalValue.HasValue) contract.TotalValue = request.TotalValue.Value;
        if (request.MonthlyValue.HasValue) contract.MonthlyValue = request.MonthlyValue.Value;
        if (request.IsRecurrent.HasValue) contract.IsRecurrent = request.IsRecurrent.Value;
        if (request.StartDate.HasValue) contract.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) contract.EndDate = request.EndDate.Value;
        if (request.HasAutoRenewal.HasValue) contract.HasAutoRenewal = request.HasAutoRenewal.Value;
        if (request.AutoRenewalNoticeDays.HasValue) contract.AutoRenewalNoticeDays = request.AutoRenewalNoticeDays.Value;
        if (request.BudgetAccountId.HasValue) contract.BudgetAccountId = request.BudgetAccountId.Value;
        if (request.SignedContractFilePath != null) contract.SignedContractFilePath = request.SignedContractFilePath;

        contract.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetContractByIdAsync(tenantId, contract.Id);
    }

    public async Task DeleteContractAsync(string tenantId, Guid contractId)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.TenantId == tenantId);

        if (contract == null)
        {
            throw new KeyNotFoundException("Contrato no encontrado.");
        }

        if (contract.Status != ContractStatus.Draft)
        {
            throw new InvalidOperationException("Solo se pueden eliminar contratos en estado Borrador.");
        }

        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync();
    }

    public async Task<ContractPolicyDto> AddContractPolicyAsync(string tenantId, string userId, Guid contractId, CreateContractPolicyRequestDto request)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.TenantId == tenantId);

        if (contract == null)
        {
            throw new KeyNotFoundException("Contrato no encontrado.");
        }

        var policy = new ContractPolicy
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ContractId = contractId,
            PolicyNumber = request.PolicyNumber,
            InsuranceCompany = request.InsuranceCompany,
            PolicyType = request.PolicyType,
            InsuredAmount = request.InsuredAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            FilePath = request.FilePath,
            IsActive = true,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ContractPolicies.Add(policy);
        await _context.SaveChangesAsync();

        return new ContractPolicyDto
        {
            Id = policy.Id,
            PolicyNumber = policy.PolicyNumber,
            InsuranceCompany = policy.InsuranceCompany,
            PolicyType = policy.PolicyType,
            InsuredAmount = policy.InsuredAmount,
            StartDate = policy.StartDate,
            EndDate = policy.EndDate,
            FilePath = policy.FilePath,
            IsActive = policy.IsActive,
            DaysUntilExpiration = (int)(policy.EndDate - DateTime.UtcNow).TotalDays
        };
    }

    public async Task<List<ContractAlertDto>> GetActiveContractAlertsAsync(string tenantId)
    {
        return await _context.ContractAlerts
            .Where(a => a.TenantId == tenantId && a.IsActive)
            .OrderByDescending(a => a.GeneratedAt)
            .Select(a => new ContractAlertDto
            {
                Id = a.Id,
                AlertType = a.AlertType.ToString(),
                Message = a.Message,
                GeneratedAt = a.GeneratedAt,
                IsActive = a.IsActive,
                ResolvedAt = a.ResolvedAt,
                ResolvedByUserId = a.ResolvedByUserId,
                EscalatedToCouncil = a.EscalatedToCouncil
            })
            .ToListAsync();
    }

    public async Task ResolveContractAlertAsync(string tenantId, string userId, Guid alertId)
    {
        var alert = await _context.ContractAlerts
            .FirstOrDefaultAsync(a => a.Id == alertId && a.TenantId == tenantId);

        if (alert == null)
        {
            throw new KeyNotFoundException("Alerta no encontrada.");
        }

        alert.IsActive = false;
        alert.ResolvedAt = DateTime.UtcNow;
        alert.ResolvedByUserId = userId;

        await _context.SaveChangesAsync();
    }

    private ApprovalLevel DetermineApprovalLevel(decimal totalValue, string tenantId)
    {
        var thresholds = _context.ApprovalThresholds
            .Where(a => a.TenantId == tenantId && a.IsActive)
            .OrderBy(a => a.MinValue)
            .ToList();

        foreach (var threshold in thresholds)
        {
            if (totalValue >= threshold.MinValue && totalValue <= threshold.MaxValue)
            {
                return threshold.ApprovalLevel;
            }
        }

        return ApprovalLevel.Administrator;
    }
}
