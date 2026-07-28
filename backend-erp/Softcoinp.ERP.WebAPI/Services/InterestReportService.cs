using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Services;

public class InterestReportService
{
    private readonly ApplicationDbContext _context;

    public InterestReportService(ApplicationDbContext context)
    {
        _context = context;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<InterestReportDto> GetReportDataAsync(
        string tenantId, Guid? unitId, string? status, DateTime? from, DateTime? to)
    {
        var query = _context.AccruedInterests
            .Where(ai => ai.TenantId == tenantId)
            .AsQueryable();

        if (unitId.HasValue)
            query = query.Where(ai => ai.UnitId == unitId.Value);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<AccruedInterestStatus>(status, true, out var statusEnum))
            query = query.Where(ai => ai.Status == statusEnum);

        if (from.HasValue)
            query = query.Where(ai => ai.InterestStartDate >= from.Value);

        if (to.HasValue)
            query = query.Where(ai => ai.InterestEndDate <= to.Value);

        var interests = await query
            .OrderBy(ai => ai.UnitId)
            .ThenBy(ai => ai.InterestStartDate)
            .Select(ai => new InterestReportLineDto
            {
                Id = ai.Id,
                UnitId = ai.UnitId,
                Period = ai.Period,
                DailyRate = ai.DailyRate,
                DaysInPeriod = ai.DaysInPeriod,
                BaseAmount = ai.BaseAmount,
                CalculatedAmount = ai.CalculatedAmount,
                BalanceAmount = ai.BalanceAmount,
                Status = ai.Status.ToString(),
                InterestStartDate = ai.InterestStartDate,
                InterestEndDate = ai.InterestEndDate
            })
            .ToListAsync();

        var unitIds = interests.Select(i => i.UnitId).Distinct().ToList();
        var units = await _context.Units
            .Where(u => unitIds.Contains(u.Id) && u.TenantId == tenantId)
            .ToDictionaryAsync(u => u.Id, u => u.Identifier);

        foreach (var line in interests)
        {
            if (units.TryGetValue(line.UnitId, out var identifier))
                line.UnitIdentifier = identifier;
        }

        return new InterestReportDto
        {
            Lines = interests,
            TotalCalculated = interests.Sum(i => i.CalculatedAmount),
            TotalBalance = interests.Sum(i => i.BalanceAmount),
            TotalBaseAmount = interests.Sum(i => i.BaseAmount),
            PendingCount = interests.Count(i => i.Status == "Pending"),
            PaidCount = interests.Count(i => i.Status == "Paid"),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<byte[]> GenerateExcelAsync(
        string tenantId, Guid? unitId, string? status, DateTime? from, DateTime? to)
    {
        var data = await GetReportDataAsync(tenantId, unitId, status, from, to);

        using var workbook = new XLWorkbook();
        workbook.Style.Font.FontSize = 10;
        workbook.Style.Font.FontName = "Calibri";

        var ws = workbook.Worksheets.Add("Reporte Intereses");

        ws.Cell("A1").Value = "Reporte de Intereses de Mora";
        ws.Cell("A1").Style.Font.Bold = true;
        ws.Cell("A1").Style.Font.FontSize = 14;

        ws.Cell("A2").Value = "Generado: " + DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm");
        ws.Cell("A2").Style.Font.FontSize = 9;

        ws.Cell("A4").Value = "Unidad";
        ws.Cell("B4").Value = "Período";
        ws.Cell("C4").Value = "Monto Base";
        ws.Cell("D4").Value = "Tasa Diaria";
        ws.Cell("E4").Value = "Días";
        ws.Cell("F4").Value = "Interés Calculado";
        ws.Cell("G4").Value = "Saldo Pendiente";
        ws.Cell("H4").Value = "Estado";
        ws.Cell("I4").Value = "Período Inicio";
        ws.Cell("J4").Value = "Período Fin";

        var headerRange = ws.Range("A4:J4");
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromArgb(0xE8, 0xF5, 0xE9);
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        int row = 5;
        foreach (var line in data.Lines)
        {
            ws.Cell(row, 1).Value = line.UnitIdentifier;
            ws.Cell(row, 2).Value = line.Period;
            ws.Cell(row, 3).Value = line.BaseAmount;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 4).Value = (line.DailyRate * 100);
            ws.Cell(row, 4).Style.NumberFormat.Format = "0.000000%";
            ws.Cell(row, 5).Value = line.DaysInPeriod;
            ws.Cell(row, 6).Value = line.CalculatedAmount;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Value = line.BalanceAmount;
            ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 8).Value = line.Status == "Paid" ? "Pagado" : "Pendiente";
            ws.Cell(row, 9).Value = line.InterestStartDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 10).Value = line.InterestEndDate.ToString("yyyy-MM-dd");
            row++;
        }

