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
[Authorize]
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

    private string GetUserRole()
    {
        return User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
    }

    // ── Catálogo de reportes ─────────────────────────────────────

    [HttpGet("catalog")]
    public async Task<ActionResult<List<ReportCatalogItemDto>>> GetCatalog()
    {
        var tenantId = GetTenantId();
        var role = GetUserRole();

        var types = await _accessControl.GetFilteredCatalogAsync(tenantId, role);

        var catalog = types.Select(t => new ReportCatalogItemDto
        {
            Code = t.ReportTypeCode,
            Name = t.Name,
            Description = t.Description,
            Category = t.Category,
            ContainsPersonalData = t.ContainsPersonalData,
            AvailableFormats = new List<string> { "Pdf", "Excel" }
        }).ToList();

        return Ok(catalog);
    }

    // ── Generar reporte ──────────────────────────────────────────

    [HttpPost("generate")]
    public async Task<ActionResult<GeneratedReportDto>> GenerateReport([FromBody] GenerateReportRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var role = GetUserRole();

        if (!_accessControl.CanAccessReport(role, request.ReportTypeCode))
            return Forbid();

        if (!Enum.TryParse<ReportTypeEnum>(request.ReportTypeCode, out _))
            return BadRequest("Tipo de reporte invalido: " + request.ReportTypeCode);

        if (request.Format != "Pdf" && request.Format != "Excel")
            return BadRequest("Formato invalido. Use 'Pdf' o 'Excel'.");

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
            RecurringConfigId = result.RecurringConfigId
        });
    }

    // ── Historial de reportes generados ──────────────────────────

    [HttpGet("history")]
    public async Task<ActionResult<List<GeneratedReportDto>>> GetHistory(
        [FromQuery] string? reportTypeCode = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var tenantId = GetTenantId();
        var role = GetUserRole();

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
        var role = GetUserRole();

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

    // ── Reportes recurrentes ─────────────────────────────────────

    [HttpGet("recurring")]
    public async Task<ActionResult<List<RecurringReportConfigDto>>> GetRecurringConfigs()
    {
        var tenantId = GetTenantId();
        var role = GetUserRole();

        if (role != "Admin" && role != "SuperAdmin")
            return Forbid();

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
        var role = GetUserRole();

        if (role != "Admin" && role != "SuperAdmin")
            return Forbid();

        if (!Enum.TryParse<ReportTypeEnum>(request.ReportTypeCode, out var reportTypeCode))
            return BadRequest("Codigo de tipo de reporte invalido.");

        var reportType = await _context.ReportTypes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.ReportTypeCode == reportTypeCode);
        if (reportType is null)
            return BadRequest("Tipo de reporte no encontrado.");

        if (!Enum.TryParse<ReportFrequency>(request.Frequency, out var frequency))
            return BadRequest("Frecuencia invalida.");

        if (!Enum.TryParse<ReportFormat>(request.Format, out var format))
            return BadRequest("Formato invalido.");

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
        var role = GetUserRole();

        if (role != "Admin" && role != "SuperAdmin")
            return Forbid();

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
        var role = GetUserRole();

        if (role != "Admin" && role != "SuperAdmin")
            return Forbid();

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

    // ── Secciones del Informe de Gestion Anual ───────────────────

    [HttpGet("annual/sections")]
    public async Task<ActionResult<List<ManagementReportSectionDto>>> GetAnnualReportSections()
    {
        var tenantId = GetTenantId();
        var role = GetUserRole();

        if (role != "Admin" && role != "SuperAdmin")
            return Forbid();

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
        var role = GetUserRole();
        var userId = GetUserId();

        if (role != "Admin" && role != "SuperAdmin")
            return Forbid();

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
        var role = GetUserRole();

        if (role != "Admin" && role != "SuperAdmin")
            return Forbid();

        var section = await _context.ManagementReportSections
            .FirstOrDefaultAsync(s => s.Id == id && s.TenantId == tenantId);

        if (section is null)
            return NotFound("Seccion no encontrada.");

        if (string.IsNullOrEmpty(section.AutoGeneratedQuery))
            return BadRequest("Esta seccion no tiene configurada una consulta de auto-generacion.");

        section.Content = "Seccion regenerada automaticamente en " + DateTime.UtcNow.ToString("dd/MMM/yyyy", CultureInfo.GetCultureInfo("es-CO"));
        section.Status = SectionStatus.AutoGenerated;
        section.LastAutoGeneratedAt = DateTime.UtcNow;
        section.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("annual/status")]
    public async Task<ActionResult<AnnualReportStatusDto>> GetAnnualReportStatus()
    {
        var tenantId = GetTenantId();
        var role = GetUserRole();

        if (role != "Admin" && role != "SuperAdmin")
            return Forbid();

        var sections = await _context.ManagementReportSections
            .Where(s => s.TenantId == tenantId)
            .ToListAsync();

        var total = sections.Count;
        var autoGenerated = sections.Count(s => s.Status == SectionStatus.AutoGenerated);
        var manuallyEdited = sections.Count(s => s.Status == SectionStatus.ManuallyEdited);
        var pending = sections.Count(s => s.Status == SectionStatus.Pending);
        var completion = total > 0 ? (double)(autoGenerated + manuallyEdited) / total * 100 : 0;

        return Ok(new AnnualReportStatusDto
        {
            TotalSections = total,
            AutoGeneratedSections = autoGenerated,
            ManuallyEditedSections = manuallyEdited,
            PendingSections = pending,
            CompletionPercentage = Math.Round(completion, 1)
        });
    }

    [HttpPost("annual/consolidate")]
    public async Task<ActionResult<GeneratedReportDto>> ConsolidateAnnualReport([FromBody] ConsolidateAnnualReportRequestDto request)
    {
        var tenantId = GetTenantId();
        var userId = GetUserId();
        var role = GetUserRole();

        if (role != "Admin" && role != "SuperAdmin")
            return Forbid();

        var sections = await _context.ManagementReportSections
            .Where(s => s.TenantId == tenantId)
            .OrderBy(s => s.SectionOrder)
            .ToListAsync();

        var tenantConfig = await _context.TenantConfigurations.FirstOrDefaultAsync(tc => tc.TenantId == tenantId);

        var periodFrom = new DateTime(request.FiscalYear, 1, 1);
        var periodTo = new DateTime(request.FiscalYear, 12, 31);

        var result = await _pdfEngine.GenerateReportAsync(
            tenantId, "AnnualManagementReport", "Pdf", userId,
            periodFrom, periodTo, null, "Informe de Gestion Anual " + request.FiscalYear);

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
            RecurringConfigId = result.RecurringConfigId
        });
    }

    // ── PDF Templates ────────────────────────────────────────────

    [HttpGet("templates")]
    public async Task<ActionResult<List<PDFTemplateDto>>> GetTemplates()
    {
        var tenantId = GetTenantId();
        var role = GetUserRole();

        if (role != "Admin" && role != "SuperAdmin")
            return Forbid();

        var templates = await _context.PDFTemplates
            .Where(t => t.TenantId == tenantId)
            .Join(_context.ReportTypes,
                t => t.ReportTypeCode,
                rt => rt.ReportTypeCode.ToString(),
                (t, rt) => new { Template = t, ReportTypeName = rt.Name })
            .OrderBy(x => x.ReportTypeName)
            .ToListAsync();

        return Ok(templates.Select(x => new PDFTemplateDto
        {
            Id = x.Template.Id,
            ReportTypeCode = x.Template.ReportTypeCode,
            ReportTypeName = x.ReportTypeName,
            LogoFilePath = x.Template.LogoFilePath,
            HeaderText = x.Template.HeaderText,
            FooterText = x.Template.FooterText,
            SignatureName = x.Template.SignatureName,
            SignatureRole = x.Template.SignatureRole,
            ConfidentialityNote = x.Template.ConfidentialityNote,
            DisclaimerNote = x.Template.DisclaimerNote,
            PrimaryColor = x.Template.PrimaryColor,
            SecondaryColor = x.Template.SecondaryColor,
            IsDefault = x.Template.IsDefault
        }).ToList());
    }

    [HttpPut("templates/{id}")]
    public async Task<ActionResult<PDFTemplateDto>> UpdateTemplate(Guid id, [FromBody] UpdatePDFTemplateDto request)
    {
        var tenantId = GetTenantId();
        var role = GetUserRole();

        if (role != "Admin" && role != "SuperAdmin")
            return Forbid();

        var template = await _context.PDFTemplates
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

        if (template is null)
            return NotFound("Plantilla no encontrada.");

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
        if (request.IsDefault.HasValue)
            template.IsDefault = request.IsDefault.Value;

        template.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new PDFTemplateDto
        {
            Id = template.Id,
            ReportTypeCode = template.ReportTypeCode,
            LogoFilePath = template.LogoFilePath,
            HeaderText = template.HeaderText,
            FooterText = template.FooterText,
            SignatureName = template.SignatureName,
            SignatureRole = template.SignatureRole,
            ConfidentialityNote = template.ConfidentialityNote,
            DisclaimerNote = template.DisclaimerNote,
            PrimaryColor = template.PrimaryColor,
            SecondaryColor = template.SecondaryColor,
            IsDefault = template.IsDefault
        });
    }

    // ── Helper ───────────────────────────────────────────────────

    private static DateTime CalculateNextExecution(ReportFrequency frequency, DateTime from)
    {
        return frequency switch
        {
            ReportFrequency.Daily => from.AddDays(1),
            ReportFrequency.Weekly => from.AddDays(7),
            ReportFrequency.Monthly => from.AddMonths(1),
            ReportFrequency.Quarterly => from.AddMonths(3),
            ReportFrequency.Annual => from.AddYears(1),
            _ => from.AddMonths(1)
        };
    }
}
