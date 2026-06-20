using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.WebAPI.DTOs;

namespace Softcoinp.ERP.WebAPI.Controllers;

[ApiController]
[Route("api/accounting-accounts")]
[Authorize]
public class AccountingAccountsController : BaseController
{
    private readonly ApplicationDbContext _context;

    public AccountingAccountsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetAccounts()
    {
        var tenantId = GetTenantId();
        var accounts = await _context.AccountingAccounts
            .Where(a => a.TenantId == tenantId)
            .OrderBy(a => a.Code)
            .Select(a => new AccountingAccountDto
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                Category = a.Category.ToString(),
                Nature = a.Nature.ToString(),
                IsGroup = a.IsGroup,
                IsActive = a.IsActive,
                IsOfficialStandard = a.IsOfficialStandard
            })
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CreateAuxiliaryAccount([FromBody] CreateAuxiliaryAccountRequestDto request)
    {
        var tenantId = GetTenantId();

        if (string.IsNullOrWhiteSpace(request.ParentCode) || string.IsNullOrWhiteSpace(request.SubCode) || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("El código padre, el subcódigo y el nombre son campos obligatorios.");
        }

        // 1. Obtener la cuenta padre
        var parentAccount = await _context.AccountingAccounts
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.Code == request.ParentCode);

        if (parentAccount == null)
        {
            return NotFound($"No se encontró la cuenta padre con código {request.ParentCode}.");
        }

        // 2. Validar nivel y longitud de jerarquía
        // Nivel 3 (4 dígitos) -> Hijo es Nivel 4 (6 dígitos)
        // Nivel 4 (6 dígitos) -> Hijo es Nivel 5 (8 dígitos)
        int parentLen = parentAccount.Code.Length;
        int expectedSubCodeLen = 2;

        if (parentLen != 4 && parentLen != 6)
        {
            return BadRequest("Regla de jerarquía: Solo se pueden agregar cuentas auxiliares de nivel 4 (bajo cuentas de 4 dígitos) y nivel 5 (bajo cuentas de 6 dígitos).");
        }

        if (request.SubCode.Length != expectedSubCodeLen)
        {
            return BadRequest($"El subcódigo debe tener exactamente {expectedSubCodeLen} caracteres (ej. '01', '02').");
        }

        // Validar que el subcódigo sea numérico
        if (!request.SubCode.All(char.IsDigit))
        {
            return BadRequest("El subcódigo debe contener únicamente números.");
        }

        string childCode = parentAccount.Code + request.SubCode;

        // 3. Validar unicidad del código
        var codeExists = await _context.AccountingAccounts
            .AnyAsync(a => a.TenantId == tenantId && a.Code == childCode);

        if (codeExists)
        {
            return Conflict($"El código de cuenta contable {childCode} ya se encuentra registrado en el conjunto.");
        }

        // 4. Construir la cuenta auxiliar (hereda categoría y naturaleza de la cuenta padre)
        var newAccount = new AccountingAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = childCode,
            Name = request.Name,
            Category = parentAccount.Category,
            Nature = parentAccount.Nature,
            IsGroup = request.IsGroup,
            IsActive = true,
            IsOfficialStandard = false
        };

        _context.AccountingAccounts.Add(newAccount);
        await _context.SaveChangesAsync();

        var dto = new AccountingAccountDto
        {
            Id = newAccount.Id,
            Code = newAccount.Code,
            Name = newAccount.Name,
            Category = newAccount.Category.ToString(),
            Nature = newAccount.Nature.ToString(),
            IsGroup = newAccount.IsGroup,
            IsActive = newAccount.IsActive,
            IsOfficialStandard = newAccount.IsOfficialStandard
        };

        return CreatedAtAction(nameof(GetAccounts), dto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateAccountingAccountRequestDto request)
    {
        var tenantId = GetTenantId();
        
        var account = await _context.AccountingAccounts
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

        if (account == null)
        {
            return NotFound("No se encontró la cuenta contable.");
        }

        // Regla de Negocio: Las cuentas del estándar de la Resolución 029 no pueden modificarse.
        if (account.IsOfficialStandard)
        {
            return BadRequest("Regla de negocio: Las cuentas oficiales del estándar de la Resolución 029 no pueden ser modificadas.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("El nombre de la cuenta no puede estar vacío.");
        }

        account.Name = request.Name;
        account.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new AccountingAccountDto
        {
            Id = account.Id,
            Code = account.Code,
            Name = account.Name,
            Category = account.Category.ToString(),
            Nature = account.Nature.ToString(),
            IsGroup = account.IsGroup,
            IsActive = account.IsActive,
            IsOfficialStandard = account.IsOfficialStandard
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        var tenantId = GetTenantId();

        var account = await _context.AccountingAccounts
            .FirstOrDefaultAsync(a => a.Id == id && a.TenantId == tenantId);

        if (account == null)
        {
            return NotFound("No se encontró la cuenta contable.");
        }

        // Regla de Negocio: Las cuentas oficiales del estándar no pueden eliminarse.
        if (account.IsOfficialStandard)
        {
            return BadRequest("Regla de negocio: Las cuentas oficiales del estándar de la Resolución 029 no pueden ser eliminadas.");
        }

        // Regla de Negocio: No se puede eliminar una cuenta que ya tiene movimientos en el diario o está vinculada al presupuesto
        var hasJournalEntries = await _context.EntryLines
            .AnyAsync(e => e.AccountingAccountId == id);

        if (hasJournalEntries)
        {
            return BadRequest("No se puede eliminar la cuenta contable porque registra movimientos en el libro diario.");
        }

        var hasBudgetDetails = await _context.BudgetDetails
            .AnyAsync(d => d.AccountingAccountId == id);

        if (hasBudgetDetails)
        {
            return BadRequest("No se puede eliminar la cuenta contable porque está asignada a un presupuesto aprobado.");
        }

        var hasBudgetMovements = await _context.BudgetMovements
            .AnyAsync(m => m.SourceAccountId == id || m.DestinationAccountId == id);

        if (hasBudgetMovements)
        {
            return BadRequest("No se puede eliminar la cuenta contable porque está vinculada a traslados o adiciones presupuestales.");
        }

        _context.AccountingAccounts.Remove(account);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
