using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Domain.Utils;
using Softcoinp.ERP.Infrastructure.Persistence;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/tenant-config")]
[Authorize]
public class TenantConfigController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TenantConfigController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Council")]
    public async Task<ActionResult<TenantConfigurationDto>> Get()
    {
        var config = await _context.TenantConfigurations.FirstOrDefaultAsync();
        if (config == null)
            return NotFound("La configuración del conjunto no ha sido inicializada.");

        return Ok(MapToDto(config));
    }

    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<TenantConfigurationDto>> Update([FromBody] UpdateTenantConfigDto dto)
    {
        var config = await _context.TenantConfigurations.FirstOrDefaultAsync();
        bool isNew = false;
        
        if (config == null)
        {
            config = new TenantConfiguration();
            isNew = true;
        }

        // 1. Validaciones
        if (!NitValidator.IsValid(dto.Nit, dto.VerificationDigit))
            return BadRequest("El dígito de verificación no coincide con el NIT ingresado.");

        if (dto.BillingCycleDay < 1 || dto.BillingCycleDay > 28)
            return BadRequest("El día de corte debe estar entre el 1 y el 28.");

        // Validación manual: La tasa de mora no puede superar la tasa máxima legal vigente ingresada
        if (dto.LatePaymentInterestRate > dto.MaxLegalInterestRate)
            return BadRequest($"La tasa de interés de mora ({dto.LatePaymentInterestRate}%) no puede superar el límite legal ingresado ({dto.MaxLegalInterestRate}%).");

        if (dto.HasContingencyFund && dto.ContingencyFundPercentage < 1m)
            return BadRequest("Según la Ley 675, el fondo de imprevistos debe ser mínimo el 1% del presupuesto.");

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";

        // 2. Historial de Cambios Financieros (Auditoría)
        if (!isNew)
        {
            await AuditFinancialChange(config, dto, currentUserId);
        }

        // 3. Historial de Representantes Legales
        if (config.LegalRepresentativeId != dto.LegalRepresentativeId)
        {
            if (!isNew && !string.IsNullOrEmpty(config.LegalRepresentativeId))
            {
                var previousRep = await _context.LegalRepresentativeHistories
                    .Where(r => r.IdentificationDocument == config.LegalRepresentativeId && r.EndDate == null)
                    .FirstOrDefaultAsync();

                if (previousRep != null)
                {
                    previousRep.EndDate = DateTime.UtcNow;
                }
            }

            var newRep = new LegalRepresentativeHistory
            {
                FullName = dto.LegalRepresentativeName,
                IdentificationDocument = dto.LegalRepresentativeId,
                StartDate = DateTime.UtcNow,
                RecordedByUserId = currentUserId
            };
            _context.LegalRepresentativeHistories.Add(newRep);
        }

        // 4. Aplicar cambios
        config.OfficialName = dto.OfficialName;
        config.Nit = dto.Nit;
        config.VerificationDigit = dto.VerificationDigit;
        config.Address = dto.Address;
        config.Municipality = dto.Municipality;
        config.Department = dto.Department;
        config.Phone = dto.Phone;
        config.Email = dto.Email;
        config.RealEstateRegistration = dto.RealEstateRegistration;
        config.ConstitutionDate = dto.ConstitutionDate;
        config.LegalRepresentativeName = dto.LegalRepresentativeName;
        config.LegalRepresentativeDocumentType = dto.LegalRepresentativeDocumentType;
        config.LegalRepresentativeId = dto.LegalRepresentativeId;
        config.LegalRepresentativeDv = dto.LegalRepresentativeDv ?? string.Empty;

        config.BillingCycleDay = dto.BillingCycleDay;
        config.GracePeriodDays = dto.GracePeriodDays;
        config.LatePaymentInterestRate = dto.LatePaymentInterestRate;
        config.MaxLegalInterestRate = dto.MaxLegalInterestRate;
        config.FiscalYearStartMonth = dto.FiscalYearStartMonth;
        config.FiscalYearStartDay = dto.FiscalYearStartDay;
        config.AnnualBudget = dto.AnnualBudget;

        config.TotalUnits = dto.TotalUnits;
        config.TotalTowers = dto.TotalTowers;
        config.RoundingPolicy = dto.RoundingPolicy;
        config.MaxActiveExtraordinaryQuotas = dto.MaxActiveExtraordinaryQuotas;
        config.HasContingencyFund = dto.HasContingencyFund;
        config.ContingencyFundPercentage = dto.ContingencyFundPercentage;

        config.SenderEmail = dto.SenderEmail;
        config.SignatureFooterTemplate = dto.SignatureFooterTemplate;
        config.AutoSendLatePaymentNotifications = dto.AutoSendLatePaymentNotifications;
        config.LatePaymentNotificationFrequencyDays = dto.LatePaymentNotificationFrequencyDays;

        if (isNew)
            _context.TenantConfigurations.Add(config);

        await _context.SaveChangesAsync();

        return Ok(MapToDto(config));
    }

    [HttpPost("logo")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UploadLogo(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Archivo vacío.");

        if (file.Length > 2 * 1024 * 1024)
            return BadRequest("El archivo excede el tamaño máximo de 2MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".png" && ext != ".svg")
            return BadRequest("Solo se permiten archivos PNG o SVG.");

        var config = await _context.TenantConfigurations.FirstOrDefaultAsync();
        if (config == null)
            return BadRequest("Debe guardar la configuración general primero.");

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tenant");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"logo_{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        config.LogoUrl = $"/uploads/tenant/{fileName}";
        await _context.SaveChangesAsync();

        return Ok(new { config.LogoUrl });
    }

    [HttpGet("audit")]
    [Authorize(Roles = "SuperAdmin,Admin,Council")]
    public async Task<IActionResult> GetAuditLogs()
    {
        var logs = await _context.ConfigurationAuditLogs
            .OrderByDescending(l => l.Timestamp)
            .Take(100)
            .ToListAsync();
            
        return Ok(logs);
    }

    [HttpGet("representatives")]
    [Authorize(Roles = "SuperAdmin,Admin,Council")]
    public async Task<IActionResult> GetRepresentatives()
    {
        var reps = await _context.LegalRepresentativeHistories
            .OrderByDescending(r => r.StartDate)
            .ToListAsync();
            
        return Ok(reps);
    }

    [HttpPost("documents")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UploadDocument([FromForm] IFormFile file, [FromForm] Softcoinp.ERP.Domain.Entities.TenantDocumentType type, [FromForm] string title, [FromForm] AppRole minRole)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Archivo vacío.");

        if (file.ContentType != "application/pdf")
            return BadRequest("Solo se permiten archivos PDF.");

        // Límite de seguridad de 10 MB
        if (file.Length > 10 * 1024 * 1024)
            return BadRequest("El archivo excede el tamaño máximo permitido de 10MB.");

        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"doc_{Guid.NewGuid()}.pdf";
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";

        var doc = new TenantDocument
        {
            Title = title,
            Type = type,
            FilePath = filePath,
            ContentType = "application/pdf",
            FileSize = file.Length,
            MinimumRoleRequired = minRole,
            UploadedByUserId = currentUserId,
            UploadedAt = DateTime.UtcNow
        };

        _context.TenantDocuments.Add(doc);
        await _context.SaveChangesAsync();

        return Ok(new { doc.Id, doc.Title, doc.Type });
    }

    [HttpGet("documents")]
    [Authorize(Roles = "SuperAdmin,Admin,Council,Auditor")]
    public async Task<IActionResult> GetDocuments()
    {
        var roleClaim = User.FindFirstValue(ClaimTypes.Role) ?? "";
        AppRole userRole;
        Enum.TryParse(roleClaim, out userRole);

        // Retornar los documentos donde el rol del usuario es <= al rol mínimo requerido
        // SuperAdmin = 0, Admin = 1, Council = 2, Accountant = 3, Auditor = 4, Resident = 5
        var docs = await _context.TenantDocuments
            .Where(d => (int)userRole <= (int)d.MinimumRoleRequired)
            .Select(d => new { d.Id, d.Title, d.Type, d.UploadedAt, d.FileSize, d.MinimumRoleRequired })
            .ToListAsync();

        return Ok(docs);
    }

    [HttpGet("documents/{id}/download")]
    [Authorize(Roles = "SuperAdmin,Admin,Council,Auditor")]
    public async Task<IActionResult> DownloadDocument(Guid id)
    {
        var doc = await _context.TenantDocuments.FindAsync(id);
        if (doc == null)
            return NotFound();

        var roleClaim = User.FindFirstValue(ClaimTypes.Role) ?? "";
        AppRole userRole;
        Enum.TryParse(roleClaim, out userRole);

        if ((int)userRole > (int)doc.MinimumRoleRequired)
            return Forbid();

        if (!System.IO.File.Exists(doc.FilePath))
            return NotFound("El archivo físico no existe en el servidor.");

        var bytes = await System.IO.File.ReadAllBytesAsync(doc.FilePath);
        return File(bytes, doc.ContentType, Path.GetFileName(doc.FilePath));
    }

    private async Task AuditFinancialChange(TenantConfiguration oldConfig, UpdateTenantConfigDto newConfig, string userId)
    {
        if (oldConfig.LatePaymentInterestRate != newConfig.LatePaymentInterestRate)
            _context.ConfigurationAuditLogs.Add(CreateAudit("LatePaymentInterestRate", oldConfig.LatePaymentInterestRate, newConfig.LatePaymentInterestRate, userId));

        if (oldConfig.BillingCycleDay != newConfig.BillingCycleDay)
            _context.ConfigurationAuditLogs.Add(CreateAudit("BillingCycleDay", oldConfig.BillingCycleDay, newConfig.BillingCycleDay, userId));

        if (oldConfig.AnnualBudget != newConfig.AnnualBudget)
            _context.ConfigurationAuditLogs.Add(CreateAudit("AnnualBudget", oldConfig.AnnualBudget, newConfig.AnnualBudget, userId));

        if (oldConfig.MaxLegalInterestRate != newConfig.MaxLegalInterestRate)
            _context.ConfigurationAuditLogs.Add(CreateAudit("MaxLegalInterestRate", oldConfig.MaxLegalInterestRate, newConfig.MaxLegalInterestRate, userId));
    }

    private ConfigurationAuditLog CreateAudit(string paramName, object oldVal, object newVal, string userId)
    {
        return new ConfigurationAuditLog
        {
            ParameterName = paramName,
            OldValue = oldVal?.ToString() ?? "",
            NewValue = newVal?.ToString() ?? "",
            ChangedByUserId = userId
        };
    }

    private TenantConfigurationDto MapToDto(TenantConfiguration config)
    {
        return new TenantConfigurationDto
        {
            OfficialName = config.OfficialName,
            Nit = config.Nit,
            VerificationDigit = config.VerificationDigit,
            Address = config.Address,
            Municipality = config.Municipality,
            Department = config.Department,
            Phone = config.Phone,
            Email = config.Email,
            RealEstateRegistration = config.RealEstateRegistration,
            ConstitutionDate = config.ConstitutionDate,
            LegalRepresentativeName = config.LegalRepresentativeName,
            LegalRepresentativeDocumentType = config.LegalRepresentativeDocumentType,
            LegalRepresentativeId = config.LegalRepresentativeId,
            LegalRepresentativeDv = config.LegalRepresentativeDv,
            BillingCycleDay = config.BillingCycleDay,
            GracePeriodDays = config.GracePeriodDays,
            LatePaymentInterestRate = config.LatePaymentInterestRate,
            MaxLegalInterestRate = config.MaxLegalInterestRate,
            FiscalYearStartMonth = config.FiscalYearStartMonth,
            FiscalYearStartDay = config.FiscalYearStartDay,
            AnnualBudget = config.AnnualBudget,
            TotalUnits = config.TotalUnits,
            TotalTowers = config.TotalTowers,
            RoundingPolicy = config.RoundingPolicy,
            MaxActiveExtraordinaryQuotas = config.MaxActiveExtraordinaryQuotas,
            HasContingencyFund = config.HasContingencyFund,
            ContingencyFundPercentage = config.ContingencyFundPercentage,
            SenderEmail = config.SenderEmail,
            SignatureFooterTemplate = config.SignatureFooterTemplate,
            AutoSendLatePaymentNotifications = config.AutoSendLatePaymentNotifications,
            LatePaymentNotificationFrequencyDays = config.LatePaymentNotificationFrequencyDays,
            LogoUrl = config.LogoUrl
        };
    }
}

public class UpdateTenantConfigDto
{
    [Required(ErrorMessage = "El Nombre Oficial es obligatorio.")] 
    [StringLength(200, ErrorMessage = "El Nombre Oficial no puede superar los 200 caracteres.")]
    public string OfficialName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El NIT es obligatorio.")] 
    [StringLength(10, ErrorMessage = "El NIT no puede tener más de 10 dígitos.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "El NIT solo debe contener números.")]
    public string Nit { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El Dígito de Verificación es obligatorio.")] 
    [RegularExpression(@"^\d$", ErrorMessage = "El DV debe ser un solo dígito numérico.")]
    public string VerificationDigit { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La Dirección es obligatoria.")] 
    [StringLength(200, ErrorMessage = "La Dirección no puede superar los 200 caracteres.")]
    public string Address { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El Municipio es obligatorio.")] 
    [StringLength(100, ErrorMessage = "El Municipio no puede superar los 100 caracteres.")]
    public string Municipality { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El Departamento es obligatorio.")] 
    [StringLength(100, ErrorMessage = "El Departamento no puede superar los 100 caracteres.")]
    public string Department { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El Teléfono es obligatorio.")] 
    [StringLength(20, ErrorMessage = "El Teléfono no puede superar los 20 caracteres.")]
    [RegularExpression(@"^[0-9\-\+\s]+$", ErrorMessage = "El Teléfono contiene caracteres no permitidos.")]
    public string Phone { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El Correo Oficial es obligatorio.")] 
    [EmailAddress(ErrorMessage = "El formato del correo oficial es inválido.")]
    [StringLength(256, ErrorMessage = "El Correo no puede superar los 256 caracteres.")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La Matrícula Inmobiliaria es obligatoria.")] 
    [StringLength(50, ErrorMessage = "La Matrícula Inmobiliaria no puede superar los 50 caracteres.")]
    public string RealEstateRegistration { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La Fecha de Constitución es obligatoria.")] 
    public DateTime ConstitutionDate { get; set; }
    
    [Required(ErrorMessage = "El Nombre del Representante Legal es obligatorio.")] 
    [StringLength(200, ErrorMessage = "El Nombre del Representante no puede superar los 200 caracteres.")]
    public string LegalRepresentativeName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El Tipo de Documento del Representante Legal es obligatorio.")]
    public IdentityDocumentType LegalRepresentativeDocumentType { get; set; } = IdentityDocumentType.CC;
    
    [Required(ErrorMessage = "El Documento del Representante Legal es obligatorio.")] 
    [StringLength(50, ErrorMessage = "El Documento del Representante no puede superar los 50 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "El Documento solo puede contener números y letras.")]
    public string LegalRepresentativeId { get; set; } = string.Empty;

    [StringLength(1, ErrorMessage = "El DV del Representante Legal debe ser un solo dígito.")]
    [RegularExpression(@"^\d?$", ErrorMessage = "El DV debe ser un dígito numérico.")]
    public string? LegalRepresentativeDv { get; set; } = string.Empty;

    public int BillingCycleDay { get; set; }
    public int GracePeriodDays { get; set; }
    public decimal LatePaymentInterestRate { get; set; }
    public decimal MaxLegalInterestRate { get; set; }
    public int FiscalYearStartMonth { get; set; }
    public int FiscalYearStartDay { get; set; }
    public decimal AnnualBudget { get; set; }

    public int TotalUnits { get; set; }
    public int TotalTowers { get; set; }
    public RoundingPolicy RoundingPolicy { get; set; }
    public int MaxActiveExtraordinaryQuotas { get; set; }
    public bool HasContingencyFund { get; set; }
    public decimal ContingencyFundPercentage { get; set; }

    [Required(ErrorMessage = "El Correo de Remitente es obligatorio.")] 
    [EmailAddress(ErrorMessage = "El formato del correo de remitente es inválido.")]
    [StringLength(256, ErrorMessage = "El Correo no puede superar los 256 caracteres.")]
    public string SenderEmail { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "La Plantilla de Firma no puede superar los 1000 caracteres.")]
    public string SignatureFooterTemplate { get; set; } = string.Empty;
    public bool AutoSendLatePaymentNotifications { get; set; }
    public int LatePaymentNotificationFrequencyDays { get; set; }
}

public class TenantConfigurationDto : UpdateTenantConfigDto
{
    public string? LogoUrl { get; set; }
}
