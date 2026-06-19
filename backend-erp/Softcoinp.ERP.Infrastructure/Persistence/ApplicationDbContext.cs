using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Enums;
using Softcoinp.ERP.Domain.Interfaces;

namespace Softcoinp.ERP.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context con multi-tenant, Identity y
/// soporte completo para el módulo de Autenticación y Roles.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<User>
{
    private readonly ITenantResolver _tenantResolver;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantResolver tenantResolver)
        : base(options)
    {
        _tenantResolver = tenantResolver;
    }

    // ── Módulo Financiero (existente) ────────────────────────────────
    public DbSet<FinancialRecord> FinancialRecords => Set<FinancialRecord>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<AccountingAccount> AccountingAccounts => Set<AccountingAccount>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();

    // ── Módulo Auth & Roles (existente) ──────────────────────────────
    public DbSet<UserTenantRole> UserTenantRoles => Set<UserTenantRole>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<AccessAuditLog> AccessAuditLogs => Set<AccessAuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // ── Módulo Configuración de Conjunto (nuevo) ─────────────────────
    public DbSet<TenantConfiguration> TenantConfigurations => Set<TenantConfiguration>();
    public DbSet<ConfigurationAuditLog> ConfigurationAuditLogs => Set<ConfigurationAuditLog>();
    public DbSet<LegalRepresentativeHistory> LegalRepresentativeHistories => Set<LegalRepresentativeHistory>();
    public DbSet<TenantDocument> TenantDocuments => Set<TenantDocument>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _tenantResolver.GetConnectionStringAsync().GetAwaiter().GetResult();
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__MasterConnection")
                                 ?? "Server=127.0.0.1;Port=3307;Database=erp_master;User=root;Password=1234;";
            }
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Entidades financieras (existentes) ───────────────────────
        modelBuilder.Entity<AccountingAccount>(entity =>
        {
            entity.ToTable("erp_accounting_accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<FinancialTransaction>(entity =>
        {
            entity.ToTable("erp_financial_transactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Reference).HasMaxLength(200);
            entity.HasIndex(e => e.ExternalUnitId);
        });

        modelBuilder.Entity<FinancialRecord>(entity =>
        {
            entity.ToTable("erp_financial_records");
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Provider>(entity =>
        {
            entity.ToTable("erp_providers");
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ── UserTenantRole ───────────────────────────────────────────
        modelBuilder.Entity<UserTenantRole>(entity =>
        {
            entity.ToTable("erp_user_tenant_roles");
            entity.HasKey(e => e.Id);

            // Un usuario solo puede tener un rol activo por tenant
            entity.HasIndex(e => new { e.UserId, e.TenantId }).IsUnique();

            entity.Property(e => e.Role)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.AssignedByUserId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.TenantRoles)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.AssignedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Invitation ───────────────────────────────────────────────
        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.ToTable("erp_invitations");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasIndex(e => new { e.Email, e.TenantId, e.Status });

            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.CreatedByUser)
                  .WithMany(u => u.SentInvitations)
                  .HasForeignKey(e => e.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AcceptedByUser)
                  .WithMany()
                  .HasForeignKey(e => e.AcceptedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── AccessAuditLog (INMUTABLE) ───────────────────────────────
        modelBuilder.Entity<AccessAuditLog>(entity =>
        {
            entity.ToTable("erp_access_audit_log");
            entity.HasKey(e => e.Id);

            // Índices para búsquedas frecuentes de auditoría
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.TenantId, e.Timestamp });
            entity.HasIndex(e => e.EventType);

            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.EventType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45); // IPv6 max length
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Details).HasMaxLength(2000);
            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.AuditLogs)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── RefreshToken ─────────────────────────────────────────────
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("erp_refresh_tokens");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.IsRevoked });

            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(64);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.ReplacedByTokenHash).HasMaxLength(64);
            entity.Property(e => e.CreatedFromIp).HasMaxLength(45);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── User — campos de seguridad adicionales ───────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.SuspendedReason).HasMaxLength(500);
            entity.Property(e => e.DailyLockoutResetDate).HasColumnType("date");
        });
        // ── TenantConfiguration ──────────────────────────────────────────
        modelBuilder.Entity<TenantConfiguration>(entity =>
        {
            entity.ToTable("erp_tenant_configuration");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OfficialName).HasMaxLength(200);
            entity.Property(e => e.Nit).HasMaxLength(20).IsRequired();
            entity.Property(e => e.VerificationDigit).HasMaxLength(1);
            
            entity.Property(e => e.LatePaymentInterestRate).HasColumnType("decimal(5,2)");
            entity.Property(e => e.MaxLegalInterestRate).HasColumnType("decimal(5,2)");
            entity.Property(e => e.AnnualBudget).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ContingencyFundPercentage).HasColumnType("decimal(5,2)");
        });

        // ── ConfigurationAuditLog ────────────────────────────────────────
        modelBuilder.Entity<ConfigurationAuditLog>(entity =>
        {
            entity.ToTable("erp_configuration_audit_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangedByUserId).HasMaxLength(450);
            entity.Property(e => e.ParameterName).HasMaxLength(100);
        });

        // ── LegalRepresentativeHistory ───────────────────────────────────
        modelBuilder.Entity<LegalRepresentativeHistory>(entity =>
        {
            entity.ToTable("erp_legal_representative_history");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.IdentificationDocument).HasMaxLength(50);
            entity.Property(e => e.RecordedByUserId).HasMaxLength(450);
        });

        // ── TenantDocument ───────────────────────────────────────────────
        modelBuilder.Entity<TenantDocument>(entity =>
        {
            entity.ToTable("erp_tenant_documents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.UploadedByUserId).HasMaxLength(450);
        });
    }

    /// <summary>
    /// Sobrescribe SaveChangesAsync para manejar timestamps de auditoría.
    /// IMPORTANTE: No actualiza AccessAuditLog (es inmutable por diseño).
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
            }
        }

        // Protección adicional en aplicación: rechazar UPDATE/DELETE en AccessAuditLog
        var auditLogMutations = ChangeTracker
            .Entries<AccessAuditLog>()
            .Where(e => e.State == EntityState.Modified || e.State == EntityState.Deleted);

        if (auditLogMutations.Any())
        {
            throw new InvalidOperationException(
                "La tabla de auditoría de accesos es inmutable. " +
                "No se permiten operaciones de UPDATE o DELETE.");
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
