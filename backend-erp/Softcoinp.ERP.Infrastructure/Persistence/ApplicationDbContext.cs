using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Common;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Interfaces;

namespace Softcoinp.ERP.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for the ERP system with Multi-tenant support and Identity.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<User>
{
    private readonly ITenantResolver _tenantResolver;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantResolver tenantResolver) 
        : base(options) 
    {
        _tenantResolver = tenantResolver;
    }

    public DbSet<FinancialRecord> FinancialRecords => Set<FinancialRecord>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<AccountingAccount> AccountingAccounts => Set<AccountingAccount>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _tenantResolver.GetConnectionStringAsync().GetAwaiter().GetResult();
            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback for design-time (migrations) using local root
                connectionString = "Server=localhost;Port=3306;Database=temp_erp;User=root;Password=1234;"; 
            }
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // AccountingAccount Configuration
        modelBuilder.Entity<AccountingAccount>(entity =>
        {
            entity.ToTable("erp_accounting_accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // FinancialTransaction Configuration
        modelBuilder.Entity<FinancialTransaction>(entity =>
        {
            entity.ToTable("erp_financial_transactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Reference).HasMaxLength(200);
            entity.HasIndex(e => e.ExternalUnitId); // Speed up searches by external unit
        });

        // FinancialRecord Configuration
        modelBuilder.Entity<FinancialRecord>(entity =>
        {
            entity.ToTable("erp_financial_records");
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Provider Configuration
        modelBuilder.Entity<Provider>(entity =>
        {
            entity.ToTable("erp_providers");
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }

    /// <summary>
    /// Overrides SaveChangesAsync to automatically handle audit timestamps.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            ((BaseEntity)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                ((BaseEntity)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
