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

namespace Softcoinp.ERP.WebAPI.Services;

public class PDFGenerationEngine
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    private readonly PortfolioAgingService _portfolioAgingService;

    public PDFGenerationEngine(ApplicationDbContext context, IWebHostEnvironment env, PortfolioAgingService portfolioAgingService)
    {
        _context = context;
        _env = env;
        _portfolioAgingService = portfolioAgingService;
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
            .Where(t => t.TenantId == tenantId && t.IsGlobal)
            .FirstOrDefaultAsync();

        var tenantConfig = await _context.TenantConfigurations
            .FirstOrDefaultAsync(tc => tc.TenantId == tenantId);

        var periodLabel = BuildPeriodLabel(periodFrom, periodTo);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var dir = Path.Combine(_env.WebRootPath ?? "wwwroot", "reports", tenantId, reportTypeCode);
        Directory.CreateDirectory(dir);

        var consecutive = await GetNextConsecutiveNumber(tenantId, reportType.Id);
        var fileName = $"{reportTypeCode}_{periodLabel}_{consecutive:D4}_{timestamp}.pdf";
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
                page.Footer().Element(f => ComposeFooter(f, template, tenantConfig, reportType, userId, consecutive));
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
            RecurringConfigId = recurringConfigId,
            ConsecutiveNumber = consecutive
        };

        _context.GeneratedReports.Add(generated);
        await _context.SaveChangesAsync();
        return generated;
    }

    private async Task<int> GetNextConsecutiveNumber(string tenantId, Guid reportTypeId)
    {
        var lastNumber = await _context.GeneratedReports
            .Where(r => r.TenantId == tenantId && r.ReportTypeId == reportTypeId)
            .OrderByDescending(r => r.ConsecutiveNumber)
            .Select(r => (int?)r.ConsecutiveNumber)
            .FirstOrDefaultAsync();

        return (lastNumber ?? 0) + 1;
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
                case "PortfolioReport":
                    RenderPortfolioReport(col, tenantId);
                    break;
                case "CollectionReport":
                    RenderCollectionReport(col, tenantId, periodFrom, periodTo);
                    break;
                case "ExpenseReport":
                    RenderExpenseReport(col, tenantId, periodFrom, periodTo);
                    break;
                case "BudgetExecution":
                    RenderBudgetExecutionReport(col, tenantId);
                    break;
                case "ActiveContracts":
                    RenderActiveContractsReport(col, tenantId);
                    break;
                case "PQRReport":
                    RenderPQRReport(col, tenantId, periodFrom, periodTo);
                    break;
                case "MaintenanceReport":
                    RenderMaintenanceReport(col, tenantId, periodFrom, periodTo);
                    break;
                case "AssemblyReport":
                    RenderAssemblyReport(col, tenantId, periodFrom, periodTo);
                    break;
                case "AnnualManagementReport":
                    col.Item().Text("Use GenerateAnnualReportPdfAsync para este tipo.").FontColor(Colors.Grey.Darken2);
                    break;
                default:
                    col.Item().Text("Tipo de reporte no soportado: " + reportTypeCode)
                        .FontColor(Colors.Red.Medium);
                    break;
            }
        });
    }

    private void ComposeFooter(IContainer container, PDFTemplate? template, TenantConfiguration? config,
        ReportType reportType, string userId, int consecutive)
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
                    c2.Item().Text($"No. {consecutive:D4} / {DateTime.UtcNow.Year}")
                        .FontSize(7).FontColor(Colors.Grey.Darken2).AlignRight();
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

    private void RenderPortfolioReport(ColumnDescriptor col, string tenantId)
    {
        col.Item().Text("Reporte de Cartera").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        // Misma fuente que el Dashboard, el mapa de estado de pago y el módulo de Cuotas
        // y Cartera (PortfolioAgingService), para que este reporte nunca muestre una cifra
        // distinta a la que el administrador ya vio en pantalla.
        var overdueByUnit = _portfolioAgingService.GetOverdueByUnit(tenantId);

        if (overdueByUnit.Count == 0)
        {
            col.Item().Padding(10).Text("No hay unidades con saldo vencido.").FontColor(Colors.Grey.Darken2);
            return;
        }

        var unitIds = overdueByUnit.Keys.ToList();
        var units = _context.Units
            .Where(u => unitIds.Contains(u.Id))
            .Include(u => u.UnitOwners.Where(uo => uo.IsActive))
            .ThenInclude(uo => uo.Owner)
            .ToList();

        var rows = units
            .Select(u =>
            {
                var ownerName = u.UnitOwners
                    .Select(uo => uo.Owner!.FullNameOrCompanyName)
                    .FirstOrDefault() ?? "Sin propietario";
                var overdue = overdueByUnit[u.Id];

                return new
                {
                    UnitIdentifier = u.Identifier,
                    Tower = u.TowerOrBlock,
                    OwnerName = ownerName,
                    overdue.MonthsOverdue,
                    overdue.TotalDebt
                };
            })
            .OrderByDescending(x => x.TotalDebt)
            .ToList();

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(50);
                cd.ConstantColumn(60);
                cd.RelativeColumn();
                cd.ConstantColumn(70);
                cd.ConstantColumn(75);
            });

            var headerStyle = TextStyle.Default.FontSize(8).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Unidad").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Torre").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Propietario").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).AlignRight().Text("Meses de Mora").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).AlignRight().Text("Saldo Vencido").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(7);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            var grandTotal = 0m;

            foreach (var item in rows)
            {
                var bg = Colors.White;
                if (index % 2 != 0)
                {
                    bg = altColor;
                }

                var ownerDisplay = item.OwnerName;
                if (ownerDisplay.Length > 25)
                {
                    ownerDisplay = ownerDisplay[..25] + "...";
                }

                table.Cell().Background(bg).Padding(2).Text(item.UnitIdentifier).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).Text(item.Tower).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).Text(ownerDisplay).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).AlignRight().Text(item.MonthsOverdue.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).AlignRight().Text(item.TotalDebt.ToString("N2")).Style(rowStyle);
                grandTotal += item.TotalDebt;
                index++;
            }

            var totalStyle = TextStyle.Default.FontSize(7).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("TOTALES").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(grandTotal.ToString("N2")).Style(totalStyle);
        });
    }

    private void RenderCollectionReport(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Reporte de Recaudo del Periodo").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var query = _context.Payments
            .Where(p => p.TenantId == tenantId)
            .Include(p => p.Unit)
            .ThenInclude(u => u!.UnitOwners.Where(uo => uo.IsActive))
            .ThenInclude(uo => uo.Owner)
            .AsQueryable();

        if (from.HasValue) query = query.Where(p => p.PaymentDate >= from.Value);
        if (to.HasValue) query = query.Where(p => p.PaymentDate <= to.Value);

        var payments = query.OrderByDescending(p => p.PaymentDate).ToList();

        if (payments.Count == 0)
        {
            col.Item().Padding(10).Text("No hay pagos registrados en el periodo.").FontColor(Colors.Grey.Darken2);
            return;
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(60);
                cd.ConstantColumn(50);
                cd.RelativeColumn();
                cd.ConstantColumn(60);
                cd.ConstantColumn(70);
            });

            var headerStyle = TextStyle.Default.FontSize(8).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Fecha").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Unidad").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Propietario").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Medio Pago").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).AlignRight().Text("Valor").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(7);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            var total = 0m;

            foreach (var payment in payments)
            {
                var ownerName = payment.Unit?.UnitOwners
                    .Select(uo => uo.Owner!.FullNameOrCompanyName)
                    .FirstOrDefault() ?? "";

                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(2).Text(payment.PaymentDate.ToString("dd/MMM", CultureInfo.GetCultureInfo("es-CO"))).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).Text(payment.Unit?.Identifier ?? "").Style(rowStyle);
                table.Cell().Background(bg).Padding(2).Text(ownerName.Length > 18 ? ownerName[..18] + "..." : ownerName).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).Text(payment.PaymentMethod.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).AlignRight().Text(payment.Amount.ToString("N2")).Style(rowStyle);
                total += payment.Amount;
                index++;
            }

            var totalStyle = TextStyle.Default.FontSize(7).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("TOTALES").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(total.ToString("N2")).Style(totalStyle);
        });
    }

    private void RenderExpenseReport(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Reporte de Gastos Ejecutados del Periodo").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var query = _context.ProviderPayments
            .Where(p => p.TenantId == tenantId)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Provider)
            .AsQueryable();

        if (from.HasValue) query = query.Where(p => p.PaymentDate >= from.Value);
        if (to.HasValue) query = query.Where(p => p.PaymentDate <= to.Value);

        var payments = query.ToList();

        if (payments.Count == 0)
        {
            col.Item().Padding(10).Text("No hay gastos registrados en el periodo.").FontColor(Colors.Grey.Darken2);
            return;
        }

        var budgetItemIds = payments
            .Where(p => p.Invoice != null && p.Invoice.BudgetItemId != null)
            .Select(p => p.Invoice!.BudgetItemId!.Value)
            .Distinct()
            .ToList();

        var budgetItemNames = _context.ExpenseItems
            .Where(e => budgetItemIds.Contains(e.Id))
            .ToDictionary(e => e.Id, e => e.Name);

        var grouped = payments
            .GroupBy(p => GetBudgetItemName(p, budgetItemNames))
            .OrderBy(g => g.Key)
            .ToList();

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(55);
                cd.RelativeColumn();
                cd.RelativeColumn();
                cd.ConstantColumn(55);
                cd.ConstantColumn(60);
            });

            var headerStyle = TextStyle.Default.FontSize(8).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Fecha").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Proveedor").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Factura").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Rubro").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).AlignRight().Text("Valor").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(7);
            var subtotalStyle = TextStyle.Default.FontSize(7).Bold().FontColor(Colors.Grey.Darken3);
            var altColor = Colors.Grey.Lighten4;
            var grandTotal = 0m;

            foreach (var group in grouped)
            {
                var index = 0;
                var groupTotal = 0m;
                var orderedPayments = group.OrderBy(p => p.PaymentDate).ToList();

                foreach (var payment in orderedPayments)
                {
                    var bg = Colors.White;
                    if (index % 2 != 0)
                    {
                        bg = altColor;
                    }

                    table.Cell().Background(bg).Padding(2).Text(payment.PaymentDate.ToString("dd/MMM", CultureInfo.GetCultureInfo("es-CO"))).Style(rowStyle);
                    table.Cell().Background(bg).Padding(2).Text(payment.Invoice?.Provider?.BusinessName ?? "").Style(rowStyle);
                    table.Cell().Background(bg).Padding(2).Text(payment.Invoice?.InvoiceNumber ?? "").Style(rowStyle);
                    table.Cell().Background(bg).Padding(2).Text(group.Key).Style(rowStyle);
                    table.Cell().Background(bg).Padding(2).AlignRight().Text(payment.Amount.ToString("N2")).Style(rowStyle);

                    groupTotal += payment.Amount;
                    index++;
                }

                table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("Subtotal " + group.Key).Style(subtotalStyle);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(subtotalStyle);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(subtotalStyle);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(subtotalStyle);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(groupTotal.ToString("N2")).Style(subtotalStyle);

                grandTotal += groupTotal;
            }

            var totalStyle = TextStyle.Default.FontSize(7).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(2).Text("TOTAL GENERAL").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(2).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(2).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(2).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(2).AlignRight().Text(grandTotal.ToString("N2")).Style(totalStyle);
        });
    }

    private static string GetBudgetItemName(ProviderPayment payment, Dictionary<Guid, string> budgetItemNames)
    {
        if (payment.Invoice is null || payment.Invoice.BudgetItemId is null)
        {
            return "Sin rubro asignado";
        }

        var budgetItemId = payment.Invoice.BudgetItemId.Value;
        if (budgetItemNames.ContainsKey(budgetItemId))
        {
            return budgetItemNames[budgetItemId];
        }

        return "Sin rubro asignado";
    }

    private void RenderBudgetExecutionReport(ColumnDescriptor col, string tenantId)
    {
        col.Item().Text("Ejecucion Presupuestal").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var fiscalYear = DateTime.Today.Year;
        var budget = _context.Budgets
            .Where(b => b.TenantId == tenantId && b.FiscalYear == fiscalYear && b.Status == BudgetStatus.Approved)
            .Include(b => b.ExpenseItems)
            .Include(b => b.IncomeItems)
            .FirstOrDefault();

        if (budget is null)
        {
            col.Item().Padding(10).Text($"No hay presupuesto aprobado para el ano fiscal {fiscalYear}.").FontColor(Colors.Grey.Darken2);
            return;
        }

        var startDate = new DateTime(fiscalYear, 1, 1);
        var endDate = new DateTime(fiscalYear, 12, 31, 23, 59, 59);
        var now = DateTime.UtcNow;
        var monthsElapsed = Math.Max((now.Year - fiscalYear) * 12 + now.Month - 1, 1);
        var proportionExpected = monthsElapsed / 12m;

        var executedExpensesInPeriod = _context.ExecutedExpenses
            .Where(e => e.TenantId == tenantId && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .ToList();

        var executedByBudgetItem = executedExpensesInPeriod
            .GroupBy(e => e.ExpenseItemId)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        var totalApprovedExpense = budget.ExpenseItems.Sum(e => e.AnnualValue);
        var totalApprovedIncome = budget.IncomeItems.Sum(i => i.AnnualValue);
        var totalExecuted = executedExpensesInPeriod.Sum(e => e.Amount);
        var expectedExpense = totalApprovedExpense * proportionExpected;
        var overallPercentage = 0m;
        if (totalApprovedExpense > 0)
        {
            overallPercentage = Math.Round(totalExecuted / totalApprovedExpense * 100m, 2);
        }

        col.Item().Text("Ingreso Presupuestado: " + totalApprovedIncome.ToString("N2")).FontSize(9).FontColor(Colors.Grey.Darken2);
        col.Item().Text("Gasto Presupuestado: " + totalApprovedExpense.ToString("N2")).FontSize(9).FontColor(Colors.Grey.Darken2);
        col.Item().Text("Ejecutado Acumulado: " + totalExecuted.ToString("N2")).FontSize(9).FontColor(Colors.Grey.Darken2);
        col.Item().Text("Esperado al Mes " + now.Month + ": " + expectedExpense.ToString("N2")).FontSize(9).FontColor(Colors.Grey.Darken2);
        col.Item().Text("Porcentaje de Ejecucion: " + overallPercentage.ToString("N2") + "%").FontSize(11).Bold();
        col.Item().PaddingBottom(8);

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.RelativeColumn();
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
                cd.ConstantColumn(70);
                cd.ConstantColumn(55);
            });

            var headerStyle = TextStyle.Default.FontSize(8).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Rubro").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).AlignRight().Text("Presupuestado").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).AlignRight().Text("Ejecutado").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).AlignRight().Text("Disponible").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).AlignRight().Text("% Ejec.").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(7);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            var totalExecutedLine = 0m;

            foreach (var item in budget.ExpenseItems)
            {
                var executed = 0m;
                if (executedByBudgetItem.ContainsKey(item.Id))
                {
                    executed = executedByBudgetItem[item.Id];
                }

                var available = item.AnnualValue - executed;
                var percentage = 0m;
                if (item.AnnualValue > 0)
                {
                    percentage = Math.Round(executed / item.AnnualValue * 100m, 2);
                }

                var bg = Colors.White;
                if (index % 2 != 0)
                {
                    bg = altColor;
                }

                table.Cell().Background(bg).Padding(2).Text(item.Name).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).AlignRight().Text(item.AnnualValue.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).AlignRight().Text(executed.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).AlignRight().Text(available.ToString("N2")).Style(rowStyle);

                var pctColor = Colors.Grey.Darken2;
                if (percentage > 100)
                {
                    pctColor = Colors.Red.Medium;
                }
                else if (percentage > 75)
                {
                    pctColor = Colors.Orange.Medium;
                }
                table.Cell().Background(bg).Padding(2).AlignRight().Text(percentage.ToString("N2") + "%").Style(rowStyle.FontColor(pctColor));
                totalExecutedLine += executed;
                index++;
            }

            var totalStyle = TextStyle.Default.FontSize(7).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("TOTALES").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(totalApprovedExpense.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(totalExecutedLine.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).AlignRight().Text((totalApprovedExpense - totalExecutedLine).ToString("N2")).Style(totalStyle);

            var overallPctColor = Colors.Grey.Darken2;
            if (overallPercentage > 100)
            {
                overallPctColor = Colors.Red.Medium;
            }
            else if (overallPercentage > 75)
            {
                overallPctColor = Colors.Orange.Medium;
            }
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(overallPercentage.ToString("N2") + "%").Style(totalStyle.FontColor(overallPctColor));
        });
    }

    private void RenderActiveContractsReport(ColumnDescriptor col, string tenantId)
    {
        col.Item().Text("Contratos Activos").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var today = DateTime.UtcNow.Date;
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
                cd.RelativeColumn();
                cd.ConstantColumn(55);
                cd.ConstantColumn(55);
                cd.ConstantColumn(55);
                cd.ConstantColumn(50);
            });

            var headerStyle = TextStyle.Default.FontSize(8).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Proveedor").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).AlignRight().Text("Valor").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Inicio").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Terminacion").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).AlignRight().Text("Dias Rest.").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(7);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            var totalValue = 0m;

            foreach (var contract in contracts)
            {
                var daysRemaining = (contract.EndDate.Date - today).Days;
                var bg = index % 2 == 0 ? Colors.White : altColor;

                table.Cell().Background(bg).Padding(2).Text(contract.Provider?.BusinessName ?? "").Style(rowStyle);
                table.Cell().Background(bg).Padding(2).AlignRight().Text(contract.TotalValue.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).Text(contract.StartDate.ToString("dd/MMM", CultureInfo.GetCultureInfo("es-CO"))).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).Text(contract.EndDate.ToString("dd/MMM", CultureInfo.GetCultureInfo("es-CO"))).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).AlignRight().Text(daysRemaining.ToString()).Style(rowStyle.FontColor(daysRemaining < 30 ? Colors.Red.Medium : Colors.Grey.Darken2));
                totalValue += contract.TotalValue;
                index++;
            }

            var totalStyle = TextStyle.Default.FontSize(7).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("TOTALES").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(totalValue.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(totalStyle);
        });
    }

    private void RenderPQRReport(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Reporte de PQR del Periodo").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var query = _context.PqrRecords
            .Where(p => p.TenantId == tenantId && !p.IsInternal)
            .AsQueryable();

        if (from.HasValue) query = query.Where(p => p.FiledAt >= from.Value);
        if (to.HasValue) query = query.Where(p => p.FiledAt <= to.Value);

        var records = query.OrderByDescending(p => p.FiledAt).ToList();

        if (records.Count == 0)
        {
            col.Item().Padding(10).Text("No hay PQR registradas en el periodo.").FontColor(Colors.Grey.Darken2);
            return;
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.ConstantColumn(55);
                cd.ConstantColumn(55);
                cd.ConstantColumn(55);
                cd.ConstantColumn(55);
                cd.ConstantColumn(55);
            });

            var headerStyle = TextStyle.Default.FontSize(8).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Radicado").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Tipo").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Categoria").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Estado").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).AlignRight().Text("Tiempo Resp.").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(7);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;

            foreach (var record in records)
            {
                var responseTime = record.ClosedAt.HasValue
                    ? (record.ClosedAt.Value - record.FiledAt).TotalHours
                    : (DateTime.UtcNow - record.FiledAt).TotalHours;

                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(2).Text(record.RadicadoNumber).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).Text(record.PQRType.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).Text(record.Category.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).Text(record.Status.ToString()).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).AlignRight().Text(Math.Round(responseTime, 0) + "h").Style(rowStyle);
                index++;
            }
        });
    }

    private void RenderMaintenanceReport(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Reporte de Mantenimientos Ejecutados").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var query = _context.WorkOrders
            .Where(w => w.TenantId == tenantId && w.Status == WorkOrderStatus.Completed)
            .Include(w => w.Asset)
            .AsQueryable();

        if (from.HasValue) query = query.Where(w => w.ExecutionEndDate >= from.Value);
        if (to.HasValue) query = query.Where(w => w.ExecutionEndDate <= to.Value);

        var orders = query.OrderByDescending(w => w.ExecutionEndDate).ToList();

        if (orders.Count == 0)
        {
            col.Item().Padding(10).Text("No hay ordenes completadas en el periodo.").FontColor(Colors.Grey.Darken2);
            return;
        }

        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cd =>
            {
                cd.RelativeColumn();
                cd.ConstantColumn(65);
                cd.ConstantColumn(55);
                cd.ConstantColumn(55);
                cd.ConstantColumn(55);
            });

            var headerStyle = TextStyle.Default.FontSize(8).Bold().FontColor(Colors.White);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Bien / Activo").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Tipo Mtto.").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Ejecucion").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).AlignRight().Text("Costo Real").Style(headerStyle);
            table.Cell().Background(Colors.Grey.Darken3).Padding(3).Text("Resultado").Style(headerStyle);

            var rowStyle = TextStyle.Default.FontSize(7);
            var altColor = Colors.Grey.Lighten4;
            var index = 0;
            var totalCost = 0m;

            foreach (var order in orders)
            {
                var assetName = order.Asset?.Name ?? "Sin activo";
                var mttoType = order.OrderType == WorkOrderType.Preventive ? "Preventivo" : "Correctivo";
                var result = order.Outcome?.ToString() ?? "Sin registro";

                var bg = index % 2 == 0 ? Colors.White : altColor;
                table.Cell().Background(bg).Padding(2).Text(assetName.Length > 20 ? assetName[..20] + "..." : assetName).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).Text(mttoType).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).Text(order.ExecutionEndDate?.ToString("dd/MMM", CultureInfo.GetCultureInfo("es-CO")) ?? "").Style(rowStyle);
                table.Cell().Background(bg).Padding(2).AlignRight().Text(order.ActualCost.ToString("N2")).Style(rowStyle);
                table.Cell().Background(bg).Padding(2).Text(result.Length > 15 ? result[..15] + "..." : result).Style(rowStyle);
                totalCost += order.ActualCost;
                index++;
            }

            var totalStyle = TextStyle.Default.FontSize(7).Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("TOTALES").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(totalCost.ToString("N2")).Style(totalStyle);
            table.Cell().Background(Colors.Grey.Lighten3).Padding(2).Text("").Style(totalStyle);
        });
    }

    private void RenderAssemblyReport(ColumnDescriptor col, string tenantId, DateTime? from, DateTime? to)
    {
        col.Item().Text("Reporte de Asambleas y Decisiones").FontSize(14).Bold();
        col.Item().PaddingBottom(8);

        var query = _context.Assemblies
            .Where(a => a.TenantId == tenantId)
            .Include(a => a.Attendances)
            .Include(a => a.AgendaItems)
            .AsQueryable();

        if (from.HasValue) query = query.Where(a => a.ScheduledDate >= from.Value);
        if (to.HasValue) query = query.Where(a => a.ScheduledDate <= to.Value);

        var assemblies = query.OrderByDescending(a => a.ScheduledDate).ToList();

        if (assemblies.Count == 0)
        {
            col.Item().Padding(10).Text("No hay asambleas registradas en el periodo.").FontColor(Colors.Grey.Darken2);
            return;
        }

        foreach (var assembly in assemblies)
        {
            var presentCoefficients = assembly.Attendances
                .Where(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Represented)
                .Sum(a => a.Coefficient);

            var quorumPct = assembly.TotalCoefficients > 0
                ? Math.Round(presentCoefficients / assembly.TotalCoefficients * 100, 1)
                : 0;

            var quorumAchieved = assembly.QuorumAchievedFirstCall || assembly.QuorumAchievedSecondCall;

            col.Item().Text(assembly.Title).FontSize(10).Bold();
            col.Item().PaddingBottom(2);

            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"Fecha: {assembly.ScheduledDate:dd/MMM/yyyy}").FontSize(8).FontColor(Colors.Grey.Darken2);
                row.RelativeItem().Text($"Tipo: {assembly.Type}").FontSize(8).FontColor(Colors.Grey.Darken2);
                row.RelativeItem().Text($"Quorum: {quorumPct}% " + (quorumAchieved ? "(Alcanzado)" : "(No alcanzado)")).FontSize(8).FontColor(quorumAchieved ? Colors.Green.Medium : Colors.Red.Medium);
            });
            col.Item().PaddingBottom(4);

            var decisions = assembly.AgendaItems.Where(ai => ai.RequiresVoting).ToList();
            if (decisions.Count > 0)
            {
                col.Item().Text("Decisiones aprobadas:").FontSize(8).Bold().FontColor(Colors.Grey.Darken2);
                foreach (var decision in decisions)
                {
                    var status = decision.IsApproved switch
                    {
                        true => "APROBADO",
                        false => "RECHAZADO",
                        null => "PENDIENTE"
                    };
                    col.Item().Text($"  - {decision.Title}: {status}").FontSize(7).FontColor(Colors.Grey.Darken2);
                }
            }
            col.Item().PaddingBottom(8);
        }
    }

    public async Task<GeneratedReport> GenerateAnnualReportPdfAsync(
        string tenantId, string reportTypeId, string userId, int fiscalYear, Guid? recurringConfigId = null)
    {
        var template = await _context.PDFTemplates
            .Where(t => t.TenantId == tenantId && t.IsGlobal)
            .FirstOrDefaultAsync();

        var tenantConfig = await _context.TenantConfigurations.FirstOrDefaultAsync(tc => tc.TenantId == tenantId);
        var sections = await _context.ManagementReportSections
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.SectionOrder)
            .ToListAsync();

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var dir = Path.Combine(_env.WebRootPath ?? "wwwroot", "reports", tenantId, "AnnualManagementReport");
        Directory.CreateDirectory(dir);

        var consecutive = await GetNextConsecutiveNumber(tenantId, Guid.Parse(reportTypeId));
        var fileName = $"AnnualManagementReport_{fiscalYear}_{consecutive:D4}_{timestamp}.pdf";
        var filePath = Path.Combine(dir, fileName);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));
                page.Header().Element(h => ComposeAnnualHeader(h, template, tenantConfig, fiscalYear));
                page.Content().Element(c => ComposeAnnualContent(c, fiscalYear, sections));
                page.Footer().Element(f => ComposeFooter(f, template, tenantConfig, new ReportType { Name = "Informe de Gestion Anual", ContainsPersonalData = false }, userId, consecutive));
            });
        });

        document.GeneratePdf(filePath);

        var fileInfo = new FileInfo(filePath);
        var generated = new GeneratedReport
        {
            TenantId = tenantId,
            ReportTypeId = Guid.Parse(reportTypeId),
            Format = ReportFormat.Pdf,
            PeriodFrom = new DateTime(fiscalYear, 1, 1),
            PeriodTo = new DateTime(fiscalYear, 12, 31),
            FileName = fileName,
            FilePath = filePath,
            FileSizeBytes = fileInfo.Length,
            GeneratedByUserId = userId,
            GeneratedAt = DateTime.UtcNow,
            Notes = "Informe de Gestion Anual " + fiscalYear,
            RecurringConfigId = recurringConfigId,
            ConsecutiveNumber = consecutive
        };

        _context.GeneratedReports.Add(generated);
        await _context.SaveChangesAsync();
        return generated;
    }

    private void ComposeAnnualHeader(IContainer container, PDFTemplate? template, TenantConfiguration? config, int fiscalYear)
    {
        container.Column(col =>
        {
            var primary = ParseColor(template?.PrimaryColor ?? "#059669");
            col.Item().Row(row =>
            {
                if (template?.LogoFilePath is not null && File.Exists(template.LogoFilePath))
                    row.ConstantItem(80).Image(template.LogoFilePath).FitWidth();

                var headerText = template?.HeaderText ?? (config?.OfficialName ?? "Propiedad Horizontal");
                row.RelativeItem().PaddingLeft(10).AlignMiddle().Column(c2 =>
                {
                    c2.Item().Text(headerText).FontSize(16).Bold().FontColor(primary);
                    if (config is not null)
                        c2.Item().Text($"NIT {config.Nit}-{config.VerificationDigit}").FontSize(9).FontColor(Colors.Grey.Darken2);
                    c2.Item().Text("Informe de Gestion Anual " + fiscalYear).FontSize(14).Bold().FontColor(Colors.Grey.Darken4);
                });
            });
            col.Item().PaddingVertical(6).LineHorizontal(1).LineColor(primary);
        });
    }

    private void ComposeAnnualContent(IContainer container, int fiscalYear, List<ManagementReportSection> sections)
    {
        container.Column(col =>
        {
            foreach (var section in sections.Where(s => !string.IsNullOrEmpty(s.Content)))
            {
                col.Item().Text(section.Title).FontSize(12).Bold();
                col.Item().PaddingBottom(4);
                col.Item().Text(section.Content).FontSize(9);
                col.Item().PaddingBottom(12);
            }
        });
    }

    public async Task<byte[]> GeneratePreviewBytesAsync(
        string tenantId, string reportTypeCode, DateTime? periodFrom, DateTime? periodTo)
    {
        if (!Enum.TryParse<ReportTypeEnum>(reportTypeCode, out var reportTypeEnum))
            throw new InvalidOperationException("Invalid report type code: " + reportTypeCode);

        var reportType = await _context.ReportTypes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.ReportTypeCode == reportTypeEnum);
        if (reportType is null)
            throw new InvalidOperationException("Report type not found: " + reportTypeCode);

        var template = await _context.PDFTemplates
            .Where(t => t.TenantId == tenantId && t.IsGlobal)
            .FirstOrDefaultAsync();

        var tenantConfig = await _context.TenantConfigurations.FirstOrDefaultAsync(tc => tc.TenantId == tenantId);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));
                page.Header().Element(h => ComposeHeader(h, template, tenantConfig, reportType));
                page.Content().Element(c => ComposeContent(c, tenantId, reportTypeCode, periodFrom, periodTo, null));
                page.Footer().Element(f => ComposeFooter(f, template, tenantConfig, reportType, "preview", 0));
            });
        });

        return document.GeneratePdf();
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
            var r = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            return Color.FromRGB(r, g, b);
        }
        return Colors.Green.Medium;
    }
}
