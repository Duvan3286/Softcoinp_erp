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
            .Include(c => c.ApprovedInAssembly)
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
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            HasAutoRenewal = contract.HasAutoRenewal,
            AutoRenewalNoticeDays = contract.AutoRenewalNoticeDays,
            ApprovedInAssemblyId = contract.ApprovedInAssemblyId,
            ApprovedInAssemblyTitle = contract.ApprovedInAssembly != null ? contract.ApprovedInAssembly.Title : string.Empty,
            Status = contract.Status.ToString(),
            SignedContractFilePath = contract.SignedContractFilePath,
            Observations = contract.Observations,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt,
            DaysUntilExpiration = daysUntilExpiration,
            Alerts = contract.Alerts.Select(a => new ContractAlertDto
            {
                Id = a.Id,
                AlertType = a.AlertType.ToString(),
                Message = a.Message,
                GeneratedAt = a.GeneratedAt,
                IsActive = a.IsActive,
                ResolvedAt = a.ResolvedAt,
                ResolvedByUserId = a.ResolvedByUserId
            }).ToList(),
            Invoices = contract.Invoices.Select(i => new ContractInvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                TotalAmount = i.TotalAmount,
                AmountPaid = i.AmountPaid,
                PendingAmount = i.TotalAmount - i.AmountPaid,
                Status = i.Status.ToString(),
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

        var provider = await _context.Providers
            .FirstOrDefaultAsync(p => p.Id == request.ProviderId && p.TenantId == tenantId);

        if (provider == null)
        {
            throw new KeyNotFoundException("El proveedor especificado no existe.");
        }

        var existingContract = await _context.Contracts
            .AnyAsync(c => c.TenantId == tenantId && c.ContractNumber == request.ContractNumber);

        if (existingContract)
        {
            throw new InvalidOperationException("Ya existe un contrato con ese número.");
        }

        var avgEvaluation = await _context.ProviderEvaluations
            .Where(e => e.ProviderId == request.ProviderId && e.TenantId == tenantId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(2)
            .Select(e => e.AverageScore)
            .ToListAsync();

        if (avgEvaluation.Count == 2 && avgEvaluation.Average() < 3.0m)
        {
            throw new InvalidOperationException(
                "No se puede crear un nuevo contrato. La evaluación promedio del proveedor en los últimos dos períodos es inferior a 3.0.");
        }

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
            Observations = request.Observations,
            Status = ContractStatus.Draft,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        return await GetContractByIdAsync(tenantId, contract.Id);
    }

    public async Task<ContractDetailDto> UpdateContractStatusAsync(string tenantId, string userId, Guid contractId, ChangeContractStatusRequestDto request)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.TenantId == tenantId);

        if (contract == null)
        {
            throw new KeyNotFoundException("Contrato no encontrado.");
        }

        if (!Enum.TryParse<ContractStatus>(request.NewStatus, true, out var newStatus))
        {
            throw new ArgumentException("Estado inválido. Use: Draft, Active, Expired o Terminated.");
        }

        if (newStatus == ContractStatus.Active && contract.Status != ContractStatus.Draft)
        {
            throw new InvalidOperationException("Solo se pueden activar contratos en estado Borrador.");
        }

        contract.Status = newStatus;
        contract.UpdatedByUserId = userId;
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
        if (request.StartDate.HasValue) contract.StartDate = request.StartDate.Value;
        if (request.EndDate.HasValue) contract.EndDate = request.EndDate.Value;
        if (request.HasAutoRenewal.HasValue) contract.HasAutoRenewal = request.HasAutoRenewal.Value;
        if (request.AutoRenewalNoticeDays.HasValue) contract.AutoRenewalNoticeDays = request.AutoRenewalNoticeDays.Value;
        if (request.SignedContractFilePath != null) contract.SignedContractFilePath = request.SignedContractFilePath;

        if (request.ApprovedInAssemblyId.HasValue)
        {
            var assemblyExists = await _context.Assemblies
                .AnyAsync(a => a.Id == request.ApprovedInAssemblyId.Value && a.TenantId == tenantId);

            if (!assemblyExists)
            {
                throw new KeyNotFoundException("La asamblea especificada no existe.");
            }

            contract.ApprovedInAssemblyId = request.ApprovedInAssemblyId.Value;
        }

        if (request.Observations != null) contract.Observations = request.Observations;

        contract.UpdatedByUserId = userId;
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

        var hasInvoices = await _context.ProviderInvoices
            .AnyAsync(i => i.ContractId == contractId);

        if (hasInvoices)
        {
            throw new InvalidOperationException("No se puede eliminar el contrato porque tiene facturas asociadas.");
        }

        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync();
    }

    public async Task<ProviderInvoice> CreateInvoiceAsync(string tenantId, string userId, CreateProviderInvoiceRequestDto request)
    {
        var provider = await _context.Providers
            .AnyAsync(p => p.Id == request.ProviderId && p.TenantId == tenantId);

        if (!provider)
        {
            throw new KeyNotFoundException("El proveedor especificado no existe.");
        }

        if (request.ContractId.HasValue)
        {
            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == request.ContractId && c.TenantId == tenantId);

            if (contract == null)
            {
                throw new KeyNotFoundException("El contrato especificado no existe.");
            }

            var invoicedSoFar = await _context.ProviderInvoices
                .Where(i => i.ContractId == request.ContractId.Value
                    && i.TenantId == tenantId
                    && i.Status != InvoiceStatus.Cancelled)
                .SumAsync(i => i.TotalAmount);

            var accumulatedTotal = invoicedSoFar + request.TotalAmount;

            if (accumulatedTotal > contract.TotalValue)
            {
                throw new InvalidOperationException(
                    $"El acumulado de facturas ({accumulatedTotal:C}) superaría el valor total del contrato ({contract.TotalValue:C}). " +
                    "Verifique el valor antes de continuar.");
            }
        }

        if (request.BudgetItemId.HasValue)
        {
            var budgetItem = await _context.ExpenseItems
                .Include(e => e.Budget)
                .FirstOrDefaultAsync(e => e.Id == request.BudgetItemId.Value && e.Budget!.TenantId == tenantId);

            if (budgetItem == null)
            {
                throw new KeyNotFoundException("El rubro presupuestal especificado no existe.");
            }

            var executedAmount = await _context.ExecutedExpenses
                .Where(e => e.ExpenseItemId == request.BudgetItemId.Value && e.TenantId == tenantId)
                .SumAsync(e => e.Amount);

            var available = budgetItem.AnnualValue - executedAmount;

            if (available <= 0)
            {
                throw new InvalidOperationException(
                    "El rubro presupuestal seleccionado está al 100% de ejecución. No es posible registrar este gasto.");
            }

            if (request.TotalAmount > available)
            {
                throw new InvalidOperationException(
                    $"El valor de la factura ({request.TotalAmount:C}) supera el saldo disponible del rubro presupuestal ({available:C}).");
            }
        }

        var parsedMethod = !string.IsNullOrEmpty(request.PaymentMethod)
            ? Enum.Parse<PaymentMethod>(request.PaymentMethod, true)
            : (PaymentMethod?)null;

        var invoiceStatus = request.AmountPaid <= 0
            ? InvoiceStatus.PendingPayment
            : request.AmountPaid >= request.TotalAmount
                ? InvoiceStatus.FullyPaid
                : InvoiceStatus.PartiallyPaid;

        var invoice = new ProviderInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProviderId = request.ProviderId,
            ContractId = request.ContractId,
            InvoiceNumber = request.InvoiceNumber,
            InvoiceDate = request.InvoiceDate,
            DueDate = request.DueDate,
            TotalAmount = request.TotalAmount,
            AmountPaid = request.AmountPaid,
            PaymentDate = request.PaymentDate,
            PaymentMethod = parsedMethod,
            PaymentReferenceNumber = request.PaymentReferenceNumber ?? string.Empty,
            BudgetItemId = request.BudgetItemId,
            Status = invoiceStatus,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProviderInvoices.Add(invoice);

        if (request.AmountPaid > 0)
        {
            var payment = new ProviderPayment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                InvoiceId = invoice.Id,
                Amount = request.AmountPaid,
                PaymentDate = request.PaymentDate ?? DateTime.UtcNow,
                PaymentMethod = parsedMethod ?? PaymentMethod.Transfer,
                ReferenceNumber = request.PaymentReferenceNumber ?? string.Empty,
                Status = PaymentStatus.Completed,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProviderPayments.Add(payment);
        }

        // El ERP es un cuaderno digital para el administrador, no un sistema contable de
        // causación: el rubro presupuestal solo se ejecuta por el valor efectivamente
        // pagado, nunca por el valor facturado. Si la factura ya nace con un pago parcial
        // o total (AmountPaid > 0), ese pago sí ejecuta el rubro de una vez; el saldo por
        // pagar se ejecutará cuando se registre en RegisterPaymentAsync.
        if (request.BudgetItemId.HasValue && request.AmountPaid > 0)
        {
            var executedExpense = new ExecutedExpense
            {
                TenantId = tenantId,
                ExpenseItemId = request.BudgetItemId.Value,
                Description = $"Factura {request.InvoiceNumber} - Proveedor: {request.ProviderId}",
                Amount = request.AmountPaid,
                ExpenseDate = request.PaymentDate ?? request.InvoiceDate,
                ProviderId = request.ProviderId,
                InvoiceReference = request.InvoiceNumber,
                CreatedByUserId = userId
            };

            _context.ExecutedExpenses.Add(executedExpense);
        }

        await _context.SaveChangesAsync();

        return invoice;
    }

    public async Task<ProviderPayment> RegisterPaymentAsync(string tenantId, string userId, Guid invoiceId, CreateProviderPaymentRequestDto request)
    {
        var invoice = await _context.ProviderInvoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.TenantId == tenantId);

        if (invoice == null)
        {
            throw new KeyNotFoundException("Factura no encontrada.");
        }

        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, true, out var paymentMethod))
        {
            throw new ArgumentException("Método de pago inválido. Use: Cash, Transfer o Check.");
        }

        var newAmountPaid = invoice.AmountPaid + request.Amount;
        if (newAmountPaid > invoice.TotalAmount)
        {
            throw new InvalidOperationException(
                $"El pago de {request.Amount:C} excede el saldo pendiente de {(invoice.TotalAmount - invoice.AmountPaid):C}.");
        }

        var payment = new ProviderPayment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InvoiceId = invoiceId,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate,
            PaymentMethod = paymentMethod,
            ReferenceNumber = request.ReferenceNumber,
            Status = PaymentStatus.Completed,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        invoice.AmountPaid = newAmountPaid;
        invoice.PaymentDate = request.PaymentDate;
        invoice.PaymentMethod = paymentMethod;
        invoice.PaymentReferenceNumber = request.ReferenceNumber;
        invoice.Status = newAmountPaid >= invoice.TotalAmount ? InvoiceStatus.FullyPaid : InvoiceStatus.PartiallyPaid;
        invoice.UpdatedAt = DateTime.UtcNow;

        _context.ProviderPayments.Add(payment);

        // Ejecución presupuestal en base a caja: el rubro se ejecuta en el momento en que
        // el pago realmente se realiza, no cuando se causó la factura.
        if (invoice.BudgetItemId.HasValue)
        {
            var executedExpense = new ExecutedExpense
            {
                TenantId = tenantId,
                ExpenseItemId = invoice.BudgetItemId.Value,
                Description = $"Pago factura {invoice.InvoiceNumber} - Proveedor: {invoice.ProviderId}",
                Amount = request.Amount,
                ExpenseDate = request.PaymentDate,
                ProviderId = invoice.ProviderId,
                InvoiceReference = invoice.InvoiceNumber,
                CreatedByUserId = userId
            };

            _context.ExecutedExpenses.Add(executedExpense);
        }

        await _context.SaveChangesAsync();

        return payment;
    }

    public async Task CancelInvoiceAsync(string tenantId, string userId, Guid invoiceId)
    {
        var invoice = await _context.ProviderInvoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.TenantId == tenantId);

        if (invoice == null)
        {
            throw new KeyNotFoundException("Factura no encontrada.");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            throw new InvalidOperationException("La factura ya se encuentra anulada.");
        }

        if (invoice.BudgetItemId.HasValue)
        {
            var linkedExpenses = await _context.ExecutedExpenses
                .Where(e => e.TenantId == tenantId
                    && e.ExpenseItemId == invoice.BudgetItemId.Value
                    && e.InvoiceReference == invoice.InvoiceNumber)
                .ToListAsync();

            _context.ExecutedExpenses.RemoveRange(linkedExpenses);
        }

        foreach (var payment in invoice.Payments)
        {
            payment.Status = PaymentStatus.Cancelled;
        }

        invoice.Status = InvoiceStatus.Cancelled;
        invoice.UpdatedByUserId = userId;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<List<PendingPaymentDto>> GetPendingPaymentsAsync(string tenantId)
    {
        var now = DateTime.UtcNow;

        var pending = await _context.ProviderInvoices
            .Include(i => i.Provider)
            .Include(i => i.Contract)
            .Where(i => i.TenantId == tenantId && i.Status != InvoiceStatus.FullyPaid)
            .OrderBy(i => i.DueDate)
            .Select(i => new PendingPaymentDto
            {
                InvoiceId = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                ProviderName = i.Provider != null ? i.Provider.BusinessName : string.Empty,
                ProviderDocumentNumber = i.Provider != null ? i.Provider.DocumentNumber : string.Empty,
                ContractId = i.ContractId,
                ContractNumber = i.Contract != null ? i.Contract.ContractNumber : string.Empty,
                TotalAmount = i.TotalAmount,
                AmountPaid = i.AmountPaid,
                PendingAmount = i.TotalAmount - i.AmountPaid,
                DueDate = i.DueDate,
                DaysOverdue = i.DueDate < now ? (int)(now - i.DueDate).TotalDays : 0,
                Status = i.Status.ToString()
            })
            .ToListAsync();

        return pending;
    }

    public async Task<List<ContractExpirationReportDto>> GetExpiringContractsReportAsync(string tenantId, int daysAhead = 90)
    {
        var now = DateTime.UtcNow;
        var limit = now.AddDays(daysAhead);

        var contracts = await _context.Contracts
            .Include(c => c.Provider)
            .Where(c => c.TenantId == tenantId &&
                c.Status == ContractStatus.Active &&
                c.EndDate >= now &&
                c.EndDate <= limit)
            .OrderBy(c => c.EndDate)
            .Select(c => new ContractExpirationReportDto
            {
                ContractId = c.Id,
                ContractNumber = c.ContractNumber,
                ContractType = c.ContractType.ToString(),
                ProviderName = c.Provider != null ? c.Provider.BusinessName : string.Empty,
                ProviderDocumentNumber = c.Provider != null ? c.Provider.DocumentNumber : string.Empty,
                TotalValue = c.TotalValue,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                DaysUntilExpiration = (int)(c.EndDate - now).TotalDays,
                HasAutoRenewal = c.HasAutoRenewal,
                AutoRenewalNoticeDays = c.AutoRenewalNoticeDays,
                Status = c.Status.ToString()
            })
            .ToListAsync();

        return contracts;
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
                ResolvedByUserId = a.ResolvedByUserId
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

    public async Task<List<ApprovalThresholdDto>> GetApprovalThresholdsAsync(string tenantId)
    {
        return await _context.ApprovalThresholds
            .Where(a => a.TenantId == tenantId)
            .OrderBy(a => a.MinValue)
            .Select(a => new ApprovalThresholdDto
            {
                Id = a.Id,
                MinValue = a.MinValue,
                MaxValue = a.MaxValue,
                Description = a.Description,
                IsActive = a.IsActive
            })
            .ToListAsync();
    }

    public async Task<ApprovalThresholdDto> CreateApprovalThresholdAsync(string tenantId, string userId, CreateApprovalThresholdRequestDto request)
    {
        if (request.MinValue > request.MaxValue)
        {
            throw new ArgumentException("El valor mínimo no puede ser mayor que el valor máximo.");
        }

        var threshold = new ApprovalThreshold
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
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
            MinValue = threshold.MinValue,
            MaxValue = threshold.MaxValue,
            Description = threshold.Description,
            IsActive = threshold.IsActive
        };
    }
}
