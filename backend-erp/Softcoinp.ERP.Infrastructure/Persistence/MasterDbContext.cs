using Microsoft.EntityFrameworkCore;
using Softcoinp.ERP.Domain.Interfaces;

namespace Softcoinp.ERP.Infrastructure.Persistence;

/// <summary>
/// Database context for managing global tenant metadata.
/// </summary>
public class MasterDbContext : DbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("erp_master_tenants");
            entity.HasIndex(e => e.Subdomain).IsUnique();
        });
    }
}
