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

namespace Softcoinp.ERP.WebAPI.Services;

public class ExcelGenerationEngine
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    private readonly PortfolioAgingService _portfolioAgingService;

    public ExcelGenerationEngine(ApplicationDbContext context, IWebHostEnvironment env, PortfolioAgingService portfolioAgingService)
    {
        _context = context;
        _env = env;
        _portfolioAgingService = portfolioAgingService;
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

        var consecutive = await GetNextConsecutiveNumber(tenantId, reportType.Id);
        var fileName = $"{reportTypeCode}_{periodLabel}_{consecutive:D4}_{timestamp}.xlsx";
        var filePath = Path.Combine(dir, fileName);

        using var workbook = new XLWorkbook();
        workbook.Style.Font.FontSize = 10;
        workbook.Style.Font.FontName = "Calibri";

        var ws = workbook.Worksheets.Add(reportType.Name.Length > 31 ? reportType.Name[..31] : reportType.Name);
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

    private void ConfigureSheet(IXLWorksheet ws, string reportTypeCode, string tenantId,
        DateTime? periodFrom, DateTime? periodTo)
    {
        switch (reportTypeCode)
        {
            case "PortfolioReport":
                FillPortfolioReport(ws, tenantId);
                break;
            case "CollectionReport":
                FillCollectionReport(ws, tenantId, periodFrom, periodTo);
                break;
            case "ExpenseReport":
                FillExpenseReport(ws, tenantId, periodFrom, periodTo);
                break;
            case "BudgetExecution":
                FillBudgetExecutionReport(ws, tenantId);
                break;
            case "ActiveContracts":
                FillActiveContractsReport(ws, tenantId);
                break;
            case "PQRReport":
                FillPQRReport(ws, tenantId, periodFrom, periodTo);
                break;
            case "MaintenanceReport":
                FillMaintenanceReport(ws, tenantId, periodFrom, periodTo);
                break;
            default:
                ws.Cell(1, 1).Value = "Exportacion de datos";
                ws.Cell(2, 1).Value = "Tipo de reporte: " + reportTypeCode;
                ws.Cell(2, 1).Style.Font.Italic = true;
                ws.Columns().AdjustToContents();
                break;
        }
    }

    private static void StyleHeader(IXLRow row, string[] headers, int startCol = 1)
    {
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = row.Cell(startCol + i);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }
    }

    private static void ApplyRowStyle(IXLRow row, int colCount, bool isAlt)
    {
        if (!isAlt) return;
        for (var c = 1; c <= colCount; c++)
            row.Cell(c).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8fafc");
    }

    private void FillPortfolioReport(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Unidad", "Torre/Bloque", "Propietario", "Meses de Mora", "Saldo Vencido" };
        StyleHeader(ws.Row(1), headers);

        // Misma fuente que el Dashboard, el mapa de estado de pago y el módulo de Cuotas
        // y Cartera (PortfolioAgingService), para que este reporte nunca muestre una cifra
        // distinta a la que el administrador ya vio en pantalla.
        var overdueByUnit = _portfolioAgingService.GetOverdueByUnit(tenantId);

        if (overdueByUnit.Count == 0)
        {
            ws.Cell(2, 1).Value = "No hay unidades con saldo vencido.";
            ws.Columns().AdjustToContents();
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
                    .FirstOrDefault() ?? "";
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

        var row = 2;
        var grandTotal = 0m;

        foreach (var item in rows)
        {
            var r = ws.Row(row);
            r.Cell(1).Value = item.UnitIdentifier;
            r.Cell(2).Value = item.Tower;
            r.Cell(3).Value = item.OwnerName;
            r.Cell(4).Value = item.MonthsOverdue;
            r.Cell(5).Value = item.TotalDebt; r.Cell(5).Style.NumberFormat.Format = "#,##0";
            ApplyRowStyle(r, 5, row % 2 == 0);

            grandTotal += item.TotalDebt;
            row++;
        }

        var t = ws.Row(row);
        t.Cell(1).Value = "TOTALES"; t.Cell(1).Style.Font.Bold = true;
        t.Cell(5).Value = grandTotal; t.Cell(5).Style.Font.Bold = true; t.Cell(5).Style.NumberFormat.Format = "#,##0";

        ws.Range(1, 1, row, 5).SetAutoFilter();
        ws.Column(1).Width = 14; ws.Column(2).Width = 14; ws.Column(3).Width = 30;
        ws.Column(4).Width = 16; ws.Column(5).Width = 16;
    }

    private void FillCollectionReport(IXLWorksheet ws, string tenantId, DateTime? from, DateTime? to)
    {
        var headers = new[] { "Fecha", "Unidad", "Propietario", "Valor", "Medio de Pago", "Comprobante" };
        StyleHeader(ws.Row(1), headers);

        var query = _context.Payments
            .Where(p => p.TenantId == tenantId)
            .Include(p => p.Unit)
            .ThenInclude(u => u!.UnitOwners.Where(uo => uo.IsActive))
            .ThenInclude(uo => uo.Owner)
            .AsQueryable();

        if (from.HasValue) query = query.Where(p => p.PaymentDate >= from.Value);
        if (to.HasValue) query = query.Where(p => p.PaymentDate <= to.Value);

        var payments = query.OrderByDescending(p => p.PaymentDate).ToList();
        var row = 2;
        var totalAmount = 0m;

        foreach (var payment in payments)
        {
            var ownerName = payment.Unit?.UnitOwners
                .Select(uo => uo.Owner!.FullNameOrCompanyName)
                .FirstOrDefault() ?? "";

            var r = ws.Row(row);
            r.Cell(1).Value = payment.PaymentDate.ToString("yyyy-MM-dd");
            r.Cell(2).Value = payment.Unit?.Identifier ?? "";
            r.Cell(3).Value = ownerName;
            r.Cell(4).Value = payment.Amount; r.Cell(4).Style.NumberFormat.Format = "#,##0.00";
            r.Cell(5).Value = payment.PaymentMethod.ToString();
            r.Cell(6).Value = payment.ReferenceNumber;
            ApplyRowStyle(r, 6, row % 2 == 0);
            totalAmount += payment.Amount;
            row++;
        }

        var t = ws.Row(row);
        t.Cell(1).Value = "TOTALES"; t.Cell(1).Style.Font.Bold = true;
        t.Cell(4).Value = totalAmount; t.Cell(4).Style.Font.Bold = true; t.Cell(4).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, 6).SetAutoFilter();
        ws.Column(1).Width = 14; ws.Column(2).Width = 14; ws.Column(3).Width = 30;
        ws.Column(4).Width = 15; ws.Column(5).Width = 16; ws.Column(6).Width = 20;
    }

    private void FillExpenseReport(IXLWorksheet ws, string tenantId, DateTime? from, DateTime? to)
    {
        var headers = new[] { "Fecha", "Proveedor", "Descripcion", "Rubro", "Valor", "Comprobante" };
        StyleHeader(ws.Row(1), headers);

        var query = _context.ProviderPayments
            .Where(p => p.TenantId == tenantId)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Provider)
            .AsQueryable();

        if (from.HasValue) query = query.Where(p => p.PaymentDate >= from.Value);
        if (to.HasValue) query = query.Where(p => p.PaymentDate <= to.Value);

        var payments = query.ToList();

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

        var row = 2;
        var grandTotal = 0m;

        foreach (var group in grouped)
        {
            var orderedPayments = group.OrderBy(p => p.PaymentDate).ToList();
            var groupTotal = 0m;

            foreach (var payment in orderedPayments)
            {
                var r = ws.Row(row);
                r.Cell(1).Value = payment.PaymentDate.ToString("yyyy-MM-dd");
                r.Cell(2).Value = payment.Invoice?.Provider?.BusinessName ?? "";
                r.Cell(3).Value = payment.ReferenceNumber;
                r.Cell(4).Value = group.Key;
                r.Cell(5).Value = payment.Amount; r.Cell(5).Style.NumberFormat.Format = "#,##0.00";
                r.Cell(6).Value = payment.Invoice?.InvoiceNumber ?? "";
                ApplyRowStyle(r, 6, row % 2 == 0);
                groupTotal += payment.Amount;
                row++;
            }

            var subRow = ws.Row(row);
            subRow.Cell(1).Value = "SUBTOTAL " + group.Key;
            subRow.Cell(1).Style.Font.Bold = true;
            subRow.Cell(5).Value = groupTotal;
            subRow.Cell(5).Style.Font.Bold = true;
            subRow.Cell(5).Style.NumberFormat.Format = "#,##0.00";
            for (var c = 1; c <= headers.Length; c++)
            {
                subRow.Cell(c).Style.Fill.BackgroundColor = XLColor.FromHtml("#e2e8f0");
            }
            row++;
            grandTotal += groupTotal;
        }

        var t = ws.Row(row);
        t.Cell(1).Value = "TOTAL GENERAL"; t.Cell(1).Style.Font.Bold = true;
        t.Cell(5).Value = grandTotal; t.Cell(5).Style.Font.Bold = true; t.Cell(5).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, 6).SetAutoFilter();
        ws.Column(1).Width = 14; ws.Column(2).Width = 28; ws.Column(3).Width = 35;
        ws.Column(4).Width = 20; ws.Column(5).Width = 15; ws.Column(6).Width = 18;
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

    private void FillBudgetExecutionReport(IXLWorksheet ws, string tenantId)
    {
        var fiscalYear = DateTime.Today.Year;
        var budget = _context.Budgets
            .Include(b => b.ExpenseItems)
            .Include(b => b.IncomeItems)
            .FirstOrDefault(b => b.TenantId == tenantId && b.FiscalYear == fiscalYear && b.Status == BudgetStatus.Approved);

        if (budget == null)
        {
            ws.Cell(1, 1).Value = "Ejecucion Presupuestal";
            ws.Cell(2, 1).Value = $"No hay presupuesto aprobado para el ano fiscal {fiscalYear}.";
            ws.Columns().AdjustToContents();
            return;
        }

        var now = DateTime.UtcNow;
        var startDate = new DateTime(fiscalYear, 1, 1);
        var endDate = new DateTime(fiscalYear, 12, 31, 23, 59, 59);
        var monthsElapsed = Math.Max((now.Year - fiscalYear) * 12 + now.Month - 1, 1);
        var proportionExpected = monthsElapsed / 12m;

        var executedExpenses = _context.ExecutedExpenses
            .Where(e => e.TenantId == tenantId && e.ExpenseDate >= startDate && e.ExpenseDate <= endDate)
            .ToList();
        var executionByExpenseItem = executedExpenses
            .GroupBy(e => e.ExpenseItemId)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        var headers = new[] { "Rubro", "Presupuestado Anual", "Esperado al Mes", "Ejecutado Acumulado", "Disponible", "% Ejecucion" };
        StyleHeader(ws.Row(1), headers);

        var row = 2;
        var totalApproved = 0m;
        var totalExecLine = 0m;
        var totalExpected = 0m;

        foreach (var item in budget.ExpenseItems)
        {
            var expected = item.AnnualValue * proportionExpected;
            var executed = 0m;
            if (executionByExpenseItem.TryGetValue(item.Id, out var executedForItem))
            {
                executed = executedForItem;
            }
            var available = item.AnnualValue - executed;
            var percentage = 0m;
            if (item.AnnualValue > 0)
            {
                percentage = Math.Round(executed / item.AnnualValue * 100m, 2);
            }

            var r = ws.Row(row);
            r.Cell(1).Value = item.Name;
            r.Cell(2).Value = item.AnnualValue; r.Cell(2).Style.NumberFormat.Format = "#,##0";
            r.Cell(3).Value = expected; r.Cell(3).Style.NumberFormat.Format = "#,##0";
            r.Cell(4).Value = executed; r.Cell(4).Style.NumberFormat.Format = "#,##0";
            r.Cell(5).Value = available; r.Cell(5).Style.NumberFormat.Format = "#,##0";
            r.Cell(6).Value = percentage; r.Cell(6).Style.NumberFormat.Format = "0.00";
            ApplyRowStyle(r, 6, row % 2 == 0);

            totalApproved += item.AnnualValue;
            totalExecLine += executed;
            totalExpected += expected;
            row++;
        }

        var overallPercentage = totalApproved > 0 ? Math.Round(totalExecLine / totalApproved * 100m, 2) : 0m;
        var t = ws.Row(row);
        t.Cell(1).Value = "TOTALES"; t.Cell(1).Style.Font.Bold = true;
        t.Cell(2).Value = totalApproved; t.Cell(2).Style.Font.Bold = true; t.Cell(2).Style.NumberFormat.Format = "#,##0";
        t.Cell(3).Value = totalExpected; t.Cell(3).Style.Font.Bold = true; t.Cell(3).Style.NumberFormat.Format = "#,##0";
        t.Cell(4).Value = totalExecLine; t.Cell(4).Style.Font.Bold = true; t.Cell(4).Style.NumberFormat.Format = "#,##0";
        t.Cell(5).Value = totalApproved - totalExecLine; t.Cell(5).Style.Font.Bold = true; t.Cell(5).Style.NumberFormat.Format = "#,##0";
        t.Cell(6).Value = overallPercentage; t.Cell(6).Style.Font.Bold = true; t.Cell(6).Style.NumberFormat.Format = "0.00";

        ws.Range(1, 1, row, 6).SetAutoFilter();
        ws.Column(1).Width = 28; ws.Column(2).Width = 18; ws.Column(3).Width = 18;
        ws.Column(4).Width = 18; ws.Column(5).Width = 14; ws.Column(6).Width = 12;
    }

    private void FillActiveContractsReport(IXLWorksheet ws, string tenantId)
    {
        var headers = new[] { "Contrato", "Proveedor", "Objeto", "Valor", "Fecha Inicio", "Fecha Terminacion", "Dias Restantes", "Evaluacion" };
        StyleHeader(ws.Row(1), headers);

        var today = DateTime.UtcNow.Date;
        var contracts = _context.Contracts
            .Where(c => c.TenantId == tenantId && c.Status == ContractStatus.Active)
            .Include(c => c.Provider)
            .OrderBy(c => c.StartDate)
            .ToList();

        var row = 2;
        var totalValue = 0m;

        foreach (var contract in contracts)
        {
            var daysRemaining = (contract.EndDate.Date - today).Days;
            var r = ws.Row(row);
            r.Cell(1).Value = contract.ContractNumber;
            r.Cell(2).Value = contract.Provider?.BusinessName ?? "";
            r.Cell(3).Value = contract.ObjectDescription;
            r.Cell(4).Value = contract.TotalValue; r.Cell(4).Style.NumberFormat.Format = "#,##0";
            r.Cell(5).Value = contract.StartDate.ToString("yyyy-MM-dd");
            r.Cell(6).Value = contract.EndDate.ToString("yyyy-MM-dd");
            r.Cell(7).Value = daysRemaining;
            r.Cell(8).Value = contract.Status.ToString();
            ApplyRowStyle(r, 8, row % 2 == 0);
            totalValue += contract.TotalValue;
            row++;
        }

        var t = ws.Row(row);
        t.Cell(1).Value = "TOTALES"; t.Cell(1).Style.Font.Bold = true;
        t.Cell(4).Value = totalValue; t.Cell(4).Style.Font.Bold = true; t.Cell(4).Style.NumberFormat.Format = "#,##0";

        ws.Range(1, 1, row, 8).SetAutoFilter();
        ws.Column(1).Width = 18; ws.Column(2).Width = 28; ws.Column(3).Width = 35;
        ws.Column(4).Width = 15; ws.Column(5).Width = 14; ws.Column(6).Width = 14;
        ws.Column(7).Width = 14; ws.Column(8).Width = 14;
    }

    private void FillPQRReport(IXLWorksheet ws, string tenantId, DateTime? from, DateTime? to)
    {
        var headers = new[] { "Radicado", "Tipo", "Categoria", "Fecha Radicacion", "Estado", "Tiempo Respuesta (h)" };
        StyleHeader(ws.Row(1), headers);

        var query = _context.PqrRecords
            .Where(p => p.TenantId == tenantId && !p.IsInternal)
            .AsQueryable();

        if (from.HasValue) query = query.Where(p => p.FiledAt >= from.Value);
        if (to.HasValue) query = query.Where(p => p.FiledAt <= to.Value);

        var records = query.OrderByDescending(p => p.FiledAt).ToList();
        var row = 2;

        foreach (var record in records)
        {
            var responseTime = record.ClosedAt.HasValue
                ? Math.Round((record.ClosedAt.Value - record.FiledAt).TotalHours, 1)
                : Math.Round((DateTime.UtcNow - record.FiledAt).TotalHours, 1);

            var r = ws.Row(row);
            r.Cell(1).Value = record.RadicadoNumber;
            r.Cell(2).Value = record.PQRType.ToString();
            r.Cell(3).Value = record.Category.ToString();
            r.Cell(4).Value = record.FiledAt.ToString("yyyy-MM-dd");
            r.Cell(5).Value = record.Status.ToString();
            r.Cell(6).Value = responseTime;
            ApplyRowStyle(r, 6, row % 2 == 0);
            row++;
        }

        ws.Range(1, 1, row - 1, 6).SetAutoFilter();
        ws.Column(1).Width = 18; ws.Column(2).Width = 16; ws.Column(3).Width = 20;
        ws.Column(4).Width = 16; ws.Column(5).Width = 16; ws.Column(6).Width = 18;
    }

    private void FillMaintenanceReport(IXLWorksheet ws, string tenantId, DateTime? from, DateTime? to)
    {
        var headers = new[] { "Bien/Activo", "Tipo Mantenimiento", "Proveedor", "Fecha Ejecucion", "Costo Real", "Resultado" };
        StyleHeader(ws.Row(1), headers);

        var query = _context.WorkOrders
            .Where(w => w.TenantId == tenantId && w.Status == WorkOrderStatus.Completed)
            .Include(w => w.Asset)
            .Include(w => w.AssignedProvider)
            .AsQueryable();

        if (from.HasValue) query = query.Where(w => w.ExecutionEndDate >= from.Value);
        if (to.HasValue) query = query.Where(w => w.ExecutionEndDate <= to.Value);

        var orders = query.OrderByDescending(w => w.ExecutionEndDate).ToList();
        var row = 2;
        var totalCost = 0m;

        foreach (var order in orders)
        {
            var mttoType = order.OrderType == WorkOrderType.Preventive ? "Preventivo" : "Correctivo";

            var r = ws.Row(row);
            r.Cell(1).Value = order.Asset?.Name ?? "";
            r.Cell(2).Value = mttoType;
            r.Cell(3).Value = order.AssignedProvider?.BusinessName ?? "";
            r.Cell(4).Value = order.ExecutionEndDate?.ToString("yyyy-MM-dd") ?? "";
            r.Cell(5).Value = order.ActualCost; r.Cell(5).Style.NumberFormat.Format = "#,##0";
            r.Cell(6).Value = order.Outcome?.ToString() ?? "";
            ApplyRowStyle(r, 6, row % 2 == 0);
            totalCost += order.ActualCost;
            row++;
        }

        var t = ws.Row(row);
        t.Cell(1).Value = "TOTALES"; t.Cell(1).Style.Font.Bold = true;
        t.Cell(5).Value = totalCost; t.Cell(5).Style.Font.Bold = true; t.Cell(5).Style.NumberFormat.Format = "#,##0";

        ws.Range(1, 1, row, 6).SetAutoFilter();
        ws.Column(1).Width = 22; ws.Column(2).Width = 16; ws.Column(3).Width = 28;
        ws.Column(4).Width = 16; ws.Column(5).Width = 14; ws.Column(6).Width = 22;
    }

    public async Task<GeneratedReport> GenerateAccountantExportAsync(
        string tenantId, string userId, DateTime periodFrom, DateTime periodTo)
    {
        var reportType = await _context.ReportTypes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.ReportTypeCode == ReportTypeEnum.AccountantExport);
        if (reportType is null)
            throw new InvalidOperationException("Report type not found: AccountantExport");

        using var workbook = new XLWorkbook();
        workbook.Style.Font.FontSize = 10;
        workbook.Style.Font.FontName = "Calibri";

        var incomeSheet = workbook.Worksheets.Add("Ingresos");
        FillAccountantIncomeSheet(incomeSheet, tenantId, periodFrom, periodTo);

        var expenseSheet = workbook.Worksheets.Add("Egresos");
        FillAccountantExpenseSheet(expenseSheet, tenantId, periodFrom, periodTo);

        var periodLabel = BuildPeriodLabel(periodFrom, periodTo);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var dir = Path.Combine(_env.WebRootPath ?? "wwwroot", "reports", tenantId, "AccountantExport");
        Directory.CreateDirectory(dir);

        var consecutive = await GetNextConsecutiveNumber(tenantId, reportType.Id);
        var fileName = $"AccountantExport_{periodLabel}_{consecutive:D4}_{timestamp}.xlsx";
        var filePath = Path.Combine(dir, fileName);

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
            ConsecutiveNumber = consecutive
        };

        _context.GeneratedReports.Add(generated);
        await _context.SaveChangesAsync();
        return generated;
    }

    private void FillAccountantIncomeSheet(IXLWorksheet ws, string tenantId, DateTime from, DateTime to)
    {
        var headers = new[] {
            "Fecha", "Unidad", "Propietario", "Identificacion Propietario",
            "Valor", "Medio de Pago", "Comprobante", "Concepto", "Periodo"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var payments = _context.Payments
            .Where(p => p.TenantId == tenantId && p.PaymentDate >= from && p.PaymentDate <= to)
            .Include(p => p.Unit)
            .ThenInclude(u => u!.UnitOwners.Where(uo => uo.IsActive))
            .ThenInclude(uo => uo.Owner)
            .OrderBy(p => p.PaymentDate)
            .ThenBy(p => p.Unit!.Identifier)
            .ToList();

        var row = 2;
        var totalAmount = 0m;
        var currentMonth = 0;

        foreach (var payment in payments)
        {
            var paymentMonth = payment.PaymentDate.Month;
            if (currentMonth != 0 && paymentMonth != currentMonth)
            {
                var subRow = ws.Row(row);
                subRow.Cell(1).Value = $"SUBTOTAL {new DateTime(from.Year, currentMonth, 1):MMMM}";
                subRow.Cell(1).Style.Font.Bold = true;
                subRow.Cell(5).Value = totalAmount;
                subRow.Cell(5).Style.Font.Bold = true;
                subRow.Cell(5).Style.NumberFormat.Format = "#,##0.00";
                for (var c = 1; c <= headers.Length; c++)
                    subRow.Cell(c).Style.Fill.BackgroundColor = XLColor.FromHtml("#e2e8f0");
                row++;
                totalAmount = 0m;
            }
            currentMonth = paymentMonth;

            var owner = payment.Unit?.UnitOwners
                .Select(uo => uo.Owner)
                .FirstOrDefault();

            var ownerDoc = owner is not null
                ? $"{owner.DocumentType} {owner.DocumentNumber}"
                : "";
            var ownerName = owner?.FullNameOrCompanyName ?? "";

            var r = ws.Row(row);
            r.Cell(1).Value = payment.PaymentDate.ToString("yyyy-MM-dd");
            r.Cell(2).Value = payment.Unit?.Identifier ?? "";
            r.Cell(3).Value = ownerName;
            r.Cell(4).Value = ownerDoc;
            r.Cell(5).Value = payment.Amount; r.Cell(5).Style.NumberFormat.Format = "#,##0.00";
            r.Cell(6).Value = payment.PaymentMethod.ToString();
            r.Cell(7).Value = payment.ReferenceNumber;
            r.Cell(8).Value = "Cuota de administracion";
            r.Cell(9).Value = payment.PaymentDate.ToString("yyyy-MM");
            if (row % 2 == 0)
                for (var c = 1; c <= headers.Length; c++)
                    r.Cell(c).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8fafc");
            totalAmount += payment.Amount;
            row++;
        }

        if (payments.Count > 0)
        {
            var subRow = ws.Row(row);
            subRow.Cell(1).Value = $"SUBTOTAL {new DateTime(from.Year, currentMonth, 1):MMMM}";
            subRow.Cell(1).Style.Font.Bold = true;
            subRow.Cell(5).Value = totalAmount;
            subRow.Cell(5).Style.Font.Bold = true;
            subRow.Cell(5).Style.NumberFormat.Format = "#,##0.00";
            for (var c = 1; c <= headers.Length; c++)
                subRow.Cell(c).Style.Fill.BackgroundColor = XLColor.FromHtml("#e2e8f0");
            row += 2;
        }

        var grandTotal = _context.Payments
            .Where(p => p.TenantId == tenantId && p.PaymentDate >= from && p.PaymentDate <= to)
            .Sum(p => (decimal?)p.Amount) ?? 0;

        var t = ws.Row(row);
        t.Cell(1).Value = "TOTAL GENERAL DEL PERIODO";
        t.Cell(1).Style.Font.Bold = true;
        t.Cell(5).Value = grandTotal;
        t.Cell(5).Style.Font.Bold = true;
        t.Cell(5).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, headers.Length).SetAutoFilter();
        ws.Column(1).Width = 14; ws.Column(2).Width = 12; ws.Column(3).Width = 30;
        ws.Column(4).Width = 22; ws.Column(5).Width = 15; ws.Column(6).Width = 16;
        ws.Column(7).Width = 20; ws.Column(8).Width = 24; ws.Column(9).Width = 10;
    }

    private void FillAccountantExpenseSheet(IXLWorksheet ws, string tenantId, DateTime from, DateTime to)
    {
        var headers = new[] {
            "Fecha", "Proveedor", "Identificacion Proveedor", "Descripcion",
            "Rubro Presupuestal", "Valor", "Factura", "Medio de Pago", "Periodo"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e293b");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var payments = _context.ProviderPayments
            .Where(p => p.TenantId == tenantId && p.PaymentDate >= from && p.PaymentDate <= to)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Provider)
            .OrderBy(p => p.PaymentDate)
            .ToList();

        var row = 2;
        var totalAmount = 0m;
        var currentMonth = 0;

        foreach (var payment in payments)
        {
            var paymentMonth = payment.PaymentDate.Month;
            if (currentMonth != 0 && paymentMonth != currentMonth)
            {
                var subRow = ws.Row(row);
                subRow.Cell(1).Value = $"SUBTOTAL {new DateTime(from.Year, currentMonth, 1):MMMM}";
                subRow.Cell(1).Style.Font.Bold = true;
                subRow.Cell(6).Value = totalAmount;
                subRow.Cell(6).Style.Font.Bold = true;
                subRow.Cell(6).Style.NumberFormat.Format = "#,##0.00";
                for (var c = 1; c <= headers.Length; c++)
                    subRow.Cell(c).Style.Fill.BackgroundColor = XLColor.FromHtml("#e2e8f0");
                row++;
                totalAmount = 0m;
            }
            currentMonth = paymentMonth;

            var providerDoc = "";
            var providerName = payment.Invoice?.Provider?.BusinessName ?? "";

            var r = ws.Row(row);
            r.Cell(1).Value = payment.PaymentDate.ToString("yyyy-MM-dd");
            r.Cell(2).Value = providerName;
            r.Cell(3).Value = providerDoc;
            r.Cell(4).Value = payment.ReferenceNumber;
            r.Cell(5).Value = "";
            r.Cell(6).Value = payment.Amount; r.Cell(6).Style.NumberFormat.Format = "#,##0.00";
            r.Cell(7).Value = payment.Invoice?.InvoiceNumber ?? "";
            r.Cell(8).Value = payment.PaymentMethod.ToString();
            r.Cell(9).Value = payment.PaymentDate.ToString("yyyy-MM");
            if (row % 2 == 0)
                for (var c = 1; c <= headers.Length; c++)
                    r.Cell(c).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8fafc");
            totalAmount += payment.Amount;
            row++;
        }

        if (payments.Count > 0)
        {
            var subRow = ws.Row(row);
            subRow.Cell(1).Value = $"SUBTOTAL {new DateTime(from.Year, currentMonth, 1):MMMM}";
            subRow.Cell(1).Style.Font.Bold = true;
            subRow.Cell(6).Value = totalAmount;
            subRow.Cell(6).Style.Font.Bold = true;
            subRow.Cell(6).Style.NumberFormat.Format = "#,##0.00";
            for (var c = 1; c <= headers.Length; c++)
                subRow.Cell(c).Style.Fill.BackgroundColor = XLColor.FromHtml("#e2e8f0");
            row += 2;
        }

        var grandTotal = _context.ProviderPayments
            .Where(p => p.TenantId == tenantId && p.PaymentDate >= from && p.PaymentDate <= to)
            .Sum(p => (decimal?)p.Amount) ?? 0;

        var t = ws.Row(row);
        t.Cell(1).Value = "TOTAL GENERAL DEL PERIODO";
        t.Cell(1).Style.Font.Bold = true;
        t.Cell(6).Value = grandTotal;
        t.Cell(6).Style.Font.Bold = true;
        t.Cell(6).Style.NumberFormat.Format = "#,##0.00";

        ws.Range(1, 1, row, headers.Length).SetAutoFilter();
        ws.Column(1).Width = 14; ws.Column(2).Width = 28; ws.Column(3).Width = 22;
        ws.Column(4).Width = 35; ws.Column(5).Width = 22; ws.Column(6).Width = 15;
        ws.Column(7).Width = 18; ws.Column(8).Width = 16; ws.Column(9).Width = 10;
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
