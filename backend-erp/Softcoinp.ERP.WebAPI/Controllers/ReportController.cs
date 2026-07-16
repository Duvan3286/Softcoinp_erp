using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;
using Softcoinp.ERP.WebAPI.Services;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ReportController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PDFGenerationEngine _pdfEngine;
    private readonly ExcelGenerationEngine _excelEngine;
    private readonly ReportAccessControlService _accessControl;

    public ReportController(
        ApplicationDbContext context,
        PDFGenerationEngine pdfEngine,
        ExcelGenerationEngine excelEngine,
        ReportAccessControlService accessControl)
    {
        _context = context;
        _pdfEngine = pdfEngine;
        _excelEngine = excelEngine;
        _accessControl = accessControl;
    }

    private string GetTenantId()
    {
        return User.FindFirst("tenant_id")?.Value ?? string.Empty;
    }

    private string GetUserId()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
    }

    [HttpGet("catalog")]
    public async Task<ActionResult<List<ReportCatalogItemDto>>> GetCatalog()
    {
        var tenantId = GetTenantId();
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;

        var types = await _accessControl.GetFilteredCatalogAsync(tenantId, role);

        var catalog = types.Select(t => new ReportCatalogItemDto
        {
            Code = t.ReportTypeCode,
            Name = t.Name,
            Description = t.Description,
            Category = t.Category,
            ContainsPersonalData = t.ContainsPersonalData,
            AvailableFormats = GetAvailableFormats(t.ReportTypeCode)
        }).ToList();

        return Ok(catalog);
    }

    private static List<string> GetAvailableFormats(string reportTypeCode)
    {
        if (RestrictedFormatsByType.ContainsKey(reportTypeCode))
        {
            return RestrictedFormatsByType[reportTypeCode];
        }

        return new List<string> { "Pdf", "Excel" };
    }

    private static readonly Dictionary<string, List<string>> RestrictedFormatsByType = new()
    {
        ["AssemblyReport"] = new List<string> { "Pdf" },
        ["AnnualManagementReport"] = new List<string> { "Pdf" },
        ["AccountantExport"] = new List<string> { "Excel" }
    };

    [HttpPost("generate")]
    public async Task<ActionResult<GeneratedReportDto>> GenerateReport([FromBody] GenerateReportRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;

        if (!_accessControl.CanAccessReport(role, request.ReportTypeCode))
            return Forbid();

        if (!Enum.TryParse<ReportTypeEnum>(request.ReportTypeCode, out _))
            return BadRequest("Tipo de reporte invalido: " + request.ReportTypeCode);

        if (request.Format != "Pdf" && request.Format != "Excel")
            return BadRequest("Formato invalido. Use 'Pdf' o 'Excel'.");

        if (request.ReportTypeCode == "AnnualManagementReport")
            return BadRequest("Use el endpoint /report/annual/consolidate para generar el Informe de Gestion Anual.");

        if (request.ReportTypeCode == "AccountantExport")
            return BadRequest("Use el endpoint /report/accountant-export para generar la Exportacion para el Contador.");

        if (RestrictedFormatsByType.TryGetValue(request.ReportTypeCode, out var allowedFormats))
        {
            if (!allowedFormats.Contains(request.Format))
                return BadRequest("El formato " + request.Format + " no esta disponible para el tipo de reporte " + request.ReportTypeCode + ".");
        }

        GeneratedReport result;

        if (request.Format == "Excel")
        {
            result = await _excelEngine.GenerateExcelReportAsync(
                tenantId, request.ReportTypeCode, userId,
                request.PeriodFrom, request.PeriodTo,
                request.Parameters, request.Notes);
        }
        else
        {
            result = await _pdfEngine.GenerateReportAsync(
                tenantId, request.ReportTypeCode, request.Format, userId,
                request.PeriodFrom, request.PeriodTo,
                request.Parameters, request.Notes);
        }

        var reportType = await _context.ReportTypes.FindAsync(result.ReportTypeId);

        return Ok(new GeneratedReportDto
        {
            Id = result.Id,
            ReportTypeId = result.ReportTypeId,
            ReportTypeName = reportType?.Name ?? string.Empty,
            ReportTypeCode = reportType?.ReportTypeCode.ToString() ?? request.ReportTypeCode,
            Format = result.Format.ToString(),
            PeriodFrom = result.PeriodFrom,
            PeriodTo = result.PeriodTo,
            FileName = result.FileName,
            FileSizeBytes = result.FileSizeBytes,
            GeneratedByUserId = result.GeneratedByUserId,
            GeneratedAt = result.GeneratedAt,
            Parameters = result.Parameters,
            Notes = result.Notes,
            RecurringConfigId = result.RecurringConfigId,
            ConsecutiveNumber = result.ConsecutiveNumber
        });
    }

    [HttpGet("preview/{reportTypeCode}")]
    public async Task<IActionResult> PreviewReport(
        string reportTypeCode, [FromQuery] DateTime? periodFrom, [FromQuery] DateTime? periodTo)
    {
        var tenantId = GetTenantId();
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;

        if (!_accessControl.CanAccessReport(role, reportTypeCode))
            return Forbid();

        if (!Enum.TryParse<ReportTypeEnum>(reportTypeCode, out _))
            return BadRequest("Tipo de reporte invalido: " + reportTypeCode);

        var pdfBytes = await _pdfEngine.GeneratePreviewBytesAsync(tenantId, reportTypeCode, periodFrom, periodTo);
        return File(pdfBytes, "application/pdf");
    }

    [HttpGet("history")]
    public async Task<ActionResult<List<GeneratedReportDto>>> GetHistory(
        [FromQuery] string? reportTypeCode = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var tenantId = GetTenantId();
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;

        var history = await _accessControl.GetFilteredHistoryAsync(tenantId, role);

        if (!string.IsNullOrEmpty(reportTypeCode))
            history = history.Where(h => h.ReportTypeCode == reportTypeCode).ToList();

        if (from.HasValue)
            history = history.Where(h => h.GeneratedAt >= from.Value).ToList();

        if (to.HasValue)
            history = history.Where(h => h.GeneratedAt <= to.Value).ToList();

        return Ok(history);
    }

    [HttpGet("history/{id}/download")]
    public async Task<IActionResult> DownloadReport(Guid id)
    {
        var tenantId = GetTenantId();
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;

        var report = await _context.GeneratedReports
            .Include(r => r.ReportType)
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == tenantId);

        if (report is null)
            return NotFound("Reporte no encontrado.");

        if (report.ReportType is null)
            return NotFound("Tipo de reporte no encontrado.");

        if (!_accessControl.CanAccessReport(role, report.ReportType.ReportTypeCode.ToString()))
            return Forbid();

        if (!System.IO.File.Exists(report.FilePath))
            return NotFound("El archivo del reporte ya no esta disponible.");

        var contentType = report.Format == ReportFormat.Pdf
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        var fileBytes = await System.IO.File.ReadAllBytesAsync(report.FilePath);
        return File(fileBytes, contentType, report.FileName);
    }

    [HttpGet("recurring")]
    public async Task<ActionResult<List<RecurringReportConfigDto>>> GetRecurringConfigs()
    {
        var tenantId = GetTenantId();

        var configs = await _context.RecurringReportConfigs
            .Where(c => c.TenantId == tenantId)
            .Include(c => c.ReportType)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(configs.Select(c => new RecurringReportConfigDto
        {
            Id = c.Id,
            ReportTypeId = c.ReportTypeId,
            ReportTypeName = c.ReportType?.Name ?? string.Empty,
            Name = c.Name,
            Frequency = c.Frequency.ToString(),
            Format = c.Format.ToString(),
            RecipientEmails = string.IsNullOrEmpty(c.RecipientEmails)
                ? new List<string>()
                : c.RecipientEmails.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            SubjectTemplate = c.SubjectTemplate,
            BodyTemplate = c.BodyTemplate,
            LastExecutionAt = c.LastExecutionAt,
            NextExecutionAt = c.NextExecutionAt,
            Status = c.Status.ToString(),
            CreatedAt = c.CreatedAt
        }).ToList());
    }

    [HttpPost("recurring")]
    public async Task<ActionResult<RecurringReportConfigDto>> CreateRecurringConfig(
        [FromBody] CreateRecurringReportConfigRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        if (!Enum.TryParse<ReportTypeEnum>(request.ReportTypeCode, out var reportTypeCode))
            return BadRequest("Codigo de tipo de reporte invalido.");

        if (request.ReportTypeCode == "AnnualManagementReport" || request.ReportTypeCode == "AccountantExport")
            return BadRequest("El tipo de reporte " + request.ReportTypeCode + " no admite programacion recurrente.");

        var reportType = await _context.ReportTypes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.ReportTypeCode == reportTypeCode);
        if (reportType is null)
            return BadRequest("Tipo de reporte no encontrado.");

        if (!Enum.TryParse<ReportFrequency>(request.Frequency, out var frequency))
            return BadRequest("Frecuencia invalida.");

        if (!Enum.TryParse<ReportFormat>(request.Format, out var format))
            return BadRequest("Formato invalido.");

        if (RestrictedFormatsByType.TryGetValue(request.ReportTypeCode, out var allowedRecurringFormats))
        {
            if (!allowedRecurringFormats.Contains(request.Format))
                return BadRequest("El formato " + request.Format + " no esta disponible para el tipo de reporte " + request.ReportTypeCode + ".");
        }

        var config = new RecurringReportConfig
        {
            TenantId = tenantId,
            ReportTypeId = reportType.Id,
            Name = request.Name,
            Frequency = frequency,
            Format = format,
            RecipientEmails = string.Join(",", request.RecipientEmails),
            SubjectTemplate = request.SubjectTemplate,
            BodyTemplate = request.BodyTemplate,
            NextExecutionAt = CalculateNextExecution(frequency, DateTime.UtcNow),
            Status = ReportRecurrentStatus.Active,
            CreatedByUserId = userId
        };

        _context.RecurringReportConfigs.Add(config);
        await _context.SaveChangesAsync();

        return Ok(new RecurringReportConfigDto
        {
            Id = config.Id,
            ReportTypeId = config.ReportTypeId,
            ReportTypeName = reportType.Name,
            Name = config.Name,
            Frequency = config.Frequency.ToString(),
            Format = config.Format.ToString(),
            RecipientEmails = string.IsNullOrEmpty(config.RecipientEmails)
                ? new List<string>()
                : config.RecipientEmails.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList(),
            SubjectTemplate = config.SubjectTemplate,
            BodyTemplate = config.BodyTemplate,
            NextExecutionAt = config.NextExecutionAt,
            Status = config.Status.ToString(),
            CreatedAt = config.CreatedAt
        });
    }

    [HttpPut("recurring/{id}/pause")]
    public async Task<IActionResult> PauseRecurringConfig(Guid id)
    {
        var tenantId = GetTenantId();

        var config = await _context.RecurringReportConfigs
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

        if (config is null)
            return NotFound("Configuracion no encontrada.");

        config.Status = ReportRecurrentStatus.Paused;
        config.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("recurring/{id}/resume")]
    public async Task<IActionResult> ResumeRecurringConfig(Guid id)
    {
        var tenantId = GetTenantId();

        var config = await _context.RecurringReportConfigs
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId);

        if (config is null)
            return NotFound("Configuracion no encontrada.");

        config.Status = ReportRecurrentStatus.Active;
        config.NextExecutionAt = CalculateNextExecution(config.Frequency, DateTime.UtcNow);
        config.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("annual/sections")]
    public async Task<ActionResult<List<ManagementReportSectionDto>>> GetAnnualReportSections()
    {
        var tenantId = GetTenantId();

        var sections = await _context.ManagementReportSections
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.SectionOrder)
            .ToListAsync();

        return Ok(sections.Select(s => new ManagementReportSectionDto
        {
            Id = s.Id,
            SectionOrder = s.SectionOrder,
            Title = s.Title,
            Content = s.Content,
            Status = s.Status.ToString(),
            AutoGeneratedQuery = s.AutoGeneratedQuery,
            LastAutoGeneratedAt = s.LastAutoGeneratedAt,
            LastManualEditAt = s.LastManualEditAt,
            LastEditedByUserId = s.LastEditedByUserId,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }).ToList());
    }

    [HttpPut("annual/sections/{id}")]
    public async Task<ActionResult<ManagementReportSectionDto>> UpdateAnnualReportSection(
        Guid id, [FromBody] UpdateManagementReportSectionDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var section = await _context.ManagementReportSections
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

        if (section is null)
            return NotFound("Seccion no encontrada.");

        if (request.Title is not null)
            section.Title = request.Title;

        if (request.Content is not null)
        {
            section.Content = request.Content;
            section.Status = SectionStatus.ManuallyEdited;
            section.LastManualEditAt = DateTime.UtcNow;
            section.LastEditedByUserId = userId;
        }

        if (request.SectionOrder.HasValue)
            section.SectionOrder = request.SectionOrder.Value;

        section.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new ManagementReportSectionDto
        {
            Id = section.Id,
            SectionOrder = section.SectionOrder,
            Title = section.Title,
            Content = section.Content,
            Status = section.Status.ToString(),
            AutoGeneratedQuery = section.AutoGeneratedQuery,
            LastAutoGeneratedAt = section.LastAutoGeneratedAt,
            LastManualEditAt = section.LastManualEditAt,
            LastEditedByUserId = section.LastEditedByUserId,
            CreatedAt = section.CreatedAt,
            UpdatedAt = section.UpdatedAt
        });
    }

    [HttpPost("annual/sections/{id}/regenerate")]
    public async Task<IActionResult> RegenerateSection(Guid id)
    {
        var tenantId = GetTenantId();

        var section = await _context.ManagementReportSections
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

        if (section is null)
            return NotFound("Seccion no encontrada.");

        if (string.IsNullOrEmpty(section.AutoGeneratedQuery))
            return BadRequest("Esta seccion no tiene configurada una consulta de auto-generacion.");

        var content = await GenerateSectionContentAsync(tenantId, section.AutoGeneratedQuery);

        section.Content = content;
        section.Status = SectionStatus.AutoGenerated;
        section.LastAutoGeneratedAt = DateTime.UtcNow;
        section.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok();
    }

    private async Task<string> GenerateSectionContentAsync(string tenantId, string queryCode)
    {
        return queryCode switch
        {
            "PortfolioSummary" => await GeneratePortfolioSummaryAsync(tenantId),
            "BudgetSummary" => await GenerateBudgetSummaryAsync(tenantId),
            "MaintenanceSummary" => await GenerateMaintenanceSummaryAsync(tenantId),
            "PQRSummary" => await GeneratePQRSummaryAsync(tenantId),
            "ContractSummary" => await GenerateContractSummaryAsync(tenantId),
            "AssemblySummary" => await GenerateAssemblySummaryAsync(tenantId),
            _ => "Contenido generado automaticamente para: " + queryCode
        };
    }

    private async Task<string> GeneratePortfolioSummaryAsync(string tenantId)
    {
        var totalPending = await _context.UnitFees
            .Where(f => f.TenantId == tenantId && f.Status != FeeStatus.FullyPaid)
            .SumAsync(f => f.BalanceAmount);

        var unitCount = await _context.UnitFees
            .Where(f => f.TenantId == tenantId && f.Status != FeeStatus.FullyPaid)
            .Select(f => f.UnitId)
            .Distinct()
            .CountAsync();

        return $"La cartera del conjunto presenta un saldo pendiente total de ${totalPending:N2} " +
               $"distribuido en {unitCount} unidades con cuotas en mora. " +
               $"El recaudo del periodo se encuentra detallado en el reporte de recaudo anexo.";
    }

    private async Task<string> GenerateBudgetSummaryAsync(string tenantId)
    {
        var fiscalYear = DateTime.Today.Year;
        var budget = await _context.Budgets
            .Where(b => b.TenantId == tenantId && b.FiscalYear == fiscalYear)
            .FirstOrDefaultAsync();

        if (budget is null)
            return "No se ha configurado presupuesto para el ano fiscal " + fiscalYear + ".";

        var totalExpense = budget.ExpenseItems.Sum(e => e.AnnualValue);
        var totalIncome = budget.IncomeItems.Sum(i => i.AnnualValue);

        return $"El presupuesto para el ano fiscal {fiscalYear} fue aprobado por un total de " +
               $"ingresos de ${totalIncome:N2} y gastos de ${totalExpense:N2}. " +
               $"La ejecucion detallada se encuentra en el reporte de ejecucion presupuestal anexo.";
    }

    private async Task<string> GenerateMaintenanceSummaryAsync(string tenantId)
    {
        var fiscalYear = DateTime.Today.Year;
        var completed = await _context.WorkOrders
            .Where(w => w.TenantId == tenantId && w.Status == WorkOrderStatus.Completed
                && w.ExecutionEndDate.HasValue && w.ExecutionEndDate.Value.Year == fiscalYear)
            .CountAsync();

        var totalCost = await _context.WorkOrders
            .Where(w => w.TenantId == tenantId && w.Status == WorkOrderStatus.Completed
                && w.ExecutionEndDate.HasValue && w.ExecutionEndDate.Value.Year == fiscalYear)
            .SumAsync(w => (decimal?)w.ActualCost) ?? 0;

        return $"Durante el ano fiscal {fiscalYear} se completaron {completed} ordenes de mantenimiento " +
               $"por un costo total de ${totalCost:N2}. " +
               $"El detalle se encuentra en el reporte de mantenimientos ejecutados anexo.";
    }

    private async Task<string> GeneratePQRSummaryAsync(string tenantId)
    {
        var fiscalYear = DateTime.Today.Year;
        var total = await _context.PqrRecords
            .Where(p => p.TenantId == tenantId && p.FiledAt.Year == fiscalYear)
            .CountAsync();

        var resolved = await _context.PqrRecords
            .Where(p => p.TenantId == tenantId && p.FiledAt.Year == fiscalYear && p.Status == PQRStatus.Closed)
            .CountAsync();

        return $"Durante el ano fiscal {fiscalYear} se radicaron {total} PQR, de las cuales " +
               $"{resolved} han sido cerradas ({Math.Round(total > 0 ? (double)resolved / total * 100 : 0)}% de resolucion). " +
               $"El detalle se encuentra en el reporte de PQR anexo.";
    }

    private async Task<string> GenerateContractSummaryAsync(string tenantId)
    {
        var activeCount = await _context.Contracts
            .Where(c => c.TenantId == tenantId && c.Status == ContractStatus.Active)
            .CountAsync();

        var totalValue = await _context.Contracts
            .Where(c => c.TenantId == tenantId && c.Status == ContractStatus.Active)
            .SumAsync(c => c.TotalValue);

        return $"Actualmente hay {activeCount} contratos activos por un valor total de ${totalValue:N2}. " +
               $"El detalle se encuentra en el reporte de contratos activos anexo.";
    }

    private async Task<string> GenerateAssemblySummaryAsync(string tenantId)
    {
        var fiscalYear = DateTime.Today.Year;
        var assemblies = await _context.Assemblies
            .Where(a => a.TenantId == tenantId && a.ScheduledDate.Year == fiscalYear)
            .CountAsync();

        return $"Durante el ano fiscal {fiscalYear} se realizaron {assemblies} asambleas. " +
               $"Las decisiones y detalles se encuentran en el reporte de asambleas anexo.";
    }

    [HttpGet("annual/status")]
    public async Task<ActionResult<AnnualReportStatusDto>> GetAnnualReportStatus()
    {
        var tenantId = GetTenantId();

        var sections = await _context.ManagementReportSections
            .Where(s => s.TenantId == tenantId)
            .ToListAsync();

        var total = sections.Count;
        var autoGenerated = sections.Count(s => s.Status == SectionStatus.AutoGenerated);
        var manuallyEdited = sections.Count(s => s.Status == SectionStatus.ManuallyEdited);
        var pending = sections.Count(s => s.Status == SectionStatus.Pending);
        var completion = total > 0 ? (double)(autoGenerated + manuallyEdited) / total * 100 : 0;

        var lastConsolidated = await _context.GeneratedReports
            .Where(r => r.TenantId == tenantId && r.ReportType!.ReportTypeCode == ReportTypeEnum.AnnualManagementReport)
            .OrderByDescending(r => r.GeneratedAt)
            .Select(r => (DateTime?)r.GeneratedAt)
            .FirstOrDefaultAsync();

        return Ok(new AnnualReportStatusDto
        {
            TotalSections = total,
            AutoGeneratedSections = autoGenerated,
            ManuallyEditedSections = manuallyEdited,
            PendingSections = pending,
            CompletionPercentage = Math.Round(completion, 1),
            LastConsolidatedAt = lastConsolidated
        });
    }

    [HttpPost("annual/consolidate")]
    public async Task<ActionResult<GeneratedReportDto>> ConsolidateAnnualReport([FromBody] ConsolidateAnnualReportRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var reportType = await _context.ReportTypes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.ReportTypeCode == ReportTypeEnum.AnnualManagementReport);

        if (reportType is null)
            return BadRequest("Tipo de reporte no encontrado.");

        var result = await _pdfEngine.GenerateAnnualReportPdfAsync(
            tenantId, reportType.Id.ToString(), userId, request.FiscalYear);

        return Ok(new GeneratedReportDto
        {
            Id = result.Id,
            ReportTypeId = result.ReportTypeId,
            ReportTypeCode = "AnnualManagementReport",
            ReportTypeName = "Informe de Gestion Anual",
            Format = result.Format.ToString(),
            PeriodFrom = result.PeriodFrom,
            PeriodTo = result.PeriodTo,
            FileName = result.FileName,
            FileSizeBytes = result.FileSizeBytes,
            GeneratedByUserId = result.GeneratedByUserId,
            GeneratedAt = result.GeneratedAt,
            Parameters = result.Parameters,
            Notes = result.Notes,
            RecurringConfigId = result.RecurringConfigId,
            ConsecutiveNumber = result.ConsecutiveNumber
        });
    }

    [HttpPost("accountant-export")]
    public async Task<IActionResult> GenerateAccountantExport([FromBody] AccountantExportRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;

        if (!_accessControl.CanAccessReport(role, "AccountantExport"))
            return Forbid();

        var result = await _excelEngine.GenerateFinancialExportAsync(tenantId, userId, request.PeriodFrom, request.PeriodTo);

        var fileBytes = await System.IO.File.ReadAllBytesAsync(result.FilePath);

        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.FileName);
    }

    [HttpGet("template")]
    public async Task<ActionResult> GetGlobalTemplate()
    {
        var tenantId = GetTenantId();

        var template = await _context.PDFTemplates
            .Where(t => t.TenantId == tenantId && t.IsGlobal)
            .FirstOrDefaultAsync();

        if (template is null)
            return Ok(new
            {
                headerText = "Propiedad Horizontal",
                footerText = "Documento generado por el sistema de gestion",
                signatureName = "Administrador",
                signatureRole = "Administrador",
                confidentialityNote = "ESTE DOCUMENTO CONTIENE DATOS PERSONALES PROTEGIDOS POR LA LEY 1581 DE 2012",
                disclaimerNote = "Los datos aqui contenidos corresponden al momento de generacion y pueden diferir de los datos actuales si se han registrado movimientos posteriores.",
                primaryColor = "#059669",
                secondaryColor = "#1e293b"
            });

        return Ok(new
        {
            id = template.Id,
            logoFilePath = template.LogoFilePath,
            headerText = template.HeaderText,
            footerText = template.FooterText,
            signatureName = template.SignatureName,
            signatureRole = template.SignatureRole,
            confidentialityNote = template.ConfidentialityNote,
            disclaimerNote = template.DisclaimerNote,
            primaryColor = template.PrimaryColor,
            secondaryColor = template.SecondaryColor
        });
    }

    [HttpPut("template")]
    public async Task<IActionResult> UpdateGlobalTemplate([FromBody] UpdateGlobalTemplateDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();

        var template = await _context.PDFTemplates
            .Where(t => t.TenantId == tenantId && t.IsGlobal)
            .FirstOrDefaultAsync();

        if (template is null)
        {
            template = new PDFTemplate
            {
                TenantId = tenantId,
                LogoFilePath = request.LogoFilePath,
                HeaderText = request.HeaderText ?? "Propiedad Horizontal",
                FooterText = request.FooterText ?? "",
                SignatureName = request.SignatureName ?? "Administrador",
                SignatureRole = request.SignatureRole ?? "Administrador",
                ConfidentialityNote = request.ConfidentialityNote,
                DisclaimerNote = request.DisclaimerNote ?? "Los datos aqui contenidos corresponden al momento de generacion y pueden diferir de los datos actuales si se han registrado movimientos posteriores.",
                PrimaryColor = request.PrimaryColor ?? "#059669",
                SecondaryColor = request.SecondaryColor ?? "#1e293b",
                IsGlobal = true,
                CreatedByUserId = userId
            };
            _context.PDFTemplates.Add(template);
        }
        else
        {
            if (request.LogoFilePath is not null)
                template.LogoFilePath = request.LogoFilePath;
            if (request.HeaderText is not null)
                template.HeaderText = request.HeaderText;
            if (request.FooterText is not null)
                template.FooterText = request.FooterText;
            if (request.SignatureName is not null)
                template.SignatureName = request.SignatureName;
            if (request.SignatureRole is not null)
                template.SignatureRole = request.SignatureRole;
            if (request.ConfidentialityNote is not null)
                template.ConfidentialityNote = request.ConfidentialityNote;
            if (request.DisclaimerNote is not null)
                template.DisclaimerNote = request.DisclaimerNote;
            if (request.PrimaryColor is not null)
                template.PrimaryColor = request.PrimaryColor;
            if (request.SecondaryColor is not null)
                template.SecondaryColor = request.SecondaryColor;
            template.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return Ok();
    }

    private static DateTime CalculateNextExecution(ReportFrequency frequency, DateTime from)
    {
        return frequency switch
        {
            ReportFrequency.Weekly => from.AddDays(7),
            ReportFrequency.Biweekly => from.AddDays(14),
            ReportFrequency.Monthly => from.AddMonths(1),
            ReportFrequency.Quarterly => from.AddMonths(3),
            ReportFrequency.Annual => from.AddYears(1),
            _ => from.AddMonths(1)
        };
    }
}

public class UpdateGlobalTemplateDto
{
    public string? LogoFilePath { get; set; }
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }
    public string? SignatureName { get; set; }
    public string? SignatureRole { get; set; }
    public string? ConfidentialityNote { get; set; }
    public string? DisclaimerNote { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
}
