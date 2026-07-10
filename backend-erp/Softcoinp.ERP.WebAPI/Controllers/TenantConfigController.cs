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
public class TenantConfigController : BaseController
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
        var tenantId = GetTenantId();
        var config = await _context.TenantConfigurations.FirstOrDefaultAsync(tc => tc.TenantId == tenantId);
        if (config == null)
            return NotFound("La configuración del conjunto no ha sido inicializada.");

        return Ok(MapToDto(config));
    }

    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<TenantConfigurationDto>> Update([FromBody] UpdateTenantConfigDto dto)
    {
        var tenantId = GetTenantId();
        var config = await _context.TenantConfigurations.FirstOrDefaultAsync(tc => tc.TenantId == tenantId);
        bool isNew = false;
        
        if (config == null)
        {
            config = new TenantConfiguration { TenantId = tenantId };
            isNew = true;
        }

        // 1. Validaciones
        if (!NitValidator.IsValid(dto.Nit, dto.VerificationDigit))
            return BadRequest("El dígito de verificación no coincide con el NIT ingresado.");

        if (dto.BillingCycleDay < 1 || dto.BillingCycleDay > 28)
            return BadRequest("El día de corte debe estar entre el 1 y el 28.");

        if (dto.HasContingencyFund && dto.ContingencyFundPercentage < 1m)
            return BadRequest("Según la Ley 675, el fondo de imprevistos debe ser mínimo el 1% del presupuesto.");

        if (dto.TotalUnits <= 0)
            return BadRequest("El total de unidades debe ser mayor a cero.");

        if (dto.TotalTowers <= 0)
            return BadRequest("El total de torres/bloques debe ser mayor a cero.");

        if (dto.FiscalYearStartDay < 1 || dto.FiscalYearStartDay > 31)
            return BadRequest("El día de inicio del año fiscal debe estar entre 1 y 31.");

        var currentUserId = GetUserId();
        var currentTenantId = GetTenantId();

        // 2. Historial de Cambios Financieros (Auditoría)
        if (!isNew)
        {
            await AuditFinancialChange(currentTenantId, config, dto, currentUserId);
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
    public async Task<IActionResult> UploadLogo([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Archivo vacío.");

        if (file.Length > 2 * 1024 * 1024)
            return BadRequest("El archivo excede el tamaño máximo de 2MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".png" && ext != ".svg")
            return BadRequest("Solo se permiten archivos PNG o SVG.");

        var tenantId = GetTenantId();
        var config = await _context.TenantConfigurations.FirstOrDefaultAsync(tc => tc.TenantId == tenantId);
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

        var relativePath = $"/uploads/documents/{fileName}";

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";

        var doc = new TenantDocument
        {
            Title = title,
            Type = type,
            FilePath = relativePath,
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

        var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", doc.FilePath.TrimStart('/'));
        if (!System.IO.File.Exists(physicalPath))
            return NotFound("El archivo físico no existe en el servidor.");

        return PhysicalFile(physicalPath, doc.ContentType, Path.GetFileName(doc.FilePath));
    }

    private Task AuditFinancialChange(string tenantId, TenantConfiguration oldConfig, UpdateTenantConfigDto newConfig, string userId)
    {
        if (oldConfig.BillingCycleDay != newConfig.BillingCycleDay)
            _context.ConfigurationAuditLogs.Add(CreateAudit(tenantId, "BillingCycleDay", oldConfig.BillingCycleDay, newConfig.BillingCycleDay, userId));

        if (oldConfig.AnnualBudget != newConfig.AnnualBudget)
            _context.ConfigurationAuditLogs.Add(CreateAudit(tenantId, "AnnualBudget", oldConfig.AnnualBudget, newConfig.AnnualBudget, userId));

        if (oldConfig.GracePeriodDays != newConfig.GracePeriodDays)
            _context.ConfigurationAuditLogs.Add(CreateAudit(tenantId, "GracePeriodDays", oldConfig.GracePeriodDays, newConfig.GracePeriodDays, userId));

        if (oldConfig.ContingencyFundPercentage != newConfig.ContingencyFundPercentage)
            _context.ConfigurationAuditLogs.Add(CreateAudit(tenantId, "ContingencyFundPercentage", oldConfig.ContingencyFundPercentage, newConfig.ContingencyFundPercentage, userId));

        if (oldConfig.HasContingencyFund != newConfig.HasContingencyFund)
            _context.ConfigurationAuditLogs.Add(CreateAudit(tenantId, "HasContingencyFund", oldConfig.HasContingencyFund, newConfig.HasContingencyFund, userId));

        if (oldConfig.RoundingPolicy != newConfig.RoundingPolicy)
            _context.ConfigurationAuditLogs.Add(CreateAudit(tenantId, "RoundingPolicy", oldConfig.RoundingPolicy, newConfig.RoundingPolicy, userId));

        return Task.CompletedTask;
    }

    private ConfigurationAuditLog CreateAudit(string tenantId, string paramName, object oldVal, object newVal, string userId)
    {
        return new ConfigurationAuditLog
        {
            TenantId = tenantId,
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
    [StringLength(15, ErrorMessage = "El NIT no puede tener más de 15 dígitos.")]
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

public class TenantConfigurationDto
{
    public string? LogoUrl { get; set; }
    public string OfficialName { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string VerificationDigit { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Municipality { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RealEstateRegistration { get; set; } = string.Empty;
    public DateTime ConstitutionDate { get; set; }
    public string LegalRepresentativeName { get; set; } = string.Empty;
    public IdentityDocumentType LegalRepresentativeDocumentType { get; set; } = IdentityDocumentType.CC;
    public string LegalRepresentativeId { get; set; } = string.Empty;
    public string LegalRepresentativeDv { get; set; } = string.Empty;
    public int BillingCycleDay { get; set; }
    public int GracePeriodDays { get; set; }
    public int FiscalYearStartMonth { get; set; }
    public int FiscalYearStartDay { get; set; }
    public decimal AnnualBudget { get; set; }
    public int TotalUnits { get; set; }
    public int TotalTowers { get; set; }
    public RoundingPolicy RoundingPolicy { get; set; }
    public int MaxActiveExtraordinaryQuotas { get; set; }
    public bool HasContingencyFund { get; set; }
    public decimal ContingencyFundPercentage { get; set; }
    public string SenderEmail { get; set; } = string.Empty;
    public string SignatureFooterTemplate { get; set; } = string.Empty;
    public bool AutoSendLatePaymentNotifications { get; set; }
    public int LatePaymentNotificationFrequencyDays { get; set; }
}
