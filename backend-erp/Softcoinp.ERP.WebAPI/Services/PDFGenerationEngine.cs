using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class PDFGenerationEngine
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public PDFGenerationEngine(
        ApplicationDbContext context,
        IWebHostEnvironment env)
    {
        _context = context;
        _env = env;

        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<GeneratedReport> GenerateReportAsync(
        string tenantId, string reportTypeCode, string format, string userId,
        DateTime? periodFrom, DateTime? periodTo, string? parameters, string? notes,
        Guid? recurringConfigId = null)
    {
        if (!Enum.TryParse<ReportTypeEnum>(reportTypeCode, out var reportTypeEnum))
            throw new InvalidOperationException("Invalid report type code: " + reportTypeCode);

        var reportType = await _context.ReportTypes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.ReportTypeCode == reportTypeEnum);

        if (reportType is null)
            throw new InvalidOperationException("Report type not found: " + reportTypeCode);

        var template = await _context.PDFTemplates
            .Where(t => t.TenantId == tenantId && t.ReportTypeCode == reportTypeCode)
            .OrderByDescending(t => t.IsDefault)
            .FirstOrDefaultAsync();

        var tenantConfig = await _context.TenantConfigurations
            .FirstOrDefaultAsync();

        var periodLabel = BuildPeriodLabel(periodFrom, periodTo);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var dir = Path.Combine(_env.WebRootPath ?? "wwwroot", "reports", tenantId, reportTypeCode);
        Directory.CreateDirectory(dir);

        var fileName = $"{reportTypeCode}_{periodLabel}_{timestamp}.pdf";
        var filePath = Path.Combine(dir, fileName);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                page.Header().Element(h => ComposeHeader(h, template, tenantConfig, reportType));

                page.Content().Element(c => ComposeContent(c, tenantId, reportTypeCode, periodFrom, periodTo, parameters));

                page.Footer().Element(f => ComposeFooter(f, template, tenantConfig, reportType, userId));
            });
        });

        document.GeneratePdf(filePath);

        var fileInfo = new FileInfo(filePath);
        var generated = new GeneratedReport
        {
            TenantId = tenantId,
            ReportTypeId = reportType.Id,
            Format = Enum.Parse<ReportFormat>(format),
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

    private void ComposeHeader(IContainer container, PDFTemplate? template, TenantConfiguration? config, ReportType reportType)
    {
        container.Column(col =>
        {
            var primary = ParseColor(template?.PrimaryColor ?? "#059669");

            col.Item().Row(row =>
            {
                if (template?.LogoFilePath is not null && File.Exists(template.LogoFilePath))
                {
                    row.ConstantItem(80).Image(template.LogoFilePath).FitWidth();
                }

                var headerText = template?.HeaderText ?? (config?.OfficialName ?? "Propiedad Horizontal");
                row.RelativeItem().PaddingLeft(10).AlignMiddle().Column(c2 =>
                {
                    c2.Item().Text(headerText).FontSize(16).Bold().FontColor(primary);
                    if (config is not null)
                    {
                        var nitDisplay = $"NIT {config.Nit}-{config.VerificationDigit}";
                        c2.Item().Text(nitDisplay).FontSize(9).FontColor(Colors.Grey.Darken2);

                        if (!string.IsNullOrEmpty(config.Address))
                            c2.Item().Text(config.Address).FontSize(8).FontColor(Colors.Grey.Darken1);
                    }
                    c2.Item().Text(reportType.Name).FontSize(12).Bold().FontColor(Colors.Grey.Darken4);
                });
            });

            col.Item().PaddingVertical(6).LineHorizontal(1).LineColor(primary);
        });
    }

    private void ComposeContent(IContainer container, string tenantId, string reportTypeCode,
        DateTime? periodFrom, DateTime? periodTo, string? parameters)
    {
        container.Column(col =>
        {
            if (periodFrom.HasValue || periodTo.HasValue)
            {
                var fromStr = periodFrom?.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO")) ?? "Inicio";
                var toStr = periodTo?.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO")) ?? "Fin";
                col.Item().Text($"Periodo: {fromStr} - {toStr}").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().PaddingBottom(8);
            }

            switch (reportTypeCode)
            {
                case "BalanceGeneral":
                    RenderBalanceGeneral(col, tenantId);
                    break;
                case "IncomeExpense":
                    RenderIncomeExpense(col, tenantId);
                    break;
                case "TrialBalance":
                    RenderTrialBalance(col, tenantId, periodFrom, periodTo);
                    break;
                case "BudgetExecution":
                    RenderBudgetExecution(col, tenantId, periodFrom, periodTo);
                    break;
                case "ContingencyFund":
                    RenderContingencyFund(col, tenantId);
                    break;
                case "CashFlow":
                    RenderCashFlow(col, tenantId, periodFrom, periodTo);
                    break;
                case "GeneralLedger":
                    RenderGeneralLedger(col, tenantId, periodFrom, periodTo);
                    break;
                case "SubsidiaryLedger":
                    RenderGeneralLedger(col, tenantId, periodFrom, periodTo);
                    break;
                case "JournalEntryDetail":
                    RenderJournalEntryDetail(col, tenantId, periodFrom, periodTo);
                    break;
                case "PortfolioAging":
                    RenderPortfolioAging(col, tenantId);
                    break;
                case "PortfolioByUnit":
                    RenderPortfolioByUnit(col, tenantId);
                    break;
                case "TopDebtors":
                    RenderTopDebtors(col, tenantId);
                    break;
                case "PeriodCollection":
                    RenderPeriodCollection(col, tenantId, periodFrom, periodTo);
                    break;
                case "PortfolioProjection":
                    RenderPortfolioProjection(col, tenantId);
                    break;
                case "PaymentAgreements":
                    RenderPaymentAgreements(col, tenantId);
                    break;
                case "PQRSummary":
                    RenderPQRSummary(col, tenantId, periodFrom, periodTo);
                    break;
                case "CommonAreaUsage":
                    RenderCommonAreaUsage(col, tenantId, periodFrom, periodTo);
                    break;
                case "MaintenanceSummary":
                    RenderMaintenanceSummary(col, tenantId, periodFrom, periodTo);
                    break;
                case "ActiveContracts":
                    RenderActiveContracts(col, tenantId);
                    break;
                case "CommunicationSummary":
                    RenderCommunicationSummary(col, tenantId, periodFrom, periodTo);
                    break;
                case "AssemblyMinutes":
                    RenderAssemblyMinutes(col, tenantId, periodFrom, periodTo);
                    break;
                case "AssemblyDecisions":
                    RenderAssemblyDecisions(col, tenantId, periodFrom, periodTo);
                    break;
                case "CouncilHistory":
                    RenderCouncilHistory(col, tenantId);
                    break;
                case "AssemblyQuorum":
                    RenderAssemblyQuorum(col, tenantId, periodFrom, periodTo);
                    break;
                case "AnnualManagementReport":
                    col.Item().Text("Informe de Gestion Anual").FontSize(14).Bold();
                    col.Item().Padding(10).Text("Use GenerateAnnualReportPdfAsync para este tipo.").FontColor(Colors.Grey.Darken2);
                    break;
                case "OwnerRegistry":
                    RenderOwnerRegistry(col, tenantId);
                    break;
                default:
                    col.Item().Text("Tipo de reporte no soportado: " + reportTypeCode)
                        .FontColor(Colors.Red.Medium);
                    break;
            }
        });
    }

    private void ComposeFooter(IContainer container, PDFTemplate? template, TenantConfiguration? config,
        ReportType reportType, string userId)
    {
        container.Column(col =>
        {
            var primary = ParseColor(template?.PrimaryColor ?? "#059669");
            col.Item().LineHorizontal(1).LineColor(primary);
            col.Item().PaddingVertical(4);

            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c2 =>
                {
                    var signName = template?.SignatureName ?? config?.LegalRepresentativeName ?? "Administrador";
                    var signRole = template?.SignatureRole ?? "Administrador";
                    c2.Item().Text($"{signName}").FontSize(8).Bold().FontColor(Colors.Grey.Darken3);
                    c2.Item().Text($"{signRole}").FontSize(7).FontColor(Colors.Grey.Darken2);
                });

                row.RelativeItem().AlignRight().Column(c2 =>
                {
                    c2.Item().Text(DateTime.UtcNow.ToString("dd/MMM/yyyy HH:mm", CultureInfo.GetCultureInfo("es-CO")))
                        .FontSize(7).FontColor(Colors.Grey.Darken2).AlignRight();
                    c2.Item().Text("Generado por el sistema ERP").FontSize(7).FontColor(Colors.Grey.Darken2).AlignRight();
                });
            });

            if (reportType.ContainsPersonalData && !string.IsNullOrEmpty(template?.ConfidentialityNote))
            {
                col.Item().PaddingTop(4);
                col.Item().Text(template.ConfidentialityNote).FontSize(7).Italic().FontColor(Colors.Red.Darken2);
            }

            if (!string.IsNullOrEmpty(template?.DisclaimerNote))
            {
                col.Item().PaddingTop(2);
                col.Item().Text(template.DisclaimerNote).FontSize(7).Italic().FontColor(Colors.Grey.Darken2);
            }
        });
    }

    private void RenderBalanceGeneral(ColumnDescriptor col, string tenantId)
    {
        col.Item().Text("Balance General").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var linesQuery = _context.EntryLines
            .Where(l => l.AccountingEntry!.TenantId == tenantId && l.AccountingEntry.Status == EntryStatus.Final)
            .AsQueryable();

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

        var categoryTotals = new Dictionary<AccountingAccountCategory, (decimal Debit, decimal Credit)>
        {
            { AccountingAccountCategory.Asset, (0, 0) },
            { AccountingAccountCategory.Liability, (0, 0) },
            { AccountingAccountCategory.Equity, (0, 0) }
        };

        foreach (var g in grouped)
        {
            if (!accounts.TryGetValue(g.AccountId, out var account)) continue;

            if (categoryTotals.ContainsKey(account.Category))
            {
                var current = categoryTotals[account.Category];
                categoryTotals[account.Category] = (current.Debit + g.TotalDebit, current.Credit + g.TotalCredit);
            }
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.RelativeColumn();
                cd.ConstantColumn(100);
                cd.ConstantColumn(100);
                cd.ConstantColumn(100);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Categoria").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Debito").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Credito").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Saldo").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            decimal totalAssets = 0;
            decimal totalLiabilities = 0;
            decimal totalEquity = 0;

            foreach (var kvp in categoryTotals)
            {
                var cat = kvp.Key;
                var (debit, credit) = kvp.Value;
                var balance = cat == AccountingAccountCategory.Asset
                    ? debit - credit
                    : credit - debit;

                var label = cat switch
                {
                    AccountingAccountCategory.Asset => "Activo",
                    AccountingAccountCategory.Liability => "Pasivo",
                    AccountingAccountCategory.Equity => "Patrimonio",
                    _ => cat.ToString()
                };

                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text(label).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(debit.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(credit.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(balance.ToString("N2")).Style(rowStyle);

                if (cat == AccountingAccountCategory.Asset) totalAssets = balance;
                if (cat == AccountingAccountCategory.Liability) totalLiabilities = balance;
                if (cat == AccountingAccountCategory.Equity) totalEquity = balance;

                index++;
            }

            var totalStyle = TextStyle.Default.FontSize(8).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("TOTAL ACTIVO").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalAssets.ToString("N2")).Style(totalStyle);

            var totalPasivoPatrimonio = totalLiabilities + totalEquity;
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("TOTAL PASIVO + PATRIMONIO").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalPasivoPatrimonio.ToString("N2")).Style(totalStyle);
        });
    }

    private void RenderIncomeExpense(ColumnDescriptor col, string tenantId)
    {
        col.Item().Text("Estado de Ingresos y Gastos").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var linesQuery = _context.EntryLines
            .Where(l => l.AccountingEntry!.TenantId == tenantId && l.AccountingEntry.Status == EntryStatus.Final)
            .AsQueryable();

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

        var incomeTotalDebit = 0m;
        var incomeTotalCredit = 0m;
        var expenseTotalDebit = 0m;
        var expenseTotalCredit = 0m;

        foreach (var g in grouped)
        {
            if (!accounts.TryGetValue(g.AccountId, out var account)) continue;

            if (account.Category == AccountingAccountCategory.Income)
            {
                incomeTotalDebit += g.TotalDebit;
                incomeTotalCredit += g.TotalCredit;
            }
            else if (account.Category == AccountingAccountCategory.Expense)
            {
                expenseTotalDebit += g.TotalDebit;
                expenseTotalCredit += g.TotalCredit;
            }
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.RelativeColumn();
                cd.ConstantColumn(100);
                cd.ConstantColumn(100);
                cd.ConstantColumn(100);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Concepto").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Debito").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Credito").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Saldo").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var totalStyle = TextStyle.Default.FontSize(8).Bold();

            var incomeBalance = incomeTotalCredit - incomeTotalDebit;
            var expenseBalance = expenseTotalDebit - expenseTotalCredit;
            var netResult = incomeBalance - expenseBalance;

            table.Cell().Padding(3).Text("INGRESOS").Style(totalStyle);
            table.Cell().Padding(3).AlignRight().Text(incomeTotalDebit.ToString("N2")).Style(rowStyle);
            table.Cell().Padding(3).AlignRight().Text(incomeTotalCredit.ToString("N2")).Style(rowStyle);
            table.Cell().Padding(3).AlignRight().Text(incomeBalance.ToString("N2")).Style(rowStyle);

            table.Cell().Padding(3).Text("GASTOS").Style(totalStyle);
            table.Cell().Padding(3).AlignRight().Text(expenseTotalDebit.ToString("N2")).Style(rowStyle);
            table.Cell().Padding(3).AlignRight().Text(expenseTotalCredit.ToString("N2")).Style(rowStyle);
            table.Cell().Padding(3).AlignRight().Text(expenseBalance.ToString("N2")).Style(rowStyle);

            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("RESULTADO NETO").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(netResult.ToString("N2")).Style(totalStyle);
        });
    }

    private void RenderTrialBalance(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Balance de Prueba").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

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

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(80);
                cd.RelativeColumn();
                cd.ConstantColumn(90);
                cd.ConstantColumn(90);
                cd.ConstantColumn(90);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Codigo").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Cuenta").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Debito").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Credito").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Saldo").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            decimal totalDebit = 0, totalCredit = 0;

            foreach (var g in grouped)
            {
                if (!accounts.TryGetValue(g.AccountId, out var account)) continue;

                var balance = account.Nature == AccountingAccountNature.Debit
                    ? g.TotalDebit - g.TotalCredit
                    : g.TotalCredit - g.TotalDebit;

                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text(account.Code).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(account.Name).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(g.TotalDebit.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(g.TotalCredit.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(balance.ToString("N2")).Style(rowStyle);

                totalDebit += g.TotalDebit;
                totalCredit += g.TotalCredit;
                index++;
            }

            var totalStyle = TextStyle.Default.FontSize(8).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("TOTALES").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalDebit.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalCredit.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(Math.Abs(totalDebit - totalCredit).ToString("N2")).Style(totalStyle);
        });
    }

    private void RenderBudgetExecution(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Ejecucion Presupuestal").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var fiscalYear = from?.Year ?? DateTime.UtcNow.Year;

        var budgets = _context.Budgets
            .Where(b => b.TenantId == tenantId && b.FiscalPeriod == fiscalYear)
            .Include(b => b.BudgetDetails)
                .ThenInclude(bd => bd.AccountingAccount)
            .ToList();

        if (budgets.Count == 0)
        {
            col.Item().Padding(10).Text("No se encontraron presupuestos para el periodo fiscal " + fiscalYear + ".").FontColor(Colors.Grey.Darken2);
            return;
        }

        var accountIds = budgets
            .SelectMany(b => b.BudgetDetails)
            .Where(bd => bd.AccountingAccount != null)
            .Select(bd => bd.AccountingAccountId)
            .Distinct()
            .ToList();

        var executedByAccount = new Dictionary<Guid, decimal>();
        if (accountIds.Count > 0)
        {
            var entryLines = _context.EntryLines
                .Where(l => l.AccountingEntry!.TenantId == tenantId && l.AccountingEntry.Status == EntryStatus.Final)
                .Where(l => accountIds.Contains(l.AccountingAccountId))
                .AsQueryable();

            if (from.HasValue)
                entryLines = entryLines.Where(l => l.AccountingEntry!.EntryDate >= from.Value);
            if (to.HasValue)
                entryLines = entryLines.Where(l => l.AccountingEntry!.EntryDate <= to.Value);

            var executed = entryLines
                .GroupBy(l => l.AccountingAccountId)
                .Select(g => new
                {
                    AccountId = g.Key,
                    TotalDebit = g.Sum(l => l.Debit),
                    TotalCredit = g.Sum(l => l.Credit)
                })
                .ToList();

            foreach (var e in executed)
            {
                var acct = _context.AccountingAccounts.Find(e.AccountId);
                if (acct is not null)
                {
                    var executedAmount = acct.Nature == AccountingAccountNature.Debit
                        ? e.TotalDebit - e.TotalCredit
                        : e.TotalCredit - e.TotalDebit;
                    executedByAccount[e.AccountId] = executedAmount;
                }
            }
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(80);
                cd.RelativeColumn();
                cd.ConstantColumn(90);
                cd.ConstantColumn(90);
                cd.ConstantColumn(90);
                cd.ConstantColumn(90);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Codigo").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Cuenta").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Aprobado").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Ejecutado").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Disponible").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("% Ejec.").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            decimal totalApproved = 0;
            decimal totalExecuted = 0;

            foreach (var budget in budgets)
            {
                foreach (var detail in budget.BudgetDetails)
                {
                    if (detail.AccountingAccount is null) continue;

                    executedByAccount.TryGetValue(detail.AccountingAccountId, out var executedAmount);
                    var available = detail.ApprovedValue - executedAmount;
                    decimal percentage = detail.ApprovedValue > 0
                        ? (executedAmount / detail.ApprovedValue) * 100
                        : 0;

                    var bg = index % 2 == 0 ? Colors.White : altColor;
                    table.Cell().Background(bg).Padding(3).Text(detail.AccountingAccount.Code).Style(rowStyle);
                    table.Cell().Background(bg).Padding(3).Text(detail.AccountingAccount.Name).Style(rowStyle);
                    table.Cell().Background(bg).Padding(3).AlignRight().Text(detail.ApprovedValue.ToString("N2")).Style(rowStyle);
                    table.Cell().Background(bg).Padding(3).AlignRight().Text(executedAmount.ToString("N2")).Style(rowStyle);
                    table.Cell().Background(bg).Padding(3).AlignRight().Text(available.ToString("N2")).Style(rowStyle);
                    table.Cell().Background(bg).Padding(3).AlignRight().Text(percentage.ToString("N2") + "%").Style(rowStyle);

                    totalApproved += detail.ApprovedValue;
                    totalExecuted += executedAmount;
                    index++;
                }
            }

            var totalStyle = TextStyle.Default.FontSize(8).Bold();
            var totalAvailable = totalApproved - totalExecuted;
            var totalPct = totalApproved > 0 ? (totalExecuted / totalApproved) * 100 : 0;
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("TOTALES").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalApproved.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalExecuted.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalAvailable.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalPct.ToString("N2") + "%").Style(totalStyle);
        });
    }

    private void RenderContingencyFund(ColumnDescriptor col, string tenantId)
    {
        col.Item().Text("Fondo de Imprevistos").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var fund = _context.ContingencyFunds
            .FirstOrDefault(f => f.TenantId == tenantId);

        if (fund is null)
        {
            col.Item().Padding(10).Text("No se ha configurado el Fondo de Imprevistos para este conjunto.").FontColor(Colors.Grey.Darken2);
            return;
        }

        col.Item().Text("Saldo Actual: " + fund.CurrentBalance.ToString("N2")).FontSize(11).Bold();
        col.Item().PaddingBottom(8);

        var contributions = _context.ContingencyFundContributions
            .Where(c => c.TenantId == tenantId)
            .OrderBy(c => c.ContributionDate)
            .ToList();

        var usages = _context.ContingencyFundUsages
            .Where(u => u.TenantId == tenantId)
            .OrderByDescending(u => u.ApprovalDate)
            .ToList();

        col.Item().Text("Historial de Aportes").FontSize(10).Bold();
        col.Item().PaddingBottom(4);

        if (contributions.Count > 0)
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cd =>
                {
                    cd.ConstantColumn(80);
                    cd.RelativeColumn();
                    cd.ConstantColumn(90);
                    cd.ConstantColumn(90);
                });

                var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
                table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Periodo").Style(headerStyle);
                table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Fecha").Style(headerStyle);
                table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Base").Style(headerStyle);
                table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Aporte").Style(headerStyle);

                var rowStyle = TextStyle.Default.FontSize(8);
                var altColor = Colors.Grey.Lighten4;
                var index = 0;
                decimal totalContributions = 0;

                foreach (var c in contributions)
                {
                    var bg = index % 2 == 0 ? Colors.White : altColor;
                    table.Cell().Background(bg).Padding(3).Text(c.Period).Style(rowStyle);
                    table.Cell().Background(bg).Padding(3).Text(c.ContributionDate.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO"))).Style(rowStyle);
                    table.Cell().Background(bg).Padding(3).AlignRight().Text(c.IncomeBase.ToString("N2")).Style(rowStyle);
                    table.Cell().Background(bg).Padding(3).AlignRight().Text(c.Amount.ToString("N2")).Style(rowStyle);
                    totalContributions += c.Amount;
                    index++;
                }

                var totalStyle = TextStyle.Default.FontSize(8).Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("TOTAL APORTES").Style(totalStyle);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalContributions.ToString("N2")).Style(totalStyle);
            });
        }
        else
        {
            col.Item().Text("No hay aportes registrados.").FontSize(8).FontColor(Colors.Grey.Darken2);
        }

        col.Item().PaddingBottom(8);

        col.Item().Text("Historial de Usos").FontSize(10).Bold();
        col.Item().PaddingBottom(4);

        if (usages.Count > 0)
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cd =>
                {
                    cd.ConstantColumn(80);
                    cd.RelativeColumn();
                    cd.ConstantColumn(90);
                });

                var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
                table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Fecha").Style(headerStyle);
                table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Justificacion").Style(headerStyle);
                table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Monto").Style(headerStyle);

                var rowStyle = TextStyle.Default.FontSize(8);
                var altColor = Colors.Grey.Lighten4;
                var index = 0;
                decimal totalUsages = 0;

                foreach (var u in usages)
                {
                    var bg = index % 2 == 0 ? Colors.White : altColor;
                    table.Cell().Background(bg).Padding(3).Text(u.ApprovalDate.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO"))).Style(rowStyle);
                    table.Cell().Background(bg).Padding(3).Text(u.Justification.Length > 60 ? u.Justification[..60] + "..." : u.Justification).Style(rowStyle);
                    table.Cell().Background(bg).Padding(3).AlignRight().Text(u.Amount.ToString("N2")).Style(rowStyle);
                    totalUsages += u.Amount;
                    index++;
                }

                var totalStyle = TextStyle.Default.FontSize(8).Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("TOTAL USOS").Style(totalStyle);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalUsages.ToString("N2")).Style(totalStyle);
            });
        }
        else
        {
            col.Item().Text("No hay usos registrados.").FontSize(8).FontColor(Colors.Grey.Darken2);
        }
    }

    private void RenderCashFlow(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Flujo de Caja").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

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

        var categoryTotals = new Dictionary<AccountingAccountCategory, (decimal Inflow, decimal Outflow)>();

        foreach (var g in grouped)
        {
            if (!accounts.TryGetValue(g.AccountId, out var account)) continue;

            if (!categoryTotals.ContainsKey(account.Category))
            {
                categoryTotals[account.Category] = (0, 0);
            }

            var current = categoryTotals[account.Category];
            categoryTotals[account.Category] = (current.Inflow + g.TotalDebit, current.Outflow + g.TotalCredit);
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.RelativeColumn();
                cd.ConstantColumn(100);
                cd.ConstantColumn(100);
                cd.ConstantColumn(100);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Categoria").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Entradas").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Salidas").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Neto").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            decimal totalInflow = 0;
            decimal totalOutflow = 0;

            foreach (var kvp in categoryTotals)
            {
                var (inflow, outflow) = kvp.Value;
                var net = inflow - outflow;

                var label = kvp.Key switch
                {
                    AccountingAccountCategory.Asset => "Activo",
                    AccountingAccountCategory.Liability => "Pasivo",
                    AccountingAccountCategory.Equity => "Patrimonio",
                    AccountingAccountCategory.Income => "Ingreso",
                    AccountingAccountCategory.Expense => "Gasto",
                    _ => kvp.Key.ToString()
                };

                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text(label).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(inflow.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(outflow.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(net.ToString("N2")).Style(rowStyle);

                totalInflow += inflow;
                totalOutflow += outflow;
                index++;
            }

            var totalStyle = TextStyle.Default.FontSize(8).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("TOTALES").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalInflow.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalOutflow.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text((totalInflow - totalOutflow).ToString("N2")).Style(totalStyle);
        });
    }

    private void RenderGeneralLedger(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Libro Mayor y Diario").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

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
                EntryDate = l.AccountingEntry!.EntryDate,
                EntryNumber = l.AccountingEntry.EntryNumber,
                Description = l.AccountingEntry.Description,
                AccountCode = l.AccountingAccount!.Code,
                AccountName = l.AccountingAccount.Name,
                AccountNature = l.AccountingAccount.Nature,
                l.Debit,
                l.Credit
            })
            .ToList();

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(60);
                cd.ConstantColumn(50);
                cd.RelativeColumn();
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Fecha").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("No.").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Descripcion / Cuenta").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Debito").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Credito").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            decimal totalDebit = 0;
            decimal totalCredit = 0;

            foreach (var line in lines)
            {
                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text(line.EntryDate.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO"))).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(line.EntryNumber.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(line.AccountCode + " - " + line.AccountName).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(line.Debit.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(line.Credit.ToString("N2")).Style(rowStyle);

                totalDebit += line.Debit;
                totalCredit += line.Credit;
                index++;
            }

            var totalStyle = TextStyle.Default.FontSize(8).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("TOTALES").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalDebit.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalCredit.ToString("N2")).Style(totalStyle);
        });
    }

    private void RenderJournalEntryDetail(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Detalle de Asientos Contables").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

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

        var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
        var rowStyle = TextStyle.Default.FontSize(8);
        var altColor = Colors.Grey.Lighten4;
        var totalEntryDebit = 0m;
        var totalEntryCredit = 0m;

        foreach (var entry in entries)
        {
            col.Item().PaddingBottom(4);

            col.Item().Row(row =>
            {
                row.RelativeItem().Text("Asiento No. " + entry.EntryNumber).FontSize(10).Bold();
                row.ConstantItem(120).AlignRight().Text(entry.EntryDate.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO"))).FontSize(9).FontColor(Colors.Grey.Darken2);
            });

            col.Item().Text(entry.Description).FontSize(8).FontColor(Colors.Grey.Darken2);
            col.Item().PaddingBottom(4);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(cd =>
                {
                    cd.ConstantColumn(80);
                    cd.RelativeColumn();
                    cd.ConstantColumn(80);
                    cd.ConstantColumn(80);
                });

                table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Codigo").Style(headerStyle);
                table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Cuenta").Style(headerStyle);
                table.Cell().Background(Colors.Grey.Darken3).Padding(3).AlignRight().Text("Debito").Style(headerStyle);
                table.Cell().Background(Colors.Grey.Darken3).Padding(3).AlignRight().Text("Credito").Style(headerStyle);

                var lineIndex = 0;
                foreach (var line in entry.Lines)
                {
                    var bg = lineIndex % 2 == 0 ? Colors.White : altColor;
                    table.Cell().Background(bg).Padding(2).Text(line.AccountingAccount?.Code ?? "").Style(rowStyle);
                    table.Cell().Background(bg).Padding(2).Text(line.AccountingAccount?.Name ?? "").Style(rowStyle);
                    table.Cell().Background(bg).Padding(2).AlignRight().Text(line.Debit.ToString("N2")).Style(rowStyle);
                    table.Cell().Background(bg).Padding(2).AlignRight().Text(line.Credit.ToString("N2")).Style(rowStyle);
                    lineIndex++;
                }

                var totalStyle = TextStyle.Default.FontSize(8).Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(totalStyle);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("Totales").Style(totalStyle);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(entry.TotalDebit.ToString("N2")).Style(totalStyle);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(entry.TotalCredit.ToString("N2")).Style(totalStyle);
            });

            totalEntryDebit += entry.TotalDebit;
            totalEntryCredit += entry.TotalCredit;
        }

        col.Item().PaddingTop(6);
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(80);
                cd.RelativeColumn();
                cd.ConstantColumn(80);
                cd.ConstantColumn(80);
            });

            var grandTotalStyle = TextStyle.Default.FontSize(9).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("GRAN TOTAL").Style(grandTotalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("").Style(grandTotalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(totalEntryDebit.ToString("N2")).Style(grandTotalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(totalEntryCredit.ToString("N2")).Style(grandTotalStyle);
        });
    }

    private void RenderPortfolioAging(ColumnDescriptor col, string tenantId)
    {
        col.Item().Text("Cartera por Antiguedad").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var today = DateTime.UtcNow.Date;

        var fees = _context.UnitFees
            .Where(f => f.TenantId == tenantId && f.Status != FeeStatus.FullyPaid)
            .Include(f => f.Unit)
            .ToList();

        var buckets = new Dictionary<string, (int Count, decimal Balance)>
        {
            { "0 - 30 dias", (0, 0) },
            { "31 - 60 dias", (0, 0) },
            { "61 - 90 dias", (0, 0) },
            { "Mas de 90 dias", (0, 0) }
        };

        foreach (var fee in fees)
        {
            var daysOverdue = (today - fee.DueDate).Days;
            if (daysOverdue < 0) daysOverdue = 0;

            var bucket = daysOverdue switch
            {
                <= 30 => "0 - 30 dias",
                <= 60 => "31 - 60 dias",
                <= 90 => "61 - 90 dias",
                _ => "Mas de 90 dias"
            };

            var current = buckets[bucket];
            buckets[bucket] = (current.Count + 1, current.Balance + fee.BalanceAmount);
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.RelativeColumn();
                cd.ConstantColumn(80);
                cd.ConstantColumn(100);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Rango").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Cantidad").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Saldo Total").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            var totalCount = 0;
            var totalBalance = 0m;

            foreach (var kvp in buckets)
            {
                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text(kvp.Key).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(kvp.Value.Count.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(kvp.Value.Balance.ToString("N2")).Style(rowStyle);

                totalCount += kvp.Value.Count;
                totalBalance += kvp.Value.Balance;
                index++;
            }

            var totalStyle = TextStyle.Default.FontSize(8).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("TOTALES").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalCount.ToString()).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalBalance.ToString("N2")).Style(totalStyle);
        });
    }

    private void RenderPortfolioByUnit(ColumnDescriptor col, string tenantId)
    {
        col.Item().Text("Cartera por Unidad").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var fees = _context.UnitFees
            .Where(f => f.TenantId == tenantId && f.Status != FeeStatus.FullyPaid)
            .Include(f => f.Unit)
            .OrderBy(f => f.Unit!.Identifier)
            .ThenBy(f => f.DueDate)
            .ToList();

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(60);
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Unidad").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Vencimiento").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Cuota").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Pagado").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Saldo").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            var totalFee = 0m;
            var totalPaid = 0m;
            var totalBalance = 0m;

            foreach (var fee in fees)
            {
                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text(fee.Unit?.Identifier ?? "").Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(fee.DueDate.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO"))).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(fee.FeeValue.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(fee.PaidAmount.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(fee.BalanceAmount.ToString("N2")).Style(rowStyle);

                totalFee += fee.FeeValue;
                totalPaid += fee.PaidAmount;
                totalBalance += fee.BalanceAmount;
                index++;
            }

            var totalStyle = TextStyle.Default.FontSize(8).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("TOTALES").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalFee.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalPaid.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalBalance.ToString("N2")).Style(totalStyle);
        });
    }

    private void RenderTopDebtors(ColumnDescriptor col, string tenantId)
    {
        col.Item().Text("Principales Deudores").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var debtorGroups = _context.UnitFees
            .Where(f => f.TenantId == tenantId && f.Status != FeeStatus.FullyPaid)
            .GroupBy(f => f.UnitId)
            .Select(g => new
            {
                UnitId = g.Key,
                TotalBalance = g.Sum(f => f.BalanceAmount),
                FeeCount = g.Count()
            })
            .OrderByDescending(g => g.TotalBalance)
            .Take(20)
            .ToList();

        var unitIds = debtorGroups.Select(d => d.UnitId).ToList();
        var units = _context.Units
            .Where(u => unitIds.Contains(u.Id))
            .ToDictionary(u => u.Id);

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(60);
                cd.RelativeColumn();
                cd.ConstantColumn(70);
                cd.ConstantColumn(90);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("#").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Unidad").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Cuotas").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Saldo Total").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var rank = 1;
            decimal grandTotal = 0;

            foreach (var debtor in debtorGroups)
            {
                units.TryGetValue(debtor.UnitId, out var unit);
                var bg = rank % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text(rank.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(unit?.Identifier ?? "").Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(debtor.FeeCount.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(debtor.TotalBalance.ToString("N2")).Style(rowStyle);
                grandTotal += debtor.TotalBalance;
                rank++;
            }

            var totalStyle = TextStyle.Default.FontSize(8).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("TOTAL").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(grandTotal.ToString("N2")).Style(totalStyle);
        });
    }

    private void RenderPeriodCollection(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Recaudo del Periodo").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

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

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(60);
                cd.ConstantColumn(60);
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Fecha").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Unidad").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Monto").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Abono").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Metodo").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            decimal totalAmount = 0;
            decimal totalAdvance = 0;

            foreach (var payment in payments)
            {
                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text(payment.PaymentDate.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO"))).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(payment.Unit?.Identifier ?? "").Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(payment.Amount.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(payment.AdvanceAmount.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(payment.PaymentMethod.ToString()).Style(rowStyle);

                totalAmount += payment.Amount;
                totalAdvance += payment.AdvanceAmount;
                index++;
            }

            var totalStyle = TextStyle.Default.FontSize(8).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("TOTALES").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalAmount.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalAdvance.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
        });
    }

    private void RenderPortfolioProjection(ColumnDescriptor col, string tenantId)
    {
        col.Item().Text("Proyeccion de Cartera").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var pendingFees = _context.UnitFees
            .Where(f => f.TenantId == tenantId && f.Status != FeeStatus.FullyPaid)
            .ToList();

        var totalPending = pendingFees.Sum(f => f.BalanceAmount);
        var feeCount = pendingFees.Count;

        var paidFeesInLast90Days = _context.UnitFees
            .Where(f => f.TenantId == tenantId && f.Status == FeeStatus.FullyPaid && f.UpdatedAt >= DateTime.UtcNow.AddDays(-90))
            .ToList();

        var totalCollected = paidFeesInLast90Days.Sum(f => f.PaidAmount);
        var historicalCollectionRate = totalCollected > 0 && (totalCollected + totalPending) > 0
            ? totalCollected / (totalCollected + totalPending)
            : 0.7m;

        var estimatedRecovery = totalPending * historicalCollectionRate;
        var estimatedUnrecoverable = totalPending - estimatedRecovery;

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.RelativeColumn();
                cd.ConstantColumn(100);
            });

            var rowStyle = TextStyle.Default.FontSize(8);
            var totalStyle = TextStyle.Default.FontSize(8).Bold();

            table.Cell().Padding(3).Text("Total cuotas pendientes").Style(rowStyle);
            table.Cell().Padding(3).AlignRight().Text(feeCount.ToString()).Style(rowStyle);

            table.Cell().Padding(3).Text("Saldo total por cobrar").Style(rowStyle);
            table.Cell().Padding(3).AlignRight().Text(totalPending.ToString("N2")).Style(rowStyle);

            table.Cell().Padding(3).Text("Tasa de recaudo historica estimada").Style(rowStyle);
            table.Cell().Padding(3).AlignRight().Text((historicalCollectionRate * 100).ToString("N2") + "%").Style(rowStyle);

            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Recaudo estimado").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(estimatedRecovery.ToString("N2")).Style(totalStyle);

            table.Cell().Padding(3).Text("Saldo de dificil recuperacion").Style(rowStyle);
            table.Cell().Padding(3).AlignRight().Text(estimatedUnrecoverable.ToString("N2")).Style(rowStyle);
        });
    }

    private void RenderPaymentAgreements(ColumnDescriptor col, string tenantId)
    {
        col.Item().Text("Acuerdos de Pago Activos").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var agreements = _context.PaymentAgreements
            .Where(a => a.TenantId == tenantId && a.Status == AgreementStatus.Active)
            .Include(a => a.Unit)
            .Include(a => a.Installments)
            .ToList();

        if (agreements.Count == 0)
        {
            col.Item().Padding(10).Text("No hay acuerdos de pago activos.").FontColor(Colors.Grey.Darken2);
            return;
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(60);
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
                cd.ConstantColumn(60);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Unidad").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Deuda").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Cuota").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("No.Cuotas").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Inicio").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Estado").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;

            foreach (var agreement in agreements)
            {
                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text(agreement.Unit?.Identifier ?? "").Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(agreement.TotalDebtIncluded.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(agreement.InstallmentAmount.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(agreement.NumberOfInstallments.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(agreement.StartedAt.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO"))).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(agreement.Status.ToString()).Style(rowStyle);
                index++;
            }
        });
    }

    private void RenderPQRSummary(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Resumen de PQR").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var pqrQuery = _context.PqrRecords
            .Where(p => p.TenantId == tenantId)
            .AsQueryable();

        if (from.HasValue)
            pqrQuery = pqrQuery.Where(p => p.FiledAt >= from.Value);
        if (to.HasValue)
            pqrQuery = pqrQuery.Where(p => p.FiledAt <= to.Value);

        var allRecords = pqrQuery.ToList();

        var byType = allRecords.GroupBy(p => p.PQRType)
            .Select(g => new { Key = g.Key.ToString(), Count = g.Count() })
            .ToList();

        var byCategory = allRecords.GroupBy(p => p.Category)
            .Select(g => new { Key = g.Key.ToString(), Count = g.Count() })
            .ToList();

        var byStatus = allRecords.GroupBy(p => p.Status)
            .Select(g => new { Key = g.Key.ToString(), Count = g.Count() })
            .ToList();

        col.Item().Text("Por Tipo").FontSize(10).Bold();
        col.Item().PaddingBottom(4);

        RenderCountTable(col, byType.Cast<dynamic>().ToList());

        col.Item().PaddingBottom(8);

        col.Item().Text("Por Categoria").FontSize(10).Bold();
        col.Item().PaddingBottom(4);

        RenderCountTable(col, byCategory.Cast<dynamic>().ToList());

        col.Item().PaddingBottom(8);

        col.Item().Text("Por Estado").FontSize(10).Bold();
        col.Item().PaddingBottom(4);

        RenderCountTable(col, byStatus.Cast<dynamic>().ToList());
    }

    private void RenderCountTable(ColumnDescriptor col, List<dynamic> data)
    {
        if (data.Count == 0)
        {
            col.Item().Text("No hay datos.").FontSize(8).FontColor(Colors.Grey.Darken2);
            return;
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.RelativeColumn();
                cd.ConstantColumn(80);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Item").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Cantidad").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            var total = 0;

            foreach (var item in data)
            {
                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text((string)item.Key).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(((int)item.Count).ToString()).Style(rowStyle);
                total += (int)item.Count;
                index++;
            }

            var totalStyle = TextStyle.Default.FontSize(8).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("TOTAL").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(total.ToString()).Style(totalStyle);
        });
    }

    private void RenderCommonAreaUsage(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Uso de Zonas Comunes").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var reservationsQuery = _context.Reservations
            .Where(r => r.TenantId == tenantId)
            .Where(r => r.Status == ReservationStatus.Approved || r.Status == ReservationStatus.Completed || r.Status == ReservationStatus.InUse)
            .Include(r => r.Space)
            .Include(r => r.Unit)
            .AsQueryable();

        if (from.HasValue)
            reservationsQuery = reservationsQuery.Where(r => r.StartDateTime >= from.Value);
        if (to.HasValue)
            reservationsQuery = reservationsQuery.Where(r => r.EndDateTime <= to.Value);

        var reservations = reservationsQuery
            .OrderBy(r => r.StartDateTime)
            .ToList();

        var bySpace = reservations
            .GroupBy(r => r.Space)
            .Select(g => new
            {
                SpaceName = g.Key?.Name ?? "Sin espacio",
                Count = g.Count(),
                TotalHours = g.Sum(r => (r.EndDateTime - r.StartDateTime).TotalHours)
            })
            .ToList();

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.RelativeColumn();
                cd.ConstantColumn(60);
                cd.ConstantColumn(80);
                cd.ConstantColumn(80);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Zona").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Reservas").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Horas").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Estado").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;

            foreach (var item in bySpace)
            {
                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text(item.SpaceName).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(item.Count.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(Math.Round(item.TotalHours, 1).ToString("N1")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text("").Style(rowStyle);
                index++;
            }
        });
    }

    private void RenderMaintenanceSummary(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Resumen de Mantenimiento").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var workOrdersQuery = _context.WorkOrders
            .Where(w => w.TenantId == tenantId)
            .Include(w => w.Asset)
            .AsQueryable();

        if (from.HasValue)
            workOrdersQuery = workOrdersQuery.Where(w => w.CreatedAt >= from.Value);
        if (to.HasValue)
            workOrdersQuery = workOrdersQuery.Where(w => w.CreatedAt <= to.Value);

        var workOrders = workOrdersQuery.ToList();

        var byStatus = workOrders.GroupBy(w => w.Status)
            .Select(g => new { Key = g.Key.ToString(), Count = g.Count() })
            .ToList();

        var byType = workOrders.GroupBy(w => w.OrderType)
            .Select(g => new { Key = g.Key.ToString(), Count = g.Count() })
            .ToList();

        col.Item().Text("Por Estado").FontSize(10).Bold();
        col.Item().PaddingBottom(4);

        RenderCountTable(col, byStatus.Select(s => (dynamic)new { Key = s.Key, Count = s.Count }).ToList());

        col.Item().PaddingBottom(8);

        col.Item().Text("Por Tipo").FontSize(10).Bold();
        col.Item().PaddingBottom(4);

        RenderCountTable(col, byType.Select(t => (dynamic)new { Key = t.Key, Count = t.Count }).ToList());
    }

    private void RenderActiveContracts(ColumnDescriptor col, string tenantId)
    {
        col.Item().Text("Contratos Activos").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var contracts = _context.Contracts
            .Where(c => c.TenantId == tenantId && c.Status == ContractStatus.Active)
            .Include(c => c.Provider)
            .ToList();

        if (contracts.Count == 0)
        {
            col.Item().Padding(10).Text("No hay contratos activos.").FontColor(Colors.Grey.Darken2);
            return;
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(60);
                cd.RelativeColumn();
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
                cd.ConstantColumn(60);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("No.").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Proveedor").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Tipo").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Valor Total").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Inicio").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Fin").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            decimal totalValue = 0;

            foreach (var contract in contracts)
            {
                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text(contract.ContractNumber).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(contract.Provider?.BusinessName ?? "").Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(contract.ContractType.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(contract.TotalValue.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(contract.StartDate.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO"))).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(contract.EndDate.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO"))).Style(rowStyle);

                totalValue += contract.TotalValue;
                index++;
            }

            var totalStyle = TextStyle.Default.FontSize(8).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("TOTALES").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text(totalValue.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("").Style(totalStyle);
        });
    }

    private void RenderCommunicationSummary(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Resumen de Comunicados").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var commsQuery = _context.Communications
            .Where(c => c.TenantId == tenantId)
            .AsQueryable();

        if (from.HasValue)
            commsQuery = commsQuery.Where(c => c.CreatedAt >= from.Value);
        if (to.HasValue)
            commsQuery = commsQuery.Where(c => c.CreatedAt <= to.Value);

        var communications = commsQuery.ToList();

        var byStatus = communications.GroupBy(c => c.Status)
            .Select(g => new { Key = g.Key.ToString(), Count = g.Count() })
            .ToList();

        var byAudience = communications.GroupBy(c => c.AudienceType)
            .Select(g => new { Key = g.Key.ToString(), Count = g.Count() })
            .ToList();

        col.Item().Text("Por Estado").FontSize(10).Bold();
        col.Item().PaddingBottom(4);

        RenderCountTable(col, byStatus.Select(s => (dynamic)new { Key = s.Key, Count = s.Count }).ToList());

        col.Item().PaddingBottom(8);

        col.Item().Text("Por Tipo de Audiencia").FontSize(10).Bold();
        col.Item().PaddingBottom(4);

        RenderCountTable(col, byAudience.Select(a => (dynamic)new { Key = a.Key, Count = a.Count }).ToList());
    }

    private void RenderAssemblyMinutes(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Actas de Asamblea").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var assembliesQuery = _context.Assemblies
            .Where(a => a.TenantId == tenantId)
            .Include(a => a.Minutes)
            .AsQueryable();

        if (from.HasValue)
            assembliesQuery = assembliesQuery.Where(a => a.ScheduledDate >= from.Value);
        if (to.HasValue)
            assembliesQuery = assembliesQuery.Where(a => a.ScheduledDate <= to.Value);

        var assemblies = assembliesQuery
            .OrderByDescending(a => a.ScheduledDate)
            .ToList();

        if (assemblies.Count == 0)
        {
            col.Item().Padding(10).Text("No se encontraron actas de asamblea.").FontColor(Colors.Grey.Darken2);
            return;
        }

        var rowStyle = TextStyle.Default.FontSize(8);
        var altColor = Colors.Grey.Lighten4;
        var minutesIndex = 0;

        foreach (var assembly in assemblies)
        {
            var assemblyTitle = assembly.Title + " (" + assembly.ScheduledDate.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO")) + ")";
            col.Item().Text(assemblyTitle).FontSize(10).Bold();
            col.Item().PaddingBottom(4);

            foreach (var minute in assembly.Minutes)
            {
                var bg = minutesIndex % 2 == 0 ? Colors.White : altColor;

                col.Item().Background(bg).Padding(4).Column(minuteCol =>
                {
                    minuteCol.Item().Text("Estado: " + minute.Status.ToString()).FontSize(8).FontColor(Colors.Grey.Darken2);
                    minuteCol.Item().Text("Generado: " + minute.GeneratedAt.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO"))).FontSize(8).FontColor(Colors.Grey.Darken2);

                    if (!string.IsNullOrEmpty(minute.PresidentName))
                        minuteCol.Item().Text("Presidente: " + minute.PresidentName).FontSize(8).FontColor(Colors.Grey.Darken2);
                    if (!string.IsNullOrEmpty(minute.SecretaryName))
                        minuteCol.Item().Text("Secretario: " + minute.SecretaryName).FontSize(8).FontColor(Colors.Grey.Darken2);

                    var textPreview = minute.FullText.Length > 200 ? minute.FullText[..200] + "..." : minute.FullText;
                    minuteCol.Item().PaddingTop(4).Text(textPreview).FontSize(8).Italic();
                });

                col.Item().PaddingBottom(6);
                minutesIndex++;
            }
        }
    }

    private void RenderAssemblyDecisions(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Decisiones de Asamblea").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var agendaItemsQuery = _context.AssemblyAgendaItems
            .Where(ai => ai.TenantId == tenantId && ai.RequiresVoting)
            .Include(ai => ai.Assembly)
            .AsQueryable();

        if (from.HasValue)
            agendaItemsQuery = agendaItemsQuery.Where(ai => ai.Assembly!.ScheduledDate >= from.Value);
        if (to.HasValue)
            agendaItemsQuery = agendaItemsQuery.Where(ai => ai.Assembly!.ScheduledDate <= to.Value);

        var items = agendaItemsQuery
            .OrderByDescending(ai => ai.Assembly!.ScheduledDate)
            .ThenBy(ai => ai.SequenceNumber)
            .ToList();

        if (items.Count == 0)
        {
            col.Item().Padding(10).Text("No se encontraron decisiones registradas.").FontColor(Colors.Grey.Darken2);
            return;
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(70);
                cd.RelativeColumn();
                cd.ConstantColumn(60);
                cd.ConstantColumn(60);
                cd.ConstantColumn(60);
                cd.ConstantColumn(60);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Asamblea").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Tema").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Favor").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("En Contra").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Abst.").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Resultado").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;

            foreach (var item in items)
            {
                var bg = index % 2 == 0 ? Colors.White : altColor;
                var assemblyLabel = item.Assembly?.Title ?? "";
                assemblyLabel = assemblyLabel.Length > 15 ? assemblyLabel[..15] + "..." : assemblyLabel;

                var result = item.IsApproved switch
                {
                    true => "Aprobado",
                    false => "Rechazado",
                    null => "Pendiente"
                };

                table.Cell().Background(bg).Padding(3).Text(assemblyLabel).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(item.Title.Length > 25 ? item.Title[..25] + "..." : item.Title).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(item.VotesInFavorCoefficients.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(item.VotesAgainstCoefficients.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(item.AbstentionCoefficients.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(result).Style(rowStyle);

                index++;
            }
        });
    }

    private void RenderCouncilHistory(ColumnDescriptor col, string tenantId)
    {
        col.Item().Text("Historial del Consejo").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var councilRoles = _context.UserTenantRoles
            .Where(r => r.TenantId == tenantId && r.Role == AppRole.Council)
            .Include(r => r.User)
            .OrderByDescending(r => r.AssignedAt)
            .ToList();

        if (councilRoles.Count == 0)
        {
            col.Item().Padding(10).Text("No hay registros de miembros del consejo.").FontColor(Colors.Grey.Darken2);
            return;
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.RelativeColumn();
                cd.ConstantColumn(70);
                cd.ConstantColumn(80);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Miembro").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Rol").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Asignado").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;

            foreach (var role in councilRoles)
            {
                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text(role.User?.FullName ?? role.UserId).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(role.Role.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(role.AssignedAt.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO"))).Style(rowStyle);
                index++;
            }
        });
    }

    private void RenderAssemblyQuorum(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Quorum y Participacion").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var assembliesQuery = _context.Assemblies
            .Where(a => a.TenantId == tenantId)
            .Include(a => a.Attendances)
            .AsQueryable();

        if (from.HasValue)
            assembliesQuery = assembliesQuery.Where(a => a.ScheduledDate >= from.Value);
        if (to.HasValue)
            assembliesQuery = assembliesQuery.Where(a => a.ScheduledDate <= to.Value);

        var assemblies = assembliesQuery
            .OrderByDescending(a => a.ScheduledDate)
            .ToList();

        if (assemblies.Count == 0)
        {
            col.Item().Padding(10).Text("No se encontraron asambleas en el periodo.").FontColor(Colors.Grey.Darken2);
            return;
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(60);
                cd.ConstantColumn(60);
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
                cd.ConstantColumn(60);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Fecha").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Tipo").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Coef.Total").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("Coef.Presente").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).AlignRight().Text("% Quorum").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Logrado").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;

            foreach (var assembly in assemblies)
            {
                var totalPresentCoefficients = assembly.Attendances
                    .Where(a => a.Status == AttendanceStatus.Present)
                    .Sum(a => a.Coefficient);

                var quorumThreshold = assembly.ConvocationNumber == 1
                    ? assembly.QuorumThresholdFirstCall
                    : assembly.QuorumThresholdSecondCall;

                var quorumPct = assembly.TotalCoefficients > 0
                    ? (totalPresentCoefficients / assembly.TotalCoefficients) * 100
                    : 0;

                var quorumAchieved = assembly.ConvocationNumber == 1
                    ? assembly.QuorumAchievedFirstCall
                    : assembly.QuorumAchievedSecondCall;

                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text(assembly.ScheduledDate.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO"))).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(assembly.Type.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(assembly.TotalCoefficients.ToString("N4")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(totalPresentCoefficients.ToString("N4")).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).AlignRight().Text(quorumPct.ToString("N2") + "%").Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(quorumAchieved ? "Si" : "No").Style(rowStyle);

                index++;
            }
        });
    }

    private void RenderOwnerRegistry(ColumnDescriptor col, string tenantId)
    {
        col.Item().Text("Padron de Propietarios").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var unitOwners = _context.UnitOwners
            .Where(uo => uo.TenantId == tenantId && uo.IsActive)
            .Include(uo => uo.Owner)
            .Include(uo => uo.Unit)
            .OrderBy(uo => uo.Unit!.Identifier)
            .ThenBy(uo => uo.Owner!.FullNameOrCompanyName)
            .ToList();

        if (unitOwners.Count == 0)
        {
            col.Item().Padding(10).Text("No hay propietarios registrados.").FontColor(Colors.Grey.Darken2);
            return;
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(50);
                cd.RelativeColumn();
                cd.ConstantColumn(50);
                cd.ConstantColumn(70);
                cd.ConstantColumn(80);
                cd.ConstantColumn(70);
            });

            var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Unidad").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Nombre").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Tipo Doc.").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Documento").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Email").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(4).Text("Telefono").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(8);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;

            foreach (var uo in unitOwners)
            {
                if (uo.Owner is null) continue;

                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(3).Text(uo.Unit?.Identifier ?? "").Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(uo.Owner.FullNameOrCompanyName).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(uo.Owner.DocumentType.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(uo.Owner.DocumentNumber).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(uo.Owner.Email).Style(rowStyle);
                table.Cell().Background(bg).Padding(3).Text(uo.Owner.MainPhone).Style(rowStyle);
                index++;
            }
        });
    }

    private static string BuildPeriodLabel(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue)
            return from.Value.ToString("yyyyMMdd") + "_" + to.Value.ToString("yyyyMMdd");
        if (from.HasValue)
            return from.Value.ToString("yyyyMMdd");
        return DateTime.UtcNow.ToString("yyyyMMdd");
    }

    private static Color ParseColor(string hex)
    {
        if (hex.StartsWith("#") && hex.Length == 7)
        {
            var r = byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);
            return Color.FromRGB(r, g, b);
        }
        return Colors.Green.Medium;
    }
}