        ws.Cell(row + 1, 1).Value = "RESUMEN";
        ws.Cell(row + 1, 1).Style.Font.Bold = true;

        ws.Cell(row + 2, 1).Value = "Total Interés Calculado:";
        ws.Cell(row + 2, 2).Value = data.TotalCalculated;
        ws.Cell(row + 2, 2).Style.NumberFormat.Format = "#,##0.00";

        ws.Cell(row + 3, 1).Value = "Total Saldo Pendiente:";
        ws.Cell(row + 3, 2).Value = data.TotalBalance;
        ws.Cell(row + 3, 2).Style.NumberFormat.Format = "#,##0.00";

        ws.Cell(row + 4, 1).Value = "Pendientes:";
        ws.Cell(row + 4, 2).Value = data.PendingCount;

        ws.Cell(row + 5, 1).Value = "Pagados:";
        ws.Cell(row + 5, 2).Value = data.PaidCount;

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> GeneratePdfAsync(
        string tenantId, Guid? unitId, string? status, DateTime? from, DateTime? to)
    {
        var data = await GetReportDataAsync(tenantId, unitId, status, from, to);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(style => style.FontSize(9));

                page.Header().Column(header =>
                {
                    header.Item().Text("Reporte de Intereses de Mora").FontSize(14).Bold();
                    header.Item().Text("Generado: " + DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"))
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                    header.Item().PaddingBottom(10);
                });

                var headerStyle = TextStyle.Default.FontSize(8).Bold();
                var rowStyle = TextStyle.Default.FontSize(7);

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(0.8f);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.2f);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                    });

                    table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Unidad").Style(headerStyle);
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Período").Style(headerStyle);
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text("Base").Style(headerStyle);
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text("Tasa Diaria").Style(headerStyle);
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text("Días").Style(headerStyle);
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text("Calculado").Style(headerStyle);
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(3).AlignRight().Text("Saldo").Style(headerStyle);
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Estado").Style(headerStyle);
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Inicio").Style(headerStyle);
                    table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Fin").Style(headerStyle);

                    foreach (var line in data.Lines)
                    {
                        var bgColor = line.Status == "Paid"
                            ? Colors.Green.Lighten5
                            : Colors.White;

                        table.Cell().Background(bgColor).Padding(2).Text(line.UnitIdentifier).Style(rowStyle);
                        table.Cell().Background(bgColor).Padding(2).Text(line.Period).Style(rowStyle);
                        table.Cell().Background(bgColor).Padding(2).AlignRight().Text(line.BaseAmount.ToString("N2")).Style(rowStyle);
                        table.Cell().Background(bgColor).Padding(2).AlignRight().Text((line.DailyRate * 100).ToString("F6") + "%").Style(rowStyle);
                        table.Cell().Background(bgColor).Padding(2).AlignRight().Text(line.DaysInPeriod.ToString()).Style(rowStyle);
                        table.Cell().Background(bgColor).Padding(2).AlignRight().Text(line.CalculatedAmount.ToString("N2")).Style(rowStyle);
                        table.Cell().Background(bgColor).Padding(2).AlignRight().Text(line.BalanceAmount.ToString("N2")).Style(rowStyle);
                        table.Cell().Background(bgColor).Padding(2).Text(line.Status == "Paid" ? "Pagado" : "Pendiente").Style(rowStyle);
                        table.Cell().Background(bgColor).Padding(2).Text(line.InterestStartDate.ToString("yyyy-MM-dd")).Style(rowStyle);
                        table.Cell().Background(bgColor).Padding(2).Text(line.InterestEndDate.ToString("yyyy-MM-dd")).Style(rowStyle);
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Página ").FontSize(8);
                    text.CurrentPageNumber().FontSize(8);
                    text.Span(" de ").FontSize(8);
                    text.TotalPages().FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }
}
