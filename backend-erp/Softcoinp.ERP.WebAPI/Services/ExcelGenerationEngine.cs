using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class ExcelGenerationEngine
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ExcelGenerationEngine(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    public async Task<GeneratedReport> GenerateExcelReportAsync(
        string tenantId, string reportTypeCode, string userId,
        DateTime? periodFrom, DateTime? periodTo, string? parameters, string? notes,
        Guid? recurringConfigId = null)
    {
        if (!Enum.TryParse<ReportTypeEnum>(reportTypeCode, out var reportTypeEnum))
            throw new InvalidOperationException("Invalid report type code: " + reportTypeCode);

        var reportType = await _context.ReportTypes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.ReportTypeCode == reportTypeEnum);

        if (reportType is null)
            throw new InvalidOperationException("Report type not found: " + reportTypeCode);

        var periodLabel = BuildPeriodLabel(periodFrom, periodTo);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var dir = Path.Combine(_env.WebRootPath ?? "wwwroot", "reports", tenantId, reportTypeCode);
        Directory.CreateDirectory(dir);

        var fileName = $"{reportTypeCode}_{periodLabel}_{timestamp}.xlsx";
        var filePath = Path.Combine(dir, fileName);

        using var workbook = new XLWorkbook();

        workbook.Style.Font.FontSize = 10;
        workbook.Style.Font.FontName = "Calibri";

        var ws = workbook.Worksheets.Add(reportType.Name.Length > 31 ? reportType.Name.Substring(0, 31) : reportType.Name);
        ConfigureSheet(ws, reportTypeCode, tenantId, periodFrom, periodTo);

        workbook.SaveAs(filePath);

        var fileInfo = new FileInfo(filePath);
        var generated = new GeneratedReport
        {
            TenantId = tenantId,
            ReportTypeId = reportType.Id,
            Format = ReportFormat.Excel,
            PeriodFrom = periodFrom,
            PeriodTo = periodTo,
            FileName = fileName,
            FilePath = filePath,
            FileSizeBytes = fileInfo.Length,
            GeneratedByUserId = userId,
            GeneratedAt = DateTime.UtcNow,
            Parameters = parameters,
            Notes = notes,
            RecurringConfigId = recurringConfigId
        };

        _context.GeneratedReports.Add(generated);
        await _context.SaveChangesAsync();

        return generated;
    }

    private void ConfigureSheet(IXLWorksheet ws, string reportTypeCode, string tenantId,
        DateTime? periodFrom, DateTime? periodTo)
    {
        switch (reportTypeCode)
        {
            case "TrialBalance":
                FillTrialBalance(ws, tenantId, periodFrom, periodTo);
                break;
            case "PortfolioAging":
                FillPortfolioAging(ws, tenantId);
                break;
            case "PortfolioByUnit":
                FillPortfolioByUnit(ws, tenantId);
                break;
            case "TopDebtors":
                FillTopDebtors(ws, tenantId);
                break;
            case "PeriodCollection":
                FillPeriodCollection(ws, tenantId, periodFrom, periodTo);
                break;
            case "PaymentAgreements":
                FillPaymentAgreements(ws, tenantId);
                break;
            case "ActiveContracts":
                FillActiveContracts(ws, tenantId);
                break;
            case "OwnerRegistry":
                FillOwnerRegistry(ws, tenantId);
                break;
            case "BalanceGeneral":
                FillBalanceGeneral(ws, tenantId, periodFrom, periodTo);
                break;
            case "IncomeExpense":
                FillIncomeExpense(ws, tenantId, periodFrom, periodTo);
                break;
            case "BudgetExecution":
                FillBudgetExecution(ws, tenantId, periodFrom, periodTo);
                break;
            case "ContingencyFund":
                FillContingencyFund(ws, tenantId);
                break;
            case "CashFlow":
                FillCashFlow(ws, tenantId, periodFrom, periodTo);
                break;
            case "GeneralLedger":
                FillGeneralLedger(ws, tenantId, periodFrom, periodTo);
                break;
            case "SubsidiaryLedger":
                FillGeneralLedger(ws, tenantId, periodFrom, periodTo);
                break;
            case "JournalEntryDetail":
                FillJournalEntryDetail(ws, tenantId, periodFrom, periodTo);
                break;
            case "PortfolioProjection":
                ws.Cell(1, 1).Value = "Proyeccion de Cartera";
                ws.Cell(2, 1).Value = "Funcionalidad en desarrollo.";
                break;
            case "PQRSummary":
                FillPQRSummary(ws, tenantId);
                break;
            case "CommonAreaUsage":
                FillCommonAreaUsage(ws, tenantId);
                break;
            case "MaintenanceSummary":
                FillMaintenanceSummary(ws, tenantId);
                break;
            case "CommunicationSummary":
                FillCommunicationSummary(ws, tenantId);
                break;
            case "AssemblyMinutes":
                FillAssemblyMinutes(ws, tenantId);
                break;
            case "AssemblyDecisions":
                FillAssemblyDecisions(ws, tenantId);
                break;
            case "CouncilHistory":
                FillCouncilHistory(ws, tenantId);
                break;
            case "AssemblyQuorum":
                FillAssemblyQuorum(ws, tenantId);
                break;
            default:
                ws.Cell(1, 1).Value = "Exportacion de datos";
                ws.Cell(2, 1).Value = "Tipo de reporte: " + reportTypeCode;
                ws.Cell(2, 1).Style.Font.Italic = true;
                ws.Columns().AdjustToContents();
                break;
        }
    }

    private void FillTrialBalance(IXLWorksheet ws, string tenantId, DateTime? from, DateTime? to)
    {
        var headers = new[] { "Codigo", "Cuenta", "Debito", "Credito", "Saldo" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var linesQuery = _context.EntryLines
            .Where(l => l.AccountingEntry!.TenantId == tenantId && l.AccountingEntry.Status == EntryStatus.Final)
            .AsQueryable();

        if (from.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.EntryDate >= from.Value);
        if (to.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.EntryDate <= to.Value);

        var grouped = linesQuery
            .GroupBy(l => l.AccountingAccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                TotalDebit = g.Sum(l => l.Debit),
                TotalCredit = g.Sum(l => l.Credit)
            })
            .ToList();

        var accountIds = grouped.Select(g => g.AccountId).ToList();
        var accounts = _context.AccountingAccounts
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionary(a => a.Id);

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        decimal totalDebit = 0, totalCredit = 0;

        foreach (var g in grouped)
        {
            if (!accounts.TryGetValue(g.AccountId, out var account)) continue;

            var balance = account.Nature == AccountingAccountNature.Debit
                ? g.TotalDebit - g.TotalCredit
                : g.TotalCredit - g.TotalDebit;

            ws.Cell(row, 1).Value = account.Code;
            ws.Cell(row, 2).Value = account.Name;
            ws.Cell(row, 3).Value = g.TotalDebit;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 4).Value = g.TotalCredit;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = balance;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 5; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            totalDebit += g.TotalDebit;
            totalCredit += g.TotalCredit;
            row++;
        }

        ws.Cell(row, 1).Value = "TOTALES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 3).Value = totalDebit;
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 4).Value = totalCredit;
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 5).Value = Math.Abs(totalDebit - totalCredit);
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, 5).SetAutoFilter();
        ws.Columns(1, 2).Width = 25;
        ws.Columns(3, 5).Width = 15;
    }

    private void FillBalanceGeneral(IXLWorksheet ws, string tenantId, DateTime? from, DateTime? to)
    {
        var headers = new[] { "Categoria", "Codigo", "Cuenta", "Saldo Debito", "Saldo Credito", "Saldo Neto" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var linesQuery = _context.EntryLines
            .Where(l => l.AccountingEntry!.TenantId == tenantId && l.AccountingEntry.Status == EntryStatus.Final)
            .AsQueryable();

        if (from.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.EntryDate >= from.Value);
        if (to.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.EntryDate <= to.Value);

        var allAccounts = _context.AccountingAccounts.Where(a => a.TenantId == tenantId).ToList();
        var accountDict = allAccounts.ToDictionary(a => a.Id);
        var balanceCategories = new[] { AccountingAccountCategory.Asset, AccountingAccountCategory.Liability, AccountingAccountCategory.Equity };
        var allowedIds = allAccounts.Where(a => balanceCategories.Contains(a.Category)).Select(a => a.Id).ToHashSet();

        var grouped = linesQuery
            .Where(l => allowedIds.Contains(l.AccountingAccountId))
            .GroupBy(l => l.AccountingAccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                TotalDebit = g.Sum(l => l.Debit),
                TotalCredit = g.Sum(l => l.Credit)
            })
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var currentCategory = string.Empty;
        var categoryStartRow = 0;
        var categoryDebitSum = 0m;
        var categoryCreditSum = 0m;
        var grandDebit = 0m;
        var grandCredit = 0m;

        foreach (var g in grouped.OrderBy(g => accountDict.GetValueOrDefault(g.AccountId)?.Category).ThenBy(g => accountDict.GetValueOrDefault(g.AccountId)?.Code))
        {
            if (!accountDict.TryGetValue(g.AccountId, out var account)) continue;

            if (account.Category.ToString() != currentCategory)
            {
                if (currentCategory != string.Empty)
                {
                    var label = currentCategory switch
                    {
                        "Asset" => "TOTAL ACTIVO",
                        "Liability" => "TOTAL PASIVO",
                        "Equity" => "TOTAL PATRIMONIO",
                        _ => "TOTAL " + currentCategory
                    };
                    ws.Cell(row, 1).Value = label;
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    ws.Cell(row, 4).Value = categoryDebitSum;
                    ws.Cell(row, 4).Style.Font.Bold = true;
                    ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, 5).Value = categoryCreditSum;
                    ws.Cell(row, 5).Style.Font.Bold = true;
                    ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(row, 6).Value = Math.Abs(categoryDebitSum - categoryCreditSum);
                    ws.Cell(row, 6).Style.Font.Bold = true;
                    ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
                    row++;
                    categoryDebitSum = 0;
                    categoryCreditSum = 0;
                }

                currentCategory = account.Category.ToString();
                categoryStartRow = row;
            }

            var netBalance = account.Nature == AccountingAccountNature.Debit
                ? g.TotalDebit - g.TotalCredit
                : g.TotalCredit - g.TotalDebit;

            var displayDebit = netBalance >= 0 ? netBalance : 0;
            var displayCredit = netBalance < 0 ? Math.Abs(netBalance) : 0;

            ws.Cell(row, 1).Value = account.Category.ToString();
            ws.Cell(row, 2).Value = account.Code;
            ws.Cell(row, 3).Value = account.Name;
            ws.Cell(row, 4).Value = g.TotalDebit;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = g.TotalCredit;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = netBalance;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 6; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            categoryDebitSum += g.TotalDebit;
            categoryCreditSum += g.TotalCredit;
            grandDebit += g.TotalDebit;
            grandCredit += g.TotalCredit;
            row++;
        }

        if (currentCategory != string.Empty)
        {
            var label = currentCategory switch
            {
                "Asset" => "TOTAL ACTIVO",
                "Liability" => "TOTAL PASIVO",
                "Equity" => "TOTAL PATRIMONIO",
                _ => "TOTAL " + currentCategory
            };
            ws.Cell(row, 1).Value = label;
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 4).Value = categoryDebitSum;
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = categoryCreditSum;
            ws.Cell(row, 5).Style.Font.Bold = true;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = Math.Abs(categoryDebitSum - categoryCreditSum);
            ws.Cell(row, 6).Style.Font.Bold = true;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        ws.Cell(row, 1).Value = "TOTAL BALANCE GENERAL";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 4).Value = grandDebit;
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 5).Value = grandCredit;
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 6).Value = Math.Abs(grandDebit - grandCredit);
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, 6).SetAutoFilter();
        ws.Columns(1, 1).Width = 18;
        ws.Columns(2, 2).Width = 15;
        ws.Columns(3, 3).Width = 35;
        ws.Columns(4, 6).Width = 15;
    }

    private void FillIncomeExpense(IXLWorksheet ws, string tenantId, DateTime? from, DateTime? to)
    {
        var headers = new[] { "Tipo", "Codigo", "Cuenta", "Debito", "Credito", "Saldo" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var linesQuery = _context.EntryLines
            .Where(l => l.AccountingEntry!.TenantId == tenantId && l.AccountingEntry.Status == EntryStatus.Final)
            .AsQueryable();

        if (from.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.EntryDate >= from.Value);
        if (to.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.EntryDate <= to.Value);

        var allAccounts = _context.AccountingAccounts.Where(a => a.TenantId == tenantId).ToList();
        var accountDict = allAccounts.ToDictionary(a => a.Id);
        var ieCategories = new[] { AccountingAccountCategory.Income, AccountingAccountCategory.Expense };
        var allowedIds = allAccounts.Where(a => ieCategories.Contains(a.Category)).Select(a => a.Id).ToHashSet();

        var grouped = linesQuery
            .Where(l => allowedIds.Contains(l.AccountingAccountId))
            .GroupBy(l => l.AccountingAccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                TotalDebit = g.Sum(l => l.Debit),
                TotalCredit = g.Sum(l => l.Credit)
            })
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var totalIncome = 0m;
        var totalExpense = 0m;

        foreach (var g in grouped.OrderBy(g => accountDict.GetValueOrDefault(g.AccountId)?.Category).ThenBy(g => accountDict.GetValueOrDefault(g.AccountId)?.Code))
        {
            if (!accountDict.TryGetValue(g.AccountId, out var account)) continue;

            var balance = account.Nature == AccountingAccountNature.Debit
                ? g.TotalDebit - g.TotalCredit
                : g.TotalCredit - g.TotalDebit;

            ws.Cell(row, 1).Value = account.Category == AccountingAccountCategory.Income ? "Ingreso" : "Gasto";
            ws.Cell(row, 2).Value = account.Code;
            ws.Cell(row, 3).Value = account.Name;
            ws.Cell(row, 4).Value = g.TotalDebit;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = g.TotalCredit;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = balance;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 6; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            if (account.Category == AccountingAccountCategory.Income)
                totalIncome += balance;
            else
                totalExpense += balance;

            row++;
        }

        row++;
        ws.Cell(row, 1).Value = "TOTAL INGRESOS";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 6).Value = totalIncome;
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
        row++;
        ws.Cell(row, 1).Value = "TOTAL GASTOS";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 6).Value = totalExpense;
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
        row++;
        ws.Cell(row, 1).Value = "RESULTADO NETO";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
        ws.Cell(row, 1).Style.Font.FontColor = XLColor.White;
        ws.Cell(row, 6).Value = totalIncome - totalExpense;
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
        ws.Cell(row, 6).Style.Font.FontColor = XLColor.White;
        for (var c = 2; c <= 5; c++)
        {
            ws.Cell(row, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
        }

        ws.Range(1, 1, row, 6).SetAutoFilter();
        ws.Columns(1, 1).Width = 18;
        ws.Columns(2, 2).Width = 15;
        ws.Columns(3, 3).Width = 35;
        ws.Columns(4, 6).Width = 15;
    }

    private void FillBudgetExecution(IXLWorksheet ws, string tenantId, DateTime? from, DateTime? to)
    {
        var headers = new[] { "Periodo", "Codigo Cuenta", "Cuenta", "Valor Aprobado", "Valor Ejecutado", "Diferencia", "% Ejecucion" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var budgetsQuery = _context.Budgets
            .Where(b => b.TenantId == tenantId)
            .Include(b => b.BudgetDetails)
            .ThenInclude(d => d.AccountingAccount)
            .AsQueryable();

        if (from.HasValue)
        {
            var fromYear = from.Value.Year;
            budgetsQuery = budgetsQuery.Where(b => b.FiscalPeriod >= fromYear);
        }
        if (to.HasValue)
        {
            var toYear = to.Value.Year;
            budgetsQuery = budgetsQuery.Where(b => b.FiscalPeriod <= toYear);
        }

        var budgets = budgetsQuery.ToList();
        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var grandApproved = 0m;
        var grandExecuted = 0m;

        foreach (var budget in budgets.OrderBy(b => b.FiscalPeriod))
        {
            foreach (var detail in budget.BudgetDetails.OrderBy(d => d.AccountingAccount != null ? d.AccountingAccount.Code : string.Empty))
            {
                if (detail.AccountingAccount is null) continue;

                var executedSum = _context.EntryLines
                    .Where(l => l.AccountingAccountId == detail.AccountingAccountId
                        && l.AccountingEntry!.TenantId == tenantId
                        && l.AccountingEntry.Status == EntryStatus.Final)
                    .Sum(l => l.Debit + l.Credit);

                var difference = detail.ApprovedValue - executedSum;
                var percentage = detail.ApprovedValue > 0
                    ? (executedSum / detail.ApprovedValue) * 100
                    : 0;

                ws.Cell(row, 1).Value = budget.FiscalPeriod.ToString();
                ws.Cell(row, 2).Value = detail.AccountingAccount.Code;
                ws.Cell(row, 3).Value = detail.AccountingAccount.Name;
                ws.Cell(row, 4).Value = detail.ApprovedValue;
                ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 5).Value = executedSum;
                ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 6).Value = difference;
                ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 7).Value = Math.Round(percentage, 2);
                ws.Cell(row, 7).Style.NumberFormat.Format = "0.00";

                if (row % 2 == 0)
                {
                    for (var c = 1; c <= 7; c++)
                        ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
                }

                grandApproved += detail.ApprovedValue;
                grandExecuted += executedSum;
                row++;
            }
        }

        ws.Cell(row, 1).Value = "TOTALES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 4).Value = grandApproved;
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 5).Value = grandExecuted;
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 6).Value = grandApproved - grandExecuted;
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 7).Value = grandApproved > 0 ? Math.Round((grandExecuted / grandApproved) * 100, 2) : 0;
        ws.Cell(row, 7).Style.Font.Bold = true;
        ws.Cell(row, 7).Style.NumberFormat.Format = "0.00";

        ws.Range(1, 1, row, 7).SetAutoFilter();
        ws.Columns(1, 1).Width = 12;
        ws.Columns(2, 2).Width = 15;
        ws.Columns(3, 3).Width = 35;
        ws.Columns(4, 7).Width = 15;
    }

    private void FillContingencyFund(IXLWorksheet ws, string tenantId)
    {
        var currentFund = _context.ContingencyFunds.FirstOrDefault(f => f.TenantId == tenantId);

        ws.Cell(1, 1).Value = "Fondo de Contingencia";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.White;
        ws.Cell(1, 2).Value = "Saldo Actual";
        ws.Cell(1, 2).Style.Font.Bold = true;
        ws.Cell(1, 2).Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
        ws.Cell(1, 2).Style.Font.FontColor = XLColor.White;
        ws.Cell(2, 1).Value = "Saldo disponible";
        ws.Cell(2, 2).Value = currentFund?.CurrentBalance ?? 0;
        ws.Cell(2, 2).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(2, 2).Style.Font.Bold = true;

        var headers = new[] { "Fecha", "Tipo", "Periodo", "Monto", "Justificacion / Base" };
        var headerRow = 4;
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var row = headerRow + 1;
        var altColor = XLColor.FromHtml("#f8fafc");
        var totalContributions = 0m;
        var totalUsages = 0m;

        var contributions = _context.ContingencyFundContributions
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.ContributionDate)
            .ToList();

        foreach (var c in contributions)
        {
            ws.Cell(row, 1).Value = c.ContributionDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 2).Value = "Aporte";
            ws.Cell(row, 3).Value = c.Period;
            ws.Cell(row, 4).Value = c.Amount;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = "Base: " + c.IncomeBase.ToString("N2") + " (" + c.Percentage.ToString("P") + ")";

            if (row % 2 == 0)
            {
                for (var col = 1; col <= 5; col++)
                    ws.Cell(row, col).Style.Fill.BackgroundColor = altColor;
            }

            totalContributions += c.Amount;
            row++;
        }

        var usages = _context.ContingencyFundUsages
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.ApprovalDate)
            .ToList();

        foreach (var u in usages)
        {
            ws.Cell(row, 1).Value = u.ApprovalDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 2).Value = "Uso";
            ws.Cell(row, 3).Value = string.Empty;
            ws.Cell(row, 4).Value = -u.Amount;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = u.Justification;

            if (row % 2 == 0)
            {
                for (var col = 1; col <= 5; col++)
                    ws.Cell(row, col).Style.Fill.BackgroundColor = altColor;
            }

            totalUsages += u.Amount;
            row++;
        }

        row++;
        ws.Cell(row, 1).Value = "TOTAL APORTES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 4).Value = totalContributions;
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        row++;
        ws.Cell(row, 1).Value = "TOTAL USOS";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 4).Value = totalUsages;
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";

        ws.Columns(1, 1).Width = 15;
        ws.Columns(2, 2).Width = 12;
        ws.Columns(3, 3).Width = 12;
        ws.Columns(4, 4).Width = 15;
        ws.Columns(5, 5).Width = 40;
    }

    private void FillCashFlow(IXLWorksheet ws, string tenantId, DateTime? from, DateTime? to)
    {
        var headers = new[] { "Categoria", "Total Debito (Salidas)", "Total Credito (Entradas)", "Saldo Neto" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var linesQuery = _context.EntryLines
            .Where(l => l.AccountingEntry!.TenantId == tenantId && l.AccountingEntry.Status == EntryStatus.Final)
            .AsQueryable();

        if (from.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.EntryDate >= from.Value);
        if (to.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.EntryDate <= to.Value);

        var accountCategoryMap = _context.AccountingAccounts
            .Where(a => a.TenantId == tenantId)
            .ToDictionary(a => a.Id, a => a.Category);

        var groupedByCategory = linesQuery
            .ToList()
            .GroupBy(l => accountCategoryMap.GetValueOrDefault(l.AccountingAccountId))
            .Select(g => new
            {
                Category = g.Key,
                TotalDebit = g.Sum(l => l.Debit),
                TotalCredit = g.Sum(l => l.Credit)
            })
            .OrderBy(g => g.Category)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var grandDebit = 0m;
        var grandCredit = 0m;

        foreach (var g in groupedByCategory)
        {
            var categoryName = g.Category switch
            {
                AccountingAccountCategory.Asset => "Activo",
                AccountingAccountCategory.Liability => "Pasivo",
                AccountingAccountCategory.Equity => "Patrimonio",
                AccountingAccountCategory.Income => "Ingresos",
                AccountingAccountCategory.Expense => "Gastos",
                _ => g.Category.ToString()
            };

            ws.Cell(row, 1).Value = categoryName;
            ws.Cell(row, 2).Value = g.TotalDebit;
            ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 3).Value = g.TotalCredit;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 4).Value = g.TotalCredit - g.TotalDebit;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 4; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            grandDebit += g.TotalDebit;
            grandCredit += g.TotalCredit;
            row++;
        }

        ws.Cell(row, 1).Value = "TOTALES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = grandDebit;
        ws.Cell(row, 2).Style.Font.Bold = true;
        ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 3).Value = grandCredit;
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 4).Value = grandCredit - grandDebit;
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, 4).SetAutoFilter();
        ws.Columns(1, 1).Width = 20;
        ws.Columns(2, 4).Width = 18;
    }

    private void FillGeneralLedger(IXLWorksheet ws, string tenantId, DateTime? from, DateTime? to)
    {
        var headers = new[] { "Fecha", "Comprobante", "Descripcion", "Cuenta", "Debito", "Credito" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var linesQuery = _context.EntryLines
            .Where(l => l.AccountingEntry!.TenantId == tenantId && l.AccountingEntry.Status == EntryStatus.Final)
            .AsQueryable();

        if (from.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.EntryDate >= from.Value);
        if (to.HasValue)
            linesQuery = linesQuery.Where(l => l.AccountingEntry!.EntryDate <= to.Value);

        var lines = linesQuery
            .OrderBy(l => l.AccountingEntry!.EntryDate)
            .ThenBy(l => l.AccountingEntry!.EntryNumber)
            .Select(l => new
            {
                l.Id,
                EntryDate = l.AccountingEntry!.EntryDate,
                EntryNumber = l.AccountingEntry.EntryNumber,
                Description = l.AccountingEntry.Description,
                AccountId = l.AccountingAccountId,
                l.Debit,
                l.Credit
            })
            .ToList();

        var accountIds = lines.Select(l => l.AccountId).Distinct().ToList();
        var accounts = _context.AccountingAccounts
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionary(a => a.Id);

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var totalDebit = 0m;
        var totalCredit = 0m;

        foreach (var line in lines)
        {
            var account = accounts.GetValueOrDefault(line.AccountId);

            ws.Cell(row, 1).Value = line.EntryDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 2).Value = line.EntryNumber.ToString();
            ws.Cell(row, 3).Value = line.Description;
            ws.Cell(row, 4).Value = account != null ? account.Code + " - " + account.Name : string.Empty;
            ws.Cell(row, 5).Value = line.Debit;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = line.Credit;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 6; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            totalDebit += line.Debit;
            totalCredit += line.Credit;
            row++;
        }

        ws.Cell(row, 1).Value = "TOTALES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 5).Value = totalDebit;
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 6).Value = totalCredit;
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, 6).SetAutoFilter();
        ws.Columns(1, 1).Width = 14;
        ws.Columns(2, 2).Width = 12;
        ws.Columns(3, 3).Width = 40;
        ws.Columns(4, 4).Width = 35;
        ws.Columns(5, 6).Width = 15;
    }

    private void FillJournalEntryDetail(IXLWorksheet ws, string tenantId, DateTime? from, DateTime? to)
    {
        var headers = new[] { "Comprobante", "Fecha", "Descripcion", "Codigo Cuenta", "Cuenta", "Debito", "Credito" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var entriesQuery = _context.AccountingEntries
            .Where(e => e.TenantId == tenantId && e.Status == EntryStatus.Final)
            .Include(e => e.Lines)
            .ThenInclude(l => l.AccountingAccount)
            .AsQueryable();

        if (from.HasValue)
            entriesQuery = entriesQuery.Where(e => e.EntryDate >= from.Value);
        if (to.HasValue)
            entriesQuery = entriesQuery.Where(e => e.EntryDate <= to.Value);

        var entries = entriesQuery
            .OrderBy(e => e.EntryDate)
            .ThenBy(e => e.EntryNumber)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var grandDebit = 0m;
        var grandCredit = 0m;

        foreach (var entry in entries)
        {
            ws.Cell(row, 1).Value = entry.EntryNumber.ToString();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = entry.EntryDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 3).Value = entry.Description;
            ws.Cell(row, 4).Value = string.Empty;
            ws.Cell(row, 5).Value = string.Empty;
            ws.Cell(row, 6).Value = string.Empty;
            ws.Cell(row, 7).Value = string.Empty;

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 7; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            row++;

            foreach (var line in entry.Lines.OrderBy(l => l.AccountingAccount != null ? l.AccountingAccount.Code : string.Empty))
            {
                var account = line.AccountingAccount;
                ws.Cell(row, 1).Value = string.Empty;
                ws.Cell(row, 2).Value = string.Empty;
                ws.Cell(row, 3).Value = string.Empty;
                ws.Cell(row, 4).Value = account != null ? account.Code : string.Empty;
                ws.Cell(row, 5).Value = account != null ? account.Name : string.Empty;
                ws.Cell(row, 6).Value = line.Debit;
                ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
                ws.Cell(row, 7).Value = line.Credit;
                ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";

                if (row % 2 == 0)
                {
                    for (var c = 1; c <= 7; c++)
                        ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
                }

                grandDebit += line.Debit;
                grandCredit += line.Credit;
                row++;
            }
        }

        ws.Cell(row, 1).Value = "TOTALES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 6).Value = grandDebit;
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 7).Value = grandCredit;
        ws.Cell(row, 7).Style.Font.Bold = true;
        ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, 7).SetAutoFilter();
        ws.Columns(1, 1).Width = 12;
        ws.Columns(2, 2).Width = 14;
        ws.Columns(3, 3).Width = 40;
        ws.Columns(4, 4).Width = 15;
        ws.Columns(5, 5).Width = 35;
        ws.Columns(6, 7).Width = 15;
    }

    private void FillPortfolioAging(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Unidad", "Torre/Bloque", "0-30 Dias", "31-60 Dias", "61-90 Dias", "90+ Dias", "Total Adeudado" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var fees = _context.UnitFees
            .Where(f => f.TenantId == tenantId && f.Status != FeeStatus.FullyPaid)
            .Include(f => f.Unit)
            .ToList();

        var groupedByUnit = fees
            .GroupBy(f => f.UnitId)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var now = DateTime.UtcNow;
        var totalBucket0 = 0m;
        var totalBucket1 = 0m;
        var totalBucket2 = 0m;
        var totalBucket3 = 0m;
        var totalGeneral = 0m;

        foreach (var group in groupedByUnit.OrderBy(g => g.FirstOrDefault()?.Unit?.Identifier))
        {
            var unit = group.FirstOrDefault()?.Unit;
            if (unit is null) continue;

            var bucket0 = 0m;
            var bucket1 = 0m;
            var bucket2 = 0m;
            var bucket3 = 0m;

            foreach (var fee in group)
            {
                var daysOverdue = (now - fee.DueDate).Days;
                if (daysOverdue <= 0)
                    continue;
                if (daysOverdue <= 30)
                    bucket0 += fee.BalanceAmount;
                else if (daysOverdue <= 60)
                    bucket1 += fee.BalanceAmount;
                else if (daysOverdue <= 90)
                    bucket2 += fee.BalanceAmount;
                else
                    bucket3 += fee.BalanceAmount;
            }

            var total = bucket0 + bucket1 + bucket2 + bucket3;

            ws.Cell(row, 1).Value = unit.Identifier;
            ws.Cell(row, 2).Value = unit.TowerOrBlock;
            ws.Cell(row, 3).Value = bucket0;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 4).Value = bucket1;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = bucket2;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = bucket3;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Value = total;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 7; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            totalBucket0 += bucket0;
            totalBucket1 += bucket1;
            totalBucket2 += bucket2;
            totalBucket3 += bucket3;
            totalGeneral += total;
            row++;
        }

        ws.Cell(row, 1).Value = "TOTALES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 3).Value = totalBucket0;
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 4).Value = totalBucket1;
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 5).Value = totalBucket2;
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 6).Value = totalBucket3;
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 7).Value = totalGeneral;
        ws.Cell(row, 7).Style.Font.Bold = true;
        ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, 7).SetAutoFilter();
        ws.Columns(1, 1).Width = 15;
        ws.Columns(2, 2).Width = 15;
        ws.Columns(3, 7).Width = 14;
    }

    private void FillPortfolioByUnit(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Unidad", "Torre/Bloque", "Total Adeudado", "Cantidad Cuotas", "Saldo Total" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var fees = _context.UnitFees
            .Where(f => f.TenantId == tenantId && f.Status != FeeStatus.FullyPaid)
            .Include(f => f.Unit)
            .ToList();

        var groupedByUnit = fees
            .GroupBy(f => f.UnitId)
            .Select(g => new
            {
                UnitId = g.Key,
                Unit = g.FirstOrDefault()!.Unit,
                TotalBalance = g.Sum(f => f.BalanceAmount),
                FeeCount = g.Count(),
                TotalOriginal = g.Sum(f => f.FeeValue)
            })
            .OrderByDescending(g => g.TotalBalance)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var grandBalance = 0m;
        var grandCount = 0;
        var grandOriginal = 0m;

        foreach (var g in groupedByUnit)
        {
            if (g.Unit is null) continue;

            ws.Cell(row, 1).Value = g.Unit.Identifier;
            ws.Cell(row, 2).Value = g.Unit.TowerOrBlock;
            ws.Cell(row, 3).Value = g.TotalBalance;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 4).Value = g.FeeCount;
            ws.Cell(row, 5).Value = g.TotalOriginal;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 5; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            grandBalance += g.TotalBalance;
            grandCount += g.FeeCount;
            grandOriginal += g.TotalOriginal;
            row++;
        }

        ws.Cell(row, 1).Value = "TOTALES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 3).Value = grandBalance;
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 4).Value = grandCount;
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 5).Value = grandOriginal;
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, 5).SetAutoFilter();
        ws.Columns(1, 1).Width = 15;
        ws.Columns(2, 2).Width = 15;
        ws.Columns(3, 5).Width = 15;
    }

    private void FillTopDebtors(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "#", "Unidad", "Propietario", "Total Adeudado", "Cuotas Pendientes" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var fees = _context.UnitFees
            .Where(f => f.TenantId == tenantId && f.Status != FeeStatus.FullyPaid)
            .Include(f => f.Unit)
            .ThenInclude(u => u!.UnitOwners)
            .ThenInclude(uo => uo.Owner)
            .ToList();

        var groupedByUnit = fees
            .GroupBy(f => f.UnitId)
            .Select(g => new
            {
                UnitId = g.Key,
                Unit = g.FirstOrDefault()!.Unit,
                TotalBalance = g.Sum(f => f.BalanceAmount),
                FeeCount = g.Count()
            })
            .OrderByDescending(g => g.TotalBalance)
            .Take(20)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var rank = 1;
        var grandBalance = 0m;
        var grandCount = 0;

        foreach (var g in groupedByUnit)
        {
            if (g.Unit is null) continue;

            var ownerName = string.Empty;
            var activeOwner = g.Unit.UnitOwners
                .Where(uo => uo.IsActive)
                .Select(uo => uo.Owner)
                .FirstOrDefault();
            if (activeOwner != null)
            {
                ownerName = activeOwner.FullNameOrCompanyName;
            }

            ws.Cell(row, 1).Value = rank;
            ws.Cell(row, 2).Value = g.Unit.Identifier;
            ws.Cell(row, 3).Value = ownerName;
            ws.Cell(row, 4).Value = g.TotalBalance;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = g.FeeCount;

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 5; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            grandBalance += g.TotalBalance;
            grandCount += g.FeeCount;
            rank++;
            row++;
        }

        ws.Cell(row, 1).Value = "TOTALES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 4).Value = grandBalance;
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 5).Value = grandCount;
        ws.Cell(row, 5).Style.Font.Bold = true;

        ws.Range(1, 1, row, 5).SetAutoFilter();
        ws.Columns(1, 1).Width = 5;
        ws.Columns(2, 2).Width = 15;
        ws.Columns(3, 3).Width = 35;
        ws.Columns(4, 5).Width = 15;
    }

    private void FillPeriodCollection(IXLWorksheet ws, string tenantId, DateTime? from, DateTime? to)
    {
        var headers = new[] { "Fecha", "Unidad", "Referencia", "Metodo", "Monto", "Avance", "Notas" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var paymentsQuery = _context.Payments
            .Where(p => p.TenantId == tenantId)
            .Include(p => p.Unit)
            .Include(p => p.Allocations)
            .AsQueryable();

        if (from.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.PaymentDate >= from.Value);
        if (to.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.PaymentDate <= to.Value);

        var payments = paymentsQuery
            .OrderByDescending(p => p.PaymentDate)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var totalAmount = 0m;
        var totalAdvance = 0m;

        foreach (var payment in payments)
        {
            var methodName = payment.PaymentMethod.ToString();
            var unitIdentifier = payment.Unit != null ? payment.Unit.Identifier : string.Empty;

            ws.Cell(row, 1).Value = payment.PaymentDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 2).Value = unitIdentifier;
            ws.Cell(row, 3).Value = payment.ReferenceNumber;
            ws.Cell(row, 4).Value = methodName;
            ws.Cell(row, 5).Value = payment.Amount;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = payment.AdvanceAmount;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Value = payment.Notes;

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 7; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            totalAmount += payment.Amount;
            totalAdvance += payment.AdvanceAmount;
            row++;
        }

        ws.Cell(row, 1).Value = "TOTALES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 5).Value = totalAmount;
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 6).Value = totalAdvance;
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, 7).SetAutoFilter();
        ws.Columns(1, 1).Width = 14;
        ws.Columns(2, 2).Width = 15;
        ws.Columns(3, 3).Width = 20;
        ws.Columns(4, 4).Width = 12;
        ws.Columns(5, 6).Width = 15;
        ws.Columns(7, 7).Width = 30;
    }

    private void FillPaymentAgreements(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Unidad", "Total Deuda", "Valor Cuota", "Numero Cuotas", "Estado", "Fecha Inicio" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var agreements = _context.PaymentAgreements
            .Where(a => a.TenantId == tenantId && a.Status == AgreementStatus.Active)
            .Include(a => a.Unit)
            .Include(a => a.Installments)
            .OrderBy(a => a.StartedAt)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var totalDebt = 0m;
        var totalInstallmentValue = 0m;
        var totalInstallments = 0;

        foreach (var agreement in agreements)
        {
            var unitIdentifier = agreement.Unit != null ? agreement.Unit.Identifier : string.Empty;
            var statusName = agreement.Status.ToString();
            var installmentsPaid = agreement.Installments.Count(i => i.Status == AgreementInstallmentStatus.Paid);
            var installmentsPending = agreement.Installments.Count(i => i.Status == AgreementInstallmentStatus.Pending);

            ws.Cell(row, 1).Value = unitIdentifier;
            ws.Cell(row, 2).Value = agreement.TotalDebtIncluded;
            ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 3).Value = agreement.InstallmentAmount;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 4).Value = installmentsPaid + " / " + agreement.NumberOfInstallments;
            ws.Cell(row, 5).Value = statusName;
            ws.Cell(row, 6).Value = agreement.StartedAt.ToString("yyyy-MM-dd");

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 6; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            totalDebt += agreement.TotalDebtIncluded;
            totalInstallmentValue += agreement.InstallmentAmount;
            totalInstallments += agreement.NumberOfInstallments;
            row++;
        }

        ws.Cell(row, 1).Value = "TOTALES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = totalDebt;
        ws.Cell(row, 2).Style.Font.Bold = true;
        ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 3).Value = totalInstallmentValue;
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 4).Value = totalInstallments.ToString();
        ws.Cell(row, 4).Style.Font.Bold = true;

        ws.Range(1, 1, row, 6).SetAutoFilter();
        ws.Columns(1, 1).Width = 15;
        ws.Columns(2, 3).Width = 15;
        ws.Columns(4, 4).Width = 18;
        ws.Columns(5, 5).Width = 14;
        ws.Columns(6, 6).Width = 14;
    }

    private void FillActiveContracts(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Contrato", "Proveedor", "Tipo", "Valor Total", "Valor Mensual", "Fecha Inicio", "Fecha Fin" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var contracts = _context.Contracts
            .Where(c => c.TenantId == tenantId && c.Status == ContractStatus.Active)
            .Include(c => c.Provider)
            .OrderBy(c => c.StartDate)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var totalValue = 0m;
        var totalMonthly = 0m;

        foreach (var contract in contracts)
        {
            var providerName = contract.Provider != null ? contract.Provider.BusinessName : string.Empty;
            var typeName = contract.ContractType.ToString();

            ws.Cell(row, 1).Value = contract.ContractNumber;
            ws.Cell(row, 2).Value = providerName;
            ws.Cell(row, 3).Value = typeName;
            ws.Cell(row, 4).Value = contract.TotalValue;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = contract.MonthlyValue;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = contract.StartDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 7).Value = contract.EndDate.ToString("yyyy-MM-dd");

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 7; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            totalValue += contract.TotalValue;
            totalMonthly += contract.MonthlyValue;
            row++;
        }

        ws.Cell(row, 1).Value = "TOTALES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 4).Value = totalValue;
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 5).Value = totalMonthly;
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, 7).SetAutoFilter();
        ws.Columns(1, 1).Width = 18;
        ws.Columns(2, 2).Width = 30;
        ws.Columns(3, 3).Width = 14;
        ws.Columns(4, 5).Width = 15;
        ws.Columns(6, 7).Width = 14;
    }

    private void FillOwnerRegistry(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Documento", "Nombre", "Email", "Telefono", "Tipo", "Unidades" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var owners = _context.Owners
            .Where(o => o.TenantId == tenantId && o.IsActive)
            .Include(o => o.UnitOwners)
            .ThenInclude(uo => uo.Unit)
            .OrderBy(o => o.FullNameOrCompanyName)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");

        foreach (var owner in owners)
        {
            var docTypeName = owner.DocumentType.ToString();
            var document = docTypeName + " " + owner.DocumentNumber;
            var ownerTypeName = owner.OwnerType == OwnerType.NaturalPerson ? "Persona Natural" : "Persona Juridica";
            var unitsList = string.Join(", ", owner.UnitOwners
                .Where(uo => uo.IsActive)
                .Select(uo => uo.Unit != null ? uo.Unit.Identifier : string.Empty));

            ws.Cell(row, 1).Value = document;
            ws.Cell(row, 2).Value = owner.FullNameOrCompanyName;
            ws.Cell(row, 3).Value = owner.Email;
            ws.Cell(row, 4).Value = owner.MainPhone;
            ws.Cell(row, 5).Value = ownerTypeName;
            ws.Cell(row, 6).Value = unitsList;

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 6; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            row++;
        }

        ws.Range(1, 1, row - 1, 6).SetAutoFilter();
        ws.Columns(1, 1).Width = 25;
        ws.Columns(2, 2).Width = 35;
        ws.Columns(3, 3).Width = 30;
        ws.Columns(4, 4).Width = 15;
        ws.Columns(5, 5).Width = 18;
        ws.Columns(6, 6).Width = 25;
    }

    private void FillPQRSummary(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Tipo PQR", "Categoria", "Estado", "Cantidad" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var records = _context.PqrRecords
            .Where(p => p.TenantId == tenantId)
            .ToList();

        var grouped = records
            .GroupBy(p => new { p.PQRType, p.Category, p.Status })
            .Select(g => new
            {
                g.Key.PQRType,
                g.Key.Category,
                g.Key.Status,
                Count = g.Count()
            })
            .OrderBy(g => g.PQRType)
            .ThenBy(g => g.Category)
            .ThenBy(g => g.Status)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var totalCount = 0;

        foreach (var g in grouped)
        {
            ws.Cell(row, 1).Value = g.PQRType.ToString();
            ws.Cell(row, 2).Value = g.Category.ToString();
            ws.Cell(row, 3).Value = g.Status.ToString();
            ws.Cell(row, 4).Value = g.Count;

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 4; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            totalCount += g.Count;
            row++;
        }

        ws.Cell(row, 1).Value = "TOTAL";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 4).Value = totalCount;
        ws.Cell(row, 4).Style.Font.Bold = true;

        ws.Range(1, 1, row, 4).SetAutoFilter();
        ws.Columns(1, 1).Width = 15;
        ws.Columns(2, 2).Width = 18;
        ws.Columns(3, 3).Width = 15;
        ws.Columns(4, 4).Width = 10;
    }

    private void FillCommonAreaUsage(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Espacio", "Total Reservas", "Horas Totales", "Ingresos Totales" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var reservations = _context.Reservations
            .Where(r => r.TenantId == tenantId
                && (r.Status == ReservationStatus.Completed
                    || r.Status == ReservationStatus.InUse
                    || r.Status == ReservationStatus.Approved))
            .Include(r => r.Space)
            .ToList();

        var groupedBySpace = reservations
            .GroupBy(r => r.SpaceId)
            .Select(g => new
            {
                SpaceId = g.Key,
                FirstReservation = g.FirstOrDefault(),
                Count = g.Count(),
                TotalHours = g.Sum(r => (r.EndDateTime - r.StartDateTime).TotalHours),
                TotalRevenue = g.Sum(r => r.TotalCost)
            })
            .OrderByDescending(g => g.Count)
            .ToList()
            .Select(g => new
            {
                g.SpaceId,
                SpaceName = g.FirstReservation?.Space?.Name ?? "Sin nombre",
                g.Count,
                g.TotalHours,
                g.TotalRevenue
            })
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var grandCount = 0;
        var grandHours = 0.0;
        var grandRevenue = 0m;

        foreach (var g in groupedBySpace)
        {
            ws.Cell(row, 1).Value = g.SpaceName;
            ws.Cell(row, 2).Value = g.Count;
            ws.Cell(row, 3).Value = Math.Round(g.TotalHours, 1);
            ws.Cell(row, 4).Value = g.TotalRevenue;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 4; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            grandCount += g.Count;
            grandHours += g.TotalHours;
            grandRevenue += g.TotalRevenue;
            row++;
        }

        ws.Cell(row, 1).Value = "TOTALES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = grandCount;
        ws.Cell(row, 2).Style.Font.Bold = true;
        ws.Cell(row, 3).Value = Math.Round(grandHours, 1);
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 4).Value = grandRevenue;
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, 4).SetAutoFilter();
        ws.Columns(1, 1).Width = 25;
        ws.Columns(2, 2).Width = 14;
        ws.Columns(3, 3).Width = 14;
        ws.Columns(4, 4).Width = 15;
    }

    private void FillMaintenanceSummary(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Tipo", "Estado", "Cantidad", "Costo Estimado", "Costo Real" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var workOrders = _context.WorkOrders
            .Where(w => w.TenantId == tenantId)
            .ToList();

        var grouped = workOrders
            .GroupBy(w => new { w.OrderType, w.Status })
            .Select(g => new
            {
                g.Key.OrderType,
                g.Key.Status,
                Count = g.Count(),
                EstimatedCost = g.Sum(w => w.EstimatedCost),
                ActualCost = g.Sum(w => w.ActualCost)
            })
            .OrderBy(g => g.OrderType)
            .ThenBy(g => g.Status)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var grandCount = 0;
        var grandEstimated = 0m;
        var grandActual = 0m;

        foreach (var g in grouped)
        {
            var typeName = g.OrderType == WorkOrderType.Preventive ? "Preventivo" : "Correctivo";

            ws.Cell(row, 1).Value = typeName;
            ws.Cell(row, 2).Value = g.Status.ToString();
            ws.Cell(row, 3).Value = g.Count;
            ws.Cell(row, 4).Value = g.EstimatedCost;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = g.ActualCost;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 5; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            grandCount += g.Count;
            grandEstimated += g.EstimatedCost;
            grandActual += g.ActualCost;
            row++;
        }

        ws.Cell(row, 1).Value = "TOTALES";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 3).Value = grandCount;
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 4).Value = grandEstimated;
        ws.Cell(row, 4).Style.Font.Bold = true;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 5).Value = grandActual;
        ws.Cell(row, 5).Style.Font.Bold = true;
        ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, 5).SetAutoFilter();
        ws.Columns(1, 1).Width = 14;
        ws.Columns(2, 2).Width = 18;
        ws.Columns(3, 3).Width = 10;
        ws.Columns(4, 5).Width = 15;
    }

    private void FillCommunicationSummary(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Estado", "Tipo Audiencia", "Cantidad", "Fecha Envio" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var communications = _context.Communications
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.SentAt)
            .ToList();

        var grouped = communications
            .GroupBy(c => new { c.Status, c.AudienceType })
            .Select(g => new
            {
                g.Key.Status,
                g.Key.AudienceType,
                Count = g.Count(),
                LastSent = g.Max(c => c.SentAt)
            })
            .OrderBy(g => g.Status)
            .ThenBy(g => g.AudienceType)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");
        var totalCount = 0;

        foreach (var g in grouped)
        {
            ws.Cell(row, 1).Value = g.Status.ToString();
            ws.Cell(row, 2).Value = g.AudienceType.ToString();
            ws.Cell(row, 3).Value = g.Count;
            ws.Cell(row, 4).Value = g.LastSent.HasValue ? g.LastSent.Value.ToString("yyyy-MM-dd") : string.Empty;

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 4; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            totalCount += g.Count;
            row++;
        }

        ws.Cell(row, 1).Value = "TOTAL";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 3).Value = totalCount;
        ws.Cell(row, 3).Style.Font.Bold = true;

        ws.Range(1, 1, row, 4).SetAutoFilter();
        ws.Columns(1, 1).Width = 14;
        ws.Columns(2, 2).Width = 18;
        ws.Columns(3, 3).Width = 10;
        ws.Columns(4, 4).Width = 14;
    }

    private void FillAssemblyMinutes(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Asamblea", "Fecha", "Estado", "Presidente", "Secretario", "Generado" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var minutes = _context.AssemblyMinutes
            .Where(m => m.TenantId == tenantId)
            .Include(m => m.Assembly)
            .OrderByDescending(m => m.GeneratedAt)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");

        foreach (var minute in minutes)
        {
            var assemblyTitle = minute.Assembly != null ? minute.Assembly.Title : string.Empty;
            var assemblyDate = minute.Assembly != null ? minute.Assembly.ScheduledDate.ToString("yyyy-MM-dd") : string.Empty;

            ws.Cell(row, 1).Value = assemblyTitle;
            ws.Cell(row, 2).Value = assemblyDate;
            ws.Cell(row, 3).Value = minute.Status.ToString();
            ws.Cell(row, 4).Value = minute.PresidentName ?? string.Empty;
            ws.Cell(row, 5).Value = minute.SecretaryName ?? string.Empty;
            ws.Cell(row, 6).Value = minute.GeneratedAt.ToString("yyyy-MM-dd");

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 6; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            row++;
        }

        ws.Range(1, 1, row - 1, 6).SetAutoFilter();
        ws.Columns(1, 1).Width = 30;
        ws.Columns(2, 2).Width = 14;
        ws.Columns(3, 3).Width = 12;
        ws.Columns(4, 5).Width = 25;
        ws.Columns(6, 6).Width = 14;
    }

    private void FillAssemblyDecisions(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Asamblea", "Tema", "Votos a Favor", "Votos en Contra", "Abstenciones", "Resultado" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var agendaItems = _context.AssemblyAgendaItems
            .Where(a => a.TenantId == tenantId && a.RequiresVoting)
            .Include(a => a.Assembly)
            .OrderByDescending(a => a.Assembly != null ? a.Assembly.ScheduledDate : DateTime.MinValue)
            .ThenBy(a => a.SequenceNumber)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");

        foreach (var item in agendaItems)
        {
            var assemblyTitle = item.Assembly != null ? item.Assembly.Title : string.Empty;
            var result = string.Empty;
            if (item.IsApproved.HasValue)
            {
                result = item.IsApproved.Value ? "Aprobado" : "Rechazado";
            }
            else
            {
                result = "Pendiente";
            }

            ws.Cell(row, 1).Value = assemblyTitle;
            ws.Cell(row, 2).Value = item.Title;
            ws.Cell(row, 3).Value = item.VotesInFavorCoefficients;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 4).Value = item.VotesAgainstCoefficients;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = item.AbstentionCoefficients;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 6).Value = result;

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 6; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            row++;
        }

        ws.Range(1, 1, row - 1, 6).SetAutoFilter();
        ws.Columns(1, 1).Width = 30;
        ws.Columns(2, 2).Width = 35;
        ws.Columns(3, 5).Width = 14;
        ws.Columns(6, 6).Width = 14;
    }

    private void FillCouncilHistory(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Usuario", "Rol", "Asignado", "Vence", "Activo" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var councilRoles = _context.UserTenantRoles
            .Where(r => r.TenantId == tenantId && r.Role == AppRole.Council)
            .OrderByDescending(r => r.AssignedAt)
            .ToList();

        var userIds = councilRoles.Select(r => r.UserId).Distinct().ToList();
        var users = _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionary(u => u.Id);

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");

        foreach (var role in councilRoles)
        {
            var userName = users.TryGetValue(role.UserId, out var user) ? user.FullName : role.UserId;
            var expiresAt = role.ExpiresAt.HasValue ? role.ExpiresAt.Value.ToString("yyyy-MM-dd") : "Sin vencimiento";
            var isActive = role.IsActive ? "Si" : "No";

            ws.Cell(row, 1).Value = userName;
            ws.Cell(row, 2).Value = role.Role.ToString();
            ws.Cell(row, 3).Value = role.AssignedAt.ToString("yyyy-MM-dd");
            ws.Cell(row, 4).Value = expiresAt;
            ws.Cell(row, 5).Value = isActive;

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 5; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            row++;
        }

        ws.Range(1, 1, row - 1, 5).SetAutoFilter();
        ws.Columns(1, 1).Width = 30;
        ws.Columns(2, 2).Width = 14;
        ws.Columns(3, 4).Width = 14;
        ws.Columns(5, 5).Width = 10;
    }

    private void FillAssemblyQuorum(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Asamblea", "Fecha", "Coeficientes Totales", "Coeficientes Asistentes", "Quorum Alcanzado", "% Asistencia" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var assemblies = _context.Assemblies
            .Where(a => a.TenantId == tenantId)
            .Include(a => a.Attendances)
            .OrderByDescending(a => a.ScheduledDate)
            .ToList();

        var row = 2;
        var altColor = XLColor.FromHtml("#f8fafc");

        foreach (var assembly in assemblies)
        {
            var presentCoefficients = assembly.Attendances
                .Where(a => a.Status == AttendanceStatus.Present)
                .Sum(a => a.Coefficient);

            var quorumAchieved = assembly.QuorumAchievedFirstCall || assembly.QuorumAchievedSecondCall;
            var percentage = assembly.TotalCoefficients > 0
                ? (presentCoefficients / assembly.TotalCoefficients) * 100
                : 0;

            ws.Cell(row, 1).Value = assembly.Title;
            ws.Cell(row, 2).Value = assembly.ScheduledDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 3).Value = assembly.TotalCoefficients;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 4).Value = presentCoefficients;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = quorumAchieved ? "Si" : "No";
            ws.Cell(row, 6).Value = Math.Round(percentage, 2);
            ws.Cell(row, 6).Style.NumberFormat.Format = "0.00";

            if (row % 2 == 0)
            {
                for (var c = 1; c <= 6; c++)
                    ws.Cell(row, c).Style.Fill.BackgroundColor = altColor;
            }

            row++;
        }

        ws.Range(1, 1, row - 1, 6).SetAutoFilter();
        ws.Columns(1, 1).Width = 30;
        ws.Columns(2, 2).Width = 14;
        ws.Columns(3, 4).Width = 18;
        ws.Columns(5, 5).Width = 14;
        ws.Columns(6, 6).Width = 12;
    }

    private static string BuildPeriodLabel(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue)
            return from.Value.ToString("yyyyMMdd") + "_" + to.Value.ToString("yyyyMMdd");
        if (from.HasValue)
            return from.Value.ToString("yyyyMMdd");
        return DateTime.UtcNow.ToString("yyyyMMdd");
    }
}
