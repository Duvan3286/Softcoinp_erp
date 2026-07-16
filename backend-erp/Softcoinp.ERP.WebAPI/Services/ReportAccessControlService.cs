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

public class ReportAccessControlService
{
    private readonly ApplicationDbContext _context;

    public ReportAccessControlService(ApplicationDbContext context)
    {
        _context = context;
    }

    private static readonly Dictionary<string, List<string>> RoleReportMap = new()
    {
        ["SuperAdmin"] = new List<string>
        {
            "PortfolioReport", "CollectionReport", "ExpenseReport",
            "BudgetExecution", "ActiveContracts", "PQRReport",
            "MaintenanceReport", "AssemblyReport", "AnnualManagementReport",
            "AccountantExport"
        },
        ["Admin"] = new List<string>
        {
            "PortfolioReport", "CollectionReport", "ExpenseReport",
            "BudgetExecution", "ActiveContracts", "PQRReport",
            "MaintenanceReport", "AssemblyReport", "AnnualManagementReport",
            "AccountantExport"
        }
    };

    public bool CanAccessReport(string role, string reportTypeCode)
    {
        if (string.IsNullOrEmpty(role))
            return false;

        if (!RoleReportMap.TryGetValue(role, out var allowedReports))
            return false;

        return allowedReports.Contains(reportTypeCode);
    }

    public List<string> GetAccessibleReports(string role)
    {
        if (string.IsNullOrEmpty(role))
            return new List<string>();

        if (!RoleReportMap.TryGetValue(role, out var allowedReports))
            return new List<string>();

        return allowedReports.ToList();
    }

    public bool CanAccessPersonalData(string role)
    {
        return role == "SuperAdmin" || role == "Admin";
    }

    public async Task<List<ReportTypeDto>> GetFilteredCatalogAsync(string tenantId, string role)
    {
        var accessibleCodes = GetAccessibleReports(role);
        var allTypes = await _context.ReportTypes
            .Where(r => r.TenantId == tenantId && r.IsActive)
            .OrderBy(r => r.Category)
            .ThenBy(r => r.Name)
            .ToListAsync();

        return allTypes
            .Where(r => accessibleCodes.Contains(r.ReportTypeCode.ToString()))
            .Select(r => new ReportTypeDto
            {
                Id = r.Id,
                ReportTypeCode = r.ReportTypeCode.ToString(),
                Name = r.Name,
                Description = r.Description,
                Category = r.Category.ToString(),
                SourceModules = r.SourceModules,
                ContainsPersonalData = r.ContainsPersonalData,
                IsActive = r.IsActive
            })
            .ToList();
    }

    public async Task<List<GeneratedReportDto>> GetFilteredHistoryAsync(string tenantId, string role)
    {
        var accessibleCodes = GetAccessibleReports(role);

        var query = await _context.GeneratedReports
            .Where(g => g.TenantId == tenantId)
            .Include(g => g.ReportType)
            .OrderByDescending(g => g.GeneratedAt)
            .ToListAsync();

        return query
            .Where(g => g.ReportType is not null && accessibleCodes.Contains(g.ReportType.ReportTypeCode.ToString()))
            .Select(g => new GeneratedReportDto
            {
                Id = g.Id,
                ReportTypeId = g.ReportTypeId,
                ReportTypeName = g.ReportType!.Name,
                ReportTypeCode = g.ReportType.ReportTypeCode.ToString(),
                Format = g.Format.ToString(),
                PeriodFrom = g.PeriodFrom,
                PeriodTo = g.PeriodTo,
                FileName = g.FileName,
                FileSizeBytes = g.FileSizeBytes,
                GeneratedByUserId = g.GeneratedByUserId,
                GeneratedAt = g.GeneratedAt,
                Parameters = g.Parameters,
                Notes = g.Notes,
                RecurringConfigId = g.RecurringConfigId,
                ConsecutiveNumber = g.ConsecutiveNumber
            })
            .ToList();
    }
}
