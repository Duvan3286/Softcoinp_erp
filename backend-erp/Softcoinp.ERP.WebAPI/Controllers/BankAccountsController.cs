using System;
using System.Linq;
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
[Route("api/bank-accounts")]
[Authorize]
public class BankAccountsController : BaseController
{
    private readonly ApplicationDbContext _context;

    public BankAccountsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetAccounts([FromQuery] bool includeInactive = false)
    {
        var tenantId = GetTenantId();
        var query = _context.BankAccounts
            .Where(b => b.TenantId == tenantId);

        if (!includeInactive)
            query = query.Where(b => b.IsActive);

        var accounts = await query
            .OrderBy(b => b.BankName)
            .ThenBy(b => b.AccountNumber)
            .Select(b => new BankAccountDto
            {
                Id = b.Id,
                BankName = b.BankName,
                AccountNumber = b.AccountNumber,
                AccountType = b.AccountType.ToString(),
                CurrentBalance = b.CurrentBalance,
                OpeningBalance = b.OpeningBalance,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,Council,Auditor")]
    public async Task<IActionResult> GetAccount(Guid id)
    {
        var tenantId = GetTenantId();
        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

        if (account == null)
            return NotFound(new { message = "Cuenta bancaria no encontrada." });

        return Ok(new BankAccountDto
        {
            Id = account.Id,
            BankName = account.BankName,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType.ToString(),
            CurrentBalance = account.CurrentBalance,
            OpeningBalance = account.OpeningBalance,
            IsActive = account.IsActive,
            CreatedAt = account.CreatedAt
        });
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateBankAccountDto dto)
    {
        var tenantId = GetTenantId();

        if (string.IsNullOrWhiteSpace(dto.BankName))
            return BadRequest("El nombre del banco es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.AccountNumber))
            return BadRequest("El número de cuenta es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.AccountType))
            return BadRequest("El tipo de cuenta es obligatorio.");
        if (!Enum.TryParse<BankAccountType>(dto.AccountType, true, out var accountType))
            return BadRequest("Tipo de cuenta inválido. Use: Checking o Savings.");

        var exists = await _context.BankAccounts
            .AnyAsync(b => b.TenantId == tenantId && b.AccountNumber == dto.AccountNumber);

        if (exists)
            return Conflict(new { message = "Ya existe una cuenta bancaria con ese número en este conjunto." });

        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BankName = dto.BankName,
            AccountNumber = dto.AccountNumber,
            AccountType = accountType,
            CurrentBalance = dto.OpeningBalance,
            OpeningBalance = dto.OpeningBalance,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, new BankAccountDto
        {
            Id = account.Id,
            BankName = account.BankName,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType.ToString(),
            CurrentBalance = account.CurrentBalance,
            OpeningBalance = account.OpeningBalance,
            IsActive = account.IsActive,
            CreatedAt = account.CreatedAt
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant")]
    public async Task<IActionResult> UpdateAccount(Guid id, [FromBody] UpdateBankAccountDto dto)
    {
        var tenantId = GetTenantId();

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

        if (account == null)
            return NotFound(new { message = "Cuenta bancaria no encontrada." });

        if (string.IsNullOrWhiteSpace(dto.BankName))
            return BadRequest("El nombre del banco es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.AccountNumber))
            return BadRequest("El número de cuenta es obligatorio.");
        if (!Enum.TryParse<BankAccountType>(dto.AccountType, true, out var accountType))
            return BadRequest("Tipo de cuenta inválido. Use: Checking o Savings.");

        var duplicate = await _context.BankAccounts
            .AnyAsync(b => b.TenantId == tenantId && b.AccountNumber == dto.AccountNumber && b.Id != id);

        if (duplicate)
            return Conflict(new { message = "Ya existe otra cuenta bancaria con ese número en este conjunto." });

        account.BankName = dto.BankName;
        account.AccountNumber = dto.AccountNumber;
        account.AccountType = accountType;
        account.OpeningBalance = dto.OpeningBalance;
        account.IsActive = dto.IsActive;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Cuenta bancaria actualizada exitosamente." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        var tenantId = GetTenantId();

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId);

        if (account == null)
            return NotFound(new { message = "Cuenta bancaria no encontrada." });

        var hasMovements = await _context.BankMovements
            .AnyAsync(m => m.BankAccountId == id && m.TenantId == tenantId);

        if (hasMovements)
            return BadRequest(new { message = "No se puede eliminar la cuenta porque tiene movimientos asociados. Desactívela en su lugar." });

        account.IsActive = false;
        account.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Cuenta bancaria desactivada exitosamente." });
    }
}
