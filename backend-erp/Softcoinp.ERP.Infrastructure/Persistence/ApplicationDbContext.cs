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
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetDetail> BudgetDetails => Set<BudgetDetail>();
    public DbSet<BudgetMovement> BudgetMovements => Set<BudgetMovement>();
    public DbSet<ContingencyFund> ContingencyFunds => Set<ContingencyFund>();
    public DbSet<ContingencyFundContribution> ContingencyFundContributions => Set<ContingencyFundContribution>();
    public DbSet<ContingencyFundUsage> ContingencyFundUsages => Set<ContingencyFundUsage>();
    public DbSet<AccountingEntry> AccountingEntries => Set<AccountingEntry>();
    public DbSet<AccountingPeriod> AccountingPeriods => Set<AccountingPeriod>();
    public DbSet<EntryLine> EntryLines => Set<EntryLine>();
    public DbSet<EntryReversal> EntryReversals => Set<EntryReversal>();

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

    // ── Módulo de Unidades (nuevo) ───────────────────────────────────
    public DbSet<UnitType> UnitTypes => Set<UnitType>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<UnitStateHistory> UnitStateHistories => Set<UnitStateHistory>();
    public DbSet<UnitComplement> UnitComplements => Set<UnitComplement>();
    public DbSet<BulkImportLog> BulkImportLogs => Set<BulkImportLog>();

    // ── Módulo de Residentes y Propietarios (nuevo) ──────────────────
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<UnitOwner> UnitOwners => Set<UnitOwner>();
    public DbSet<TenantResident> TenantResidents => Set<TenantResident>();
    public DbSet<CohabitationGroupMember> CohabitationGroupMembers => Set<CohabitationGroupMember>();
    public DbSet<OwnerHistory> OwnerHistories => Set<OwnerHistory>();
    public DbSet<ContactHistory> ContactHistories => Set<ContactHistory>();
    public DbSet<SpokespersonHistory> SpokespersonHistories => Set<SpokespersonHistory>();

    // ── Módulo de Cuotas y Cartera (nuevo) ───────────────────────────
    public DbSet<BillingPeriod> BillingPeriods => Set<BillingPeriod>();
    public DbSet<UnitFee> UnitFees => Set<UnitFee>();
    public DbSet<ExtraordinaryFee> ExtraordinaryFees => Set<ExtraordinaryFee>();
    public DbSet<ExtraordinaryFeeDistribution> ExtraordinaryFeeDistributions => Set<ExtraordinaryFeeDistribution>();
    public DbSet<IndividualCharge> IndividualCharges => Set<IndividualCharge>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
    public DbSet<LateInterest> LateInterests => Set<LateInterest>();
    public DbSet<PaymentAgreement> PaymentAgreements => Set<PaymentAgreement>();
    public DbSet<AgreementInstallment> AgreementInstallments => Set<AgreementInstallment>();
    public DbSet<ClearanceCertificate> ClearanceCertificates => Set<ClearanceCertificate>();
    public DbSet<AgreementDebt> AgreementDebts => Set<AgreementDebt>();

    // ── Módulo Bancario (nuevo) ─────────────────────────────────────────
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankMovement> BankMovements => Set<BankMovement>();
    public DbSet<BankReconciliation> BankReconciliations => Set<BankReconciliation>();
    public DbSet<ReconciliationItem> ReconciliationItems => Set<ReconciliationItem>();

    // ── Módulo de Activos Fijos ───────────────────────────────────────────
    public DbSet<FixedAsset> FixedAssets => Set<FixedAsset>();
    public DbSet<MonthlyDepreciation> MonthlyDepreciations => Set<MonthlyDepreciation>();

    // ── Módulo de Dashboard (nuevo) ────────────────────────────────────
    public DbSet<AlertConfiguration> AlertConfigurations => Set<AlertConfiguration>();
    public DbSet<IndicatorCache> IndicatorCaches => Set<IndicatorCache>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // ── Módulo PQR (nuevo) ────────────────────────────────────────────
    public DbSet<PqrRecord> PqrRecords => Set<PqrRecord>();
    public DbSet<PqrFollowUp> PqrFollowUps => Set<PqrFollowUp>();
    public DbSet<PqrResponse> PqrResponses => Set<PqrResponse>();
    public DbSet<PqrInternalNote> PqrInternalNotes => Set<PqrInternalNote>();
    public DbSet<PqrFile> PqrFiles => Set<PqrFile>();
    public DbSet<PqrTimeConfig> PqrTimeConfigs => Set<PqrTimeConfig>();
    public DbSet<PqrAlert> PqrAlerts => Set<PqrAlert>();

    // ── Módulo de Proveedores y Contratos ──────────────────────────────
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractPolicy> ContractPolicies => Set<ContractPolicy>();
    public DbSet<ContractAlert> ContractAlerts => Set<ContractAlert>();
    public DbSet<ProviderInvoice> ProviderInvoices => Set<ProviderInvoice>();
    public DbSet<ProviderPayment> ProviderPayments => Set<ProviderPayment>();
    public DbSet<ProviderEvaluation> ProviderEvaluations => Set<ProviderEvaluation>();
    public DbSet<RetentionConfiguration> RetentionConfigurations => Set<RetentionConfiguration>();
    public DbSet<ApprovalThreshold> ApprovalThresholds => Set<ApprovalThreshold>();

    // ── Módulo de Mantenimiento y Zonas Comunes ────────────────────────
    public DbSet<CommonAsset> CommonAssets => Set<CommonAsset>();
    public DbSet<AssetPhoto> AssetPhotos => Set<AssetPhoto>();
    public DbSet<MaintenancePlan> MaintenancePlans => Set<MaintenancePlan>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderEvidence> WorkOrderEvidences => Set<WorkOrderEvidence>();
    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<IncidentWorkOrder> IncidentWorkOrders => Set<IncidentWorkOrder>();
    public DbSet<AssetStatusHistory> AssetStatusHistories => Set<AssetStatusHistory>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Only resolve the tenant connection string when no provider has been configured.
        // - When created via DI with AddDbContext<T>() (no lambda): IsConfigured=false → resolver runs.
        // - When created with explicit options (e.g. DatabaseMigrationService): IsConfigured=true → skip.
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _tenantResolver.GetConnectionStringAsync().GetAwaiter().GetResult();

            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }
            else
            {
                // Fallback: no tenant in context (e.g. background services, seed without SetCurrentTenant)
                var masterCs = Environment.GetEnvironmentVariable("ConnectionStrings__MasterConnection")
                             ?? "Server=127.0.0.1;Port=3307;Database=erp_master;User=root;Password=1234;";
                optionsBuilder.UseMySql(masterCs, ServerVersion.AutoDetect(masterCs));
            }
        }

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Entidades financieras y Plan de Cuentas ───────────────────
        modelBuilder.Entity<AccountingAccount>(entity =>
        {
            entity.ToTable("erp_accounting_accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.Nature).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.ToTable("erp_budgets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.MeetingActNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);
            entity.HasIndex(e => new { e.TenantId, e.FiscalPeriod });
        });

        modelBuilder.Entity<BudgetDetail>(entity =>
        {
            entity.ToTable("erp_budget_details");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ApprovedValue).HasPrecision(18, 2);
            entity.Property(e => e.Observations).HasMaxLength(500);

            entity.HasOne(e => e.Budget)
                  .WithMany(b => b.BudgetDetails)
                  .HasForeignKey(e => e.BudgetId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AccountingAccount)
                  .WithMany()
                  .HasForeignKey(e => e.AccountingAccountId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.BudgetId, e.AccountingAccountId }).IsUnique();
        });

        modelBuilder.Entity<BudgetMovement>(entity =>
        {
            entity.ToTable("erp_budget_movements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Justification).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.MeetingActNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MovementType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.ApprovalType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.Budget)
                  .WithMany()
                  .HasForeignKey(e => e.BudgetId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SourceAccount)
                  .WithMany()
                  .HasForeignKey(e => e.SourceAccountId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DestinationAccount)
                  .WithMany()
                  .HasForeignKey(e => e.DestinationAccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ContingencyFund>(entity =>
        {
            entity.ToTable("erp_contingency_funds");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CurrentBalance).HasPrecision(18, 2);
            entity.HasIndex(e => e.TenantId).IsUnique();
        });

        modelBuilder.Entity<ContingencyFundContribution>(entity =>
        {
            entity.ToTable("erp_contingency_fund_contributions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Period).IsRequired().HasMaxLength(7); // YYYY-MM
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.IncomeBase).HasPrecision(18, 2);
            entity.Property(e => e.Percentage).HasPrecision(5, 2);
            entity.HasIndex(e => new { e.TenantId, e.Period }).IsUnique();
        });

        modelBuilder.Entity<ContingencyFundUsage>(entity =>
        {
            entity.ToTable("erp_contingency_fund_usages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Justification).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.CouncilApprovalActNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);
        });

        modelBuilder.Entity<AccountingPeriod>(entity =>
        {
            entity.ToTable("erp_accounting_periods");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PeriodLabel).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.ClosedByUserId).HasMaxLength(450);
            entity.HasIndex(e => new { e.TenantId, e.FiscalYear, e.Month }).IsUnique();
        });

        modelBuilder.Entity<AccountingEntry>(entity =>
        {
            entity.ToTable("erp_accounting_entries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.EntryType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ExternalReference).HasMaxLength(100);
            entity.Property(e => e.TotalDebit).HasPrecision(18, 2);
            entity.Property(e => e.TotalCredit).HasPrecision(18, 2);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.UpdatedByUserId).HasMaxLength(450);

            entity.HasOne(e => e.AccountingPeriod)
                  .WithMany()
                  .HasForeignKey(e => e.AccountingPeriodId)
                  .OnDelete(DeleteBehavior.Restrict)
                  .IsRequired(false);

            entity.HasIndex(e => new { e.TenantId, e.EntryNumber }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.EntryDate });
            entity.HasIndex(e => new { e.TenantId, e.Status });
        });

        modelBuilder.Entity<EntryLine>(entity =>
        {
            entity.ToTable("erp_entry_lines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ThirdPartyId).HasMaxLength(255);
            entity.Property(e => e.Debit).HasPrecision(18, 2);
            entity.Property(e => e.Credit).HasPrecision(18, 2);

            entity.HasOne(e => e.AccountingEntry)
                  .WithMany(e => e.Lines)
                  .HasForeignKey(e => e.AccountingEntryId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AccountingAccount)
                  .WithMany()
                  .HasForeignKey(e => e.AccountingAccountId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EntryReversal>(entity =>
        {
            entity.ToTable("erp_entry_reversals");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ReversedByUserId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.OriginalEntry)
                  .WithMany()
                  .HasForeignKey(e => e.OriginalEntryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReversalEntry)
                  .WithMany()
                  .HasForeignKey(e => e.ReversalEntryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.TenantId, e.OriginalEntryId }).IsUnique();
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
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ProviderType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.VerificationDigit).HasMaxLength(2);
            entity.Property(e => e.BusinessName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.TradeName).HasMaxLength(300);
            entity.Property(e => e.ContactName).HasMaxLength(300);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.EconomicActivity).HasMaxLength(200);
            entity.Property(e => e.ServiceType).HasMaxLength(100);
            entity.Property(e => e.RutFilePath).HasMaxLength(1000);
            entity.Property(e => e.LegalRepDocumentType).HasMaxLength(20);
            entity.Property(e => e.LegalRepDocumentNumber).HasMaxLength(50);
            entity.Property(e => e.LegalRepName).HasMaxLength(300);
            entity.Property(e => e.LegalRepEmail).HasMaxLength(256);
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);

            entity.HasIndex(e => new { e.TenantId, e.DocumentNumber }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.HasIndex(e => new { e.TenantId, e.ServiceType });
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
            entity.Property(e => e.LegalRepresentativeDocumentType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.LegalRepresentativeDv).HasMaxLength(1);
            
            entity.Property(e => e.LatePaymentInterestRate).HasPrecision(5, 2);
            entity.Property(e => e.MaxLegalInterestRate).HasPrecision(5, 2);
            entity.Property(e => e.AnnualBudget).HasPrecision(18, 2);
            entity.Property(e => e.ContingencyFundPercentage).HasPrecision(5, 2);
        });

        // ── ConfigurationAuditLog ────────────────────────────────────────
        modelBuilder.Entity<ConfigurationAuditLog>(entity =>
        {
            entity.ToTable("erp_configuration_audit_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ChangedByUserId).HasMaxLength(450);
            entity.Property(e => e.ParameterName).HasMaxLength(100);
            entity.HasIndex(e => new { e.TenantId, e.ParameterName, e.Timestamp });
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

        // ── Módulo de Unidades ───────────────────────────────────────────
        modelBuilder.Entity<UnitType>(entity =>
        {
            entity.ToTable("erp_unit_types");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.ToTable("erp_units");
            entity.HasKey(e => e.Id);
            
            // Unique index for identifier per tenant
            entity.HasIndex(e => new { e.TenantId, e.Identifier }).IsUnique();

            entity.Property(e => e.Identifier).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TowerOrBlock).HasMaxLength(50);
            entity.Property(e => e.PrivateArea).HasPrecision(18, 2);
            entity.Property(e => e.BalconyArea).HasPrecision(18, 2);
            entity.Property(e => e.CoproprietyCoefficient).HasPrecision(18, 4);
            
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.ParkingIdentifier).HasMaxLength(50);
            entity.Property(e => e.StorageIdentifier).HasMaxLength(50);
            entity.Property(e => e.InternalObservations).HasMaxLength(1000);
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);

            // Add Check Constraint for Coefficient
#pragma warning disable CS0618 // HasCheckConstraint is obsolete in newer EF Core
            entity.HasCheckConstraint("CK_Unit_CoproprietyCoefficient_Positive", "`CoproprietyCoefficient` > 0");
#pragma warning restore CS0618

            entity.HasOne(e => e.UnitType)
                  .WithMany()
                  .HasForeignKey(e => e.UnitTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UnitStateHistory>(entity =>
        {
            entity.ToTable("erp_unit_state_history");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.PreviousStatus).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.NewStatus).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.ChangedByUserId).HasMaxLength(450);
            entity.Property(e => e.Reason).HasMaxLength(500);

            entity.HasOne(e => e.Unit)
                  .WithMany()
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UnitComplement>(entity =>
        {
            entity.ToTable("erp_unit_complements");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ComplementType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);

            entity.HasOne(e => e.ParentUnit)
                  .WithMany()
                  .HasForeignKey(e => e.ParentUnitId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ComplementUnit)
                  .WithMany()
                  .HasForeignKey(e => e.ComplementUnitId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BulkImportLog>(entity =>
        {
            entity.ToTable("erp_bulk_import_logs");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.ExecutedByUserId).HasMaxLength(450);
            entity.Property(e => e.ErrorReport).HasColumnType("longtext");
        });
        // ── Módulo de Residentes y Propietarios ──────────────────────────

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.ToTable("erp_owners");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.OwnerType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.DocumentType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.VerificationDigit).HasMaxLength(2);
            entity.Property(e => e.FullNameOrCompanyName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.MainPhone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.AlternativePhone).HasMaxLength(20);
            entity.Property(e => e.CorrespondenceAddress).HasMaxLength(500);
            entity.Property(e => e.CivilStatus).HasMaxLength(30);
            entity.Property(e => e.LegalRepresentativeName).HasMaxLength(300);
            entity.Property(e => e.LegalRepresentativeDocumentType).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.LegalRepresentativeDocument).HasMaxLength(50);
            entity.Property(e => e.LegalRepresentativeRole).HasMaxLength(100);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);

            // Un mismo documento no puede pertenecer a dos propietarios activos del mismo tenant
            entity.HasIndex(e => new { e.TenantId, e.DocumentNumber }).IsUnique();
            entity.HasIndex(e => e.TenantId);
        });

        modelBuilder.Entity<UnitOwner>(entity =>
        {
            entity.ToTable("erp_unit_owners");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.OwnershipPercentage).HasPrecision(7, 4);

            entity.HasOne(e => e.Unit)
                  .WithMany(u => u.UnitOwners)
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Owner)
                  .WithMany(o => o.UnitOwners)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índice compuesto para buscar vocero activo de una unidad eficientemente
            entity.HasIndex(e => new { e.UnitId, e.IsActive, e.IsSpokesperson });
            entity.HasIndex(e => new { e.TenantId, e.UnitId, e.IsActive });
        });

        modelBuilder.Entity<TenantResident>(entity =>
        {
            entity.ToTable("erp_tenant_residents");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.DocumentType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
            entity.Property(e => e.RealEstateAgentName).HasMaxLength(200);
            entity.Property(e => e.RealEstateAgentPhone).HasMaxLength(20);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.Unit)
                  .WithMany(u => u.TenantResidents)
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índice para consulta de contratos próximos a vencer
            entity.HasIndex(e => new { e.TenantId, e.LeaseEndDate, e.IsActive });
            entity.HasIndex(e => new { e.UnitId, e.IsActive });
        });

        modelBuilder.Entity<CohabitationGroupMember>(entity =>
        {
            entity.ToTable("erp_cohabitation_group_members");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FullNameOrPetName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Relationship).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PetSpecies).HasMaxLength(100);
            entity.Property(e => e.PetBreed).HasMaxLength(100);
            entity.Property(e => e.PetSanitaryRegistration).HasMaxLength(100);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.Unit)
                  .WithMany(u => u.CohabitationGroupMembers)
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índice para conteo de mascotas activas por unidad
            entity.HasIndex(e => new { e.UnitId, e.IsActive, e.IsPet });
        });

        modelBuilder.Entity<OwnerHistory>(entity =>
        {
            entity.ToTable("erp_owner_histories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.TransferNotes).HasColumnType("longtext");
            entity.Property(e => e.RecordedByUserId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.Unit)
                  .WithMany(u => u.OwnerHistories)
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Owner)
                  .WithMany(o => o.OwnerHistories)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índice para búsqueda de propietarios actuales (EndDate IS NULL)
            entity.HasIndex(e => new { e.UnitId, e.EndDate });
            entity.HasIndex(e => new { e.TenantId, e.OwnerId });
        });

        modelBuilder.Entity<ContactHistory>(entity =>
        {
            entity.ToTable("erp_contact_histories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FieldChanged).IsRequired().HasMaxLength(100);
            entity.Property(e => e.OldValue).HasColumnType("longtext");
            entity.Property(e => e.NewValue).HasColumnType("longtext");
            entity.Property(e => e.ChangedByUserId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.Owner)
                  .WithMany(o => o.ContactHistories)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.OwnerId, e.ChangedAt });
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("erp_notifications");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);

            entity.HasOne(e => e.Owner)
                  .WithMany()
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.OwnerId, e.IsRead, e.CreatedAt });
        });

        modelBuilder.Entity<SpokespersonHistory>(entity =>
        {
            entity.ToTable("erp_spokesperson_histories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ChangedByUserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.ChangeReason).HasMaxLength(500);

            entity.HasOne(e => e.Unit)
                  .WithMany(u => u.SpokespersonHistories)
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PreviousSpokesperson)
                  .WithMany(o => o.SpokespersonHistoriesAsPrevious)
                  .HasForeignKey(e => e.PreviousSpokespersonId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.NewSpokesperson)
                  .WithMany(o => o.SpokespersonHistoriesAsNew)
                  .HasForeignKey(e => e.NewSpokespersonId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.UnitId, e.ChangedAt });
        });

        // ── Módulo de Cuotas y Cartera ────────────────────────────────────

        modelBuilder.Entity<BillingPeriod>(entity =>
        {
            entity.ToTable("erp_billing_periods");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Period).IsRequired().HasMaxLength(7);
            entity.Property(e => e.MonthlyBudgetTotal).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.ExecutedByUserId).HasMaxLength(450);
            entity.Property(e => e.RoundingAdjustment).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);

            entity.HasIndex(e => new { e.TenantId, e.Period }).IsUnique();
        });

        modelBuilder.Entity<UnitFee>(entity =>
        {
            entity.ToTable("erp_unit_fees");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FeeValue).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.BillingPeriod)
                  .WithMany(b => b.UnitFees)
                  .HasForeignKey(e => e.BillingPeriodId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Unit)
                  .WithMany()
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.TenantId, e.BillingPeriodId, e.UnitId }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.UnitId, e.Status });
        });

        modelBuilder.Entity<ExtraordinaryFee>(entity =>
        {
            entity.ToTable("erp_extraordinary_fees");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.MeetingActNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.StartPeriod).IsRequired().HasMaxLength(7);
            entity.Property(e => e.DistributionType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);

            entity.HasIndex(e => new { e.TenantId, e.Status });
        });

        modelBuilder.Entity<ExtraordinaryFeeDistribution>(entity =>
        {
            entity.ToTable("erp_extraordinary_fee_distributions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.ExtraordinaryFee)
                  .WithMany(f => f.Distributions)
                  .HasForeignKey(e => e.ExtraordinaryFeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Unit)
                  .WithMany()
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.ExtraordinaryFeeId, e.UnitId, e.InstallmentNumber });
            entity.HasIndex(e => new { e.TenantId, e.UnitId, e.Status });
        });

        modelBuilder.Entity<IndividualCharge>(entity =>
        {
            entity.ToTable("erp_individual_charges");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ChargeType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.Concept).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ReferenceActNumber).HasMaxLength(100);
            entity.Property(e => e.DisputeReason).HasMaxLength(500);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceAmount).HasPrecision(18, 2);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.Unit)
                  .WithMany()
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.TenantId, e.UnitId, e.Status });
            entity.HasIndex(e => new { e.TenantId, e.Status, e.IsDisputed });
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("erp_payments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethod).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ReceivedByUserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.AdvanceAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.Unit)
                  .WithMany()
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.TenantId, e.UnitId, e.PaymentDate });
        });

        modelBuilder.Entity<PaymentAllocation>(entity =>
        {
            entity.ToTable("erp_payment_allocations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.AllocationType).HasConversion<string>().HasMaxLength(20).IsRequired();

            entity.HasOne(e => e.Payment)
                  .WithMany(p => p.Allocations)
                  .HasForeignKey(e => e.PaymentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UnitFee)
                  .WithMany()
                  .HasForeignKey(e => e.UnitFeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ExtraordinaryFeeDistribution)
                  .WithMany()
                  .HasForeignKey(e => e.ExtraordinaryFeeDistributionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.IndividualCharge)
                  .WithMany()
                  .HasForeignKey(e => e.IndividualChargeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.LateInterest)
                  .WithMany()
                  .HasForeignKey(e => e.LateInterestId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.PaymentId, e.AllocationType });
            entity.HasIndex(e => new { e.UnitFeeId, e.AllocationType });
        });

        modelBuilder.Entity<LateInterest>(entity =>
        {
            entity.ToTable("erp_late_interests");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Period).IsRequired().HasMaxLength(7);
            entity.Property(e => e.BaseAmount).HasPrecision(18, 2);
            entity.Property(e => e.DailyRate).HasPrecision(12, 8);
            entity.Property(e => e.CalculatedAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.UnitFee)
                  .WithMany()
                  .HasForeignKey(e => e.UnitFeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ExtraordinaryFeeDistribution)
                  .WithMany()
                  .HasForeignKey(e => e.ExtraordinaryFeeDistributionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.IndividualCharge)
                  .WithMany()
                  .HasForeignKey(e => e.IndividualChargeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.TenantId, e.UnitFeeId, e.Period });
            entity.HasIndex(e => new { e.TenantId, e.IsCapitalized });
        });

        modelBuilder.Entity<PaymentAgreement>(entity =>
        {
            entity.ToTable("erp_payment_agreements");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.TotalDebtIncluded).HasPrecision(18, 2);
            entity.Property(e => e.InstallmentAmount).HasPrecision(18, 2);
            entity.Property(e => e.InterestForgivenessPercentage).HasPrecision(5, 2);
            entity.Property(e => e.CouncilActNumber).HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.DigitalAcceptance).HasMaxLength(2000);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.Unit)
                  .WithMany()
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.TenantId, e.UnitId, e.Status });
        });

        modelBuilder.Entity<AgreementInstallment>(entity =>
        {
            entity.ToTable("erp_agreement_installments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

            entity.HasOne(e => e.PaymentAgreement)
                  .WithMany(a => a.Installments)
                  .HasForeignKey(e => e.PaymentAgreementId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.PaymentAgreementId, e.InstallmentNumber }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Status, e.DueDate });
        });

        modelBuilder.Entity<AgreementDebt>(entity =>
        {
            entity.ToTable("erp_agreement_debts");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.SourceType).IsRequired().HasMaxLength(30);
            entity.Property(e => e.OriginalBalance).HasPrecision(18, 2);

            entity.HasOne(e => e.PaymentAgreement)
                  .WithMany()
                  .HasForeignKey(e => e.PaymentAgreementId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.PaymentAgreementId, e.SourceType, e.SourceId }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.SourceType, e.SourceId });
        });

        modelBuilder.Entity<ClearanceCertificate>(entity =>
        {
            entity.ToTable("erp_clearance_certificates");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CertificateNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.BalanceAtDate).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.IssuedByUserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.SignedByAdministratorName).HasMaxLength(300);

            entity.HasOne(e => e.Unit)
                  .WithMany()
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.TenantId, e.CertificateNumber }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.UnitId, e.Status });
        });

        // ── Módulo Bancario ──────────────────────────────────────────────────────

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.ToTable("erp_bank_accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.BankName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AccountType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.CurrentBalance).HasPrecision(18, 2);
            entity.Property(e => e.OpeningBalance).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.TenantId, e.AccountNumber }).IsUnique();
        });

        modelBuilder.Entity<BankMovement>(entity =>
        {
            entity.ToTable("erp_bank_movements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.MovementType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.RunningBalance).HasPrecision(18, 2);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);
            entity.HasOne(e => e.BankAccount)
                  .WithMany(b => b.Movements)
                  .HasForeignKey(e => e.BankAccountId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BankReconciliation>(entity =>
        {
            entity.ToTable("erp_bank_reconciliations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PeriodLabel).IsRequired().HasMaxLength(20);
            entity.Property(e => e.BookBalance).HasPrecision(18, 2);
            entity.Property(e => e.StatementBalance).HasPrecision(18, 2);
            entity.Property(e => e.Difference).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.CompletedByUserId).HasMaxLength(450);
            entity.HasOne(e => e.BankAccount)
                  .WithMany(b => b.Reconciliations)
                  .HasForeignKey(e => e.BankAccountId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.TenantId, e.BankAccountId, e.FiscalYear, e.Month }).IsUnique();
        });

        modelBuilder.Entity<ReconciliationItem>(entity =>
        {
            entity.ToTable("erp_reconciliation_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(e => e.BankReconciliation)
                  .WithMany(r => r.Items)
                  .HasForeignKey(e => e.BankReconciliationId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Módulo de Activos Fijos ─────────────────────────────────────────────

        modelBuilder.Entity<FixedAsset>(entity =>
        {
            entity.ToTable("erp_fixed_assets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SerialNumber).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.AcquisitionValue).HasPrecision(18, 2);
            entity.Property(e => e.ResidualValue).HasPrecision(18, 2);
            entity.Property(e => e.AccumulatedDepreciation).HasPrecision(18, 2);
            entity.Property(e => e.BookValue).HasPrecision(18, 2);
            entity.Property(e => e.DisposalReason).HasMaxLength(500);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.DepreciationMethod).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.HasOne(e => e.AccountingAccount)
                  .WithMany()
                  .HasForeignKey(e => e.AccountingAccountId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.TenantId, e.SerialNumber }).IsUnique().HasFilter("[SerialNumber] IS NOT NULL AND [SerialNumber] <> ''");
        });

        modelBuilder.Entity<MonthlyDepreciation>(entity =>
        {
            entity.ToTable("erp_monthly_depreciations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PeriodLabel).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DepreciationAmount).HasPrecision(18, 2);
            entity.Property(e => e.AccumulatedAfter).HasPrecision(18, 2);
            entity.Property(e => e.BookValueAfter).HasPrecision(18, 2);
            entity.HasOne(e => e.FixedAsset)
                  .WithMany(f => f.Depreciations)
                  .HasForeignKey(e => e.FixedAssetId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.AccountingEntry)
                  .WithMany()
                  .HasForeignKey(e => e.AccountingEntryId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.TenantId, e.FixedAssetId, e.FiscalYear, e.Month }).IsUnique();
        });

        // ── Módulo de Dashboard ─────────────────────────────────────────────────

        modelBuilder.Entity<AlertConfiguration>(entity =>
        {
            entity.ToTable("erp_alert_configurations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.RuleType).HasConversion<string>().HasMaxLength(50).IsRequired();
            entity.Property(e => e.DefaultUrgency).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.ThresholdPercentage).HasPrecision(5, 2);

            entity.HasIndex(e => new { e.TenantId, e.RuleType }).IsUnique();
        });

        modelBuilder.Entity<IndicatorCache>(entity =>
        {
            entity.ToTable("erp_indicator_caches");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CacheKey).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CacheValue).HasColumnType("longtext");
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

            entity.HasIndex(e => new { e.TenantId, e.CacheKey }).IsUnique();
        });

        // ── Configuración Módulo PQR ──────────────────────────────────
        modelBuilder.Entity<PqrRecord>(entity =>
        {
            entity.ToTable("erp_pqr_records");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);

            entity.Property(e => e.RadicadoNumber).IsRequired().HasMaxLength(30);
            entity.Property(e => e.PQRType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.Category).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.Priority).HasConversion<string>().HasMaxLength(10).IsRequired();

            entity.Property(e => e.Subject).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(4000);

            entity.Property(e => e.RadiadorName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.RadiadorDocumentType).HasMaxLength(20);
            entity.Property(e => e.RadiadorDocumentNumber).HasMaxLength(50);
            entity.Property(e => e.RadiadorContact).HasMaxLength(200);

            entity.Property(e => e.Channel).HasConversion<string>().HasMaxLength(15).IsRequired();

            entity.Property(e => e.AssignedToUserId).HasMaxLength(450);

            entity.Property(e => e.InvolvedResidentName).HasMaxLength(300);

            entity.Property(e => e.IsInternal).IsRequired();

            entity.Property(e => e.ClaimResolutionNote).HasMaxLength(2000);

            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.Unit)
                  .WithMany(u => u.PqrRecords)
                  .HasForeignKey(e => e.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Owner)
                  .WithMany(o => o.PqrRecords)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.TenantResident)
                  .WithMany(t => t.PqrRecords)
                  .HasForeignKey(e => e.TenantResidentId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.RelatedPQR)
                  .WithMany()
                  .HasForeignKey(e => e.RelatedPQRId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.InvolvedResidentUnit)
                  .WithMany(u => u.PqrRecordsAsInvolvedResident)
                  .HasForeignKey(e => e.InvolvedResidentUnitId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.UnitFee)
                  .WithMany(f => f.PqrRecords)
                  .HasForeignKey(e => e.UnitFeeId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ExtraordinaryFeeDistribution)
                  .WithMany(d => d.PqrRecords)
                  .HasForeignKey(e => e.ExtraordinaryFeeDistributionId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.IndividualCharge)
                  .WithMany(c => c.PqrRecords)
                  .HasForeignKey(e => e.IndividualChargeId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.TenantId, e.RadicadoNumber }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.UnitId, e.Status });
            entity.HasIndex(e => new { e.TenantId, e.Status, e.Priority, e.Deadline });
            entity.HasIndex(e => new { e.TenantId, e.PQRType, e.Status });
            entity.HasIndex(e => e.FiledAt);
        });

        modelBuilder.Entity<PqrFollowUp>(entity =>
        {
            entity.ToTable("erp_pqr_follow_ups");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ChangedByUserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.ChangedByUserName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Justification).IsRequired().HasMaxLength(2000);

            entity.Property(e => e.PreviousStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.NewStatus).HasConversion<string>().HasMaxLength(20).IsRequired();

            entity.HasOne(e => e.PQR)
                  .WithMany(p => p.FollowUps)
                  .HasForeignKey(e => e.PQRId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.PQRId, e.ChangedAt });
        });

        modelBuilder.Entity<PqrResponse>(entity =>
        {
            entity.ToTable("erp_pqr_responses");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ResponseText).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.SentByUserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.SentByUserName).IsRequired().HasMaxLength(300);

            entity.HasOne(e => e.PQR)
                  .WithMany(p => p.Responses)
                  .HasForeignKey(e => e.PQRId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.PQRId, e.SentAt });
        });

        modelBuilder.Entity<PqrInternalNote>(entity =>
        {
            entity.ToTable("erp_pqr_internal_notes");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.NoteText).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.AuthorName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.PQR)
                  .WithMany(p => p.InternalNotes)
                  .HasForeignKey(e => e.PQRId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PqrFile>(entity =>
        {
            entity.ToTable("erp_pqr_files");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.OriginalFileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.UploadedByUserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.UploadedByUserName).IsRequired().HasMaxLength(300);

            entity.HasOne(e => e.PQR)
                  .WithMany(p => p.Files)
                  .HasForeignKey(e => e.PQRId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PqrResponse)
                  .WithMany(r => r.Files)
                  .HasForeignKey(e => e.PqrResponseId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PqrInternalNote)
                  .WithMany(n => n.Files)
                  .HasForeignKey(e => e.PqrInternalNoteId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PqrTimeConfig>(entity =>
        {
            entity.ToTable("erp_pqr_time_configs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PQRType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.UpdatedByUserId).IsRequired().HasMaxLength(450);

            entity.HasIndex(e => new { e.TenantId, e.PQRType }).IsUnique();
        });

        modelBuilder.Entity<PqrAlert>(entity =>
        {
            entity.ToTable("erp_pqr_alerts");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.AlertType).HasConversion<string>().HasMaxLength(20).IsRequired();

            entity.HasOne(e => e.PQR)
                  .WithMany(p => p.Alerts)
                  .HasForeignKey(e => e.PQRId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.PQRId, e.AlertType, e.IsActive });
            entity.HasIndex(e => new { e.IsActive, e.GeneratedAt });
        });

        // ── Módulo de Proveedores y Contratos ──────────────────────────

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("erp_contracts");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContractNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ContractType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.ObjectDescription).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.TotalValue).HasPrecision(18, 2);
            entity.Property(e => e.MonthlyValue).HasPrecision(18, 2);
            entity.Property(e => e.ApprovalLevel).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.CouncilMeetingActNumber).HasMaxLength(100);
            entity.Property(e => e.AssemblyMeetingActNumber).HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.SignedContractFilePath).HasMaxLength(1000);
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);
            entity.Property(e => e.UpdatedByUserId).HasMaxLength(450);

            entity.HasOne(e => e.Provider)
                  .WithMany(p => p.Contracts)
                  .HasForeignKey(e => e.ProviderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.BudgetAccount)
                  .WithMany()
                  .HasForeignKey(e => e.BudgetAccountId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.TenantId, e.ContractNumber }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.HasIndex(e => new { e.TenantId, e.ProviderId });
            entity.HasIndex(e => new { e.TenantId, e.EndDate });
        });

        modelBuilder.Entity<ContractPolicy>(entity =>
        {
            entity.ToTable("erp_contract_policies");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PolicyNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.InsuranceCompany).IsRequired().HasMaxLength(300);
            entity.Property(e => e.PolicyType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.InsuredAmount).HasPrecision(18, 2);
            entity.Property(e => e.FilePath).HasMaxLength(1000);
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);

            entity.HasOne(e => e.Contract)
                  .WithMany(c => c.Policies)
                  .HasForeignKey(e => e.ContractId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.ContractId });
            entity.HasIndex(e => new { e.TenantId, e.EndDate, e.IsActive });
        });

        modelBuilder.Entity<ContractAlert>(entity =>
        {
            entity.ToTable("erp_contract_alerts");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.AlertType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.ResolvedByUserId).HasMaxLength(450);

            entity.HasOne(e => e.Contract)
                  .WithMany(c => c.Alerts)
                  .HasForeignKey(e => e.ContractId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.ContractId, e.IsActive });
            entity.HasIndex(e => new { e.TenantId, e.IsActive, e.GeneratedAt });
        });

        modelBuilder.Entity<ProviderInvoice>(entity =>
        {
            entity.ToTable("erp_provider_invoices");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.IvaAmount).HasPrecision(18, 2);
            entity.Property(e => e.RetentionFuelAmount).HasPrecision(18, 2);
            entity.Property(e => e.RetentionIcaAmount).HasPrecision(18, 2);
            entity.Property(e => e.NetAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.InvoiceFilePath).HasMaxLength(1000);
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);
            entity.Property(e => e.UpdatedByUserId).HasMaxLength(450);

            entity.HasOne(e => e.Provider)
                  .WithMany(p => p.Invoices)
                  .HasForeignKey(e => e.ProviderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Contract)
                  .WithMany(c => c.Invoices)
                  .HasForeignKey(e => e.ContractId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AccountingEntry)
                  .WithMany()
                  .HasForeignKey(e => e.AccountingEntryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.TenantId, e.InvoiceNumber });
            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.HasIndex(e => new { e.TenantId, e.DueDate });
            entity.HasIndex(e => new { e.TenantId, e.ProviderId });
        });

        modelBuilder.Entity<ProviderPayment>(entity =>
        {
            entity.ToTable("erp_provider_payments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethod).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.BankAccount).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.ReceiptFilePath).HasMaxLength(1000);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);

            entity.HasOne(e => e.Invoice)
                  .WithMany(i => i.Payments)
                  .HasForeignKey(e => e.InvoiceId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AccountingEntry)
                  .WithMany()
                  .HasForeignKey(e => e.AccountingEntryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.TenantId, e.InvoiceId });
            entity.HasIndex(e => new { e.TenantId, e.Status });
        });

        modelBuilder.Entity<ProviderEvaluation>(entity =>
        {
            entity.ToTable("erp_provider_evaluations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.EvaluationPeriod).IsRequired().HasMaxLength(20);
            entity.Property(e => e.AverageScore).HasPrecision(3, 2);
            entity.Property(e => e.Comments).HasMaxLength(4000);
            entity.Property(e => e.Recommendation).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.EvaluatedByUserId).HasMaxLength(450);
            entity.Property(e => e.EvaluatedByUserName).HasMaxLength(300);

            entity.HasOne(e => e.Provider)
                  .WithMany(p => p.Evaluations)
                  .HasForeignKey(e => e.ProviderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Contract)
                  .WithMany()
                  .HasForeignKey(e => e.ContractId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.TenantId, e.ProviderId });
            entity.HasIndex(e => new { e.TenantId, e.ProviderId, e.EvaluationPeriod });
        });

        modelBuilder.Entity<RetentionConfiguration>(entity =>
        {
            entity.ToTable("erp_retention_configurations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ServiceDescription).HasMaxLength(500);
            entity.Property(e => e.RetentionFuelRate).HasPrecision(5, 4);
            entity.Property(e => e.RetentionIcaRate).HasPrecision(5, 4);
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);
            entity.Property(e => e.UpdatedByUserId).HasMaxLength(450);

            entity.HasIndex(e => new { e.TenantId, e.ServiceType }).IsUnique();
        });

        modelBuilder.Entity<ApprovalThreshold>(entity =>
        {
            entity.ToTable("erp_approval_thresholds");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ApprovalLevel).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.MinValue).HasPrecision(18, 2);
            entity.Property(e => e.MaxValue).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);
            entity.Property(e => e.UpdatedByUserId).HasMaxLength(450);

            entity.HasIndex(e => new { e.TenantId, e.ApprovalLevel }).IsUnique();
        });

        // ── Módulo de Mantenimiento y Zonas Comunes ──────────────────

        modelBuilder.Entity<CommonAsset>(entity =>
        {
            entity.ToTable("erp_common_assets");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Category).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.Location).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Brand).HasMaxLength(150);
            entity.Property(e => e.Model).HasMaxLength(150);
            entity.Property(e => e.SerialNumber).HasMaxLength(100);
            entity.Property(e => e.AcquisitionValue).HasPrecision(18, 2);
            entity.Property(e => e.Manufacturer).HasMaxLength(200);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.StatusNotes).HasMaxLength(2000);
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);

            entity.HasOne(e => e.ReferenceProvider)
                  .WithMany()
                  .HasForeignKey(e => e.ReferenceProviderId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.HasIndex(e => new { e.TenantId, e.Category });
            entity.HasIndex(e => new { e.TenantId, e.Name });
        });

        modelBuilder.Entity<AssetPhoto>(entity =>
        {
            entity.ToTable("erp_asset_photos");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CapturedByUserId).HasMaxLength(450);

            entity.HasOne(e => e.Asset)
                  .WithMany(a => a.Photos)
                  .HasForeignKey(e => e.AssetId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.AssetId });
        });

        modelBuilder.Entity<MaintenancePlan>(entity =>
        {
            entity.ToTable("erp_maintenance_plans");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ActivityType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.FrequencyDays).IsRequired();
            entity.Property(e => e.EstimatedCost).HasPrecision(18, 2);
            entity.Property(e => e.EstimatedDowntimeHours).HasDefaultValue(0);
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);
            entity.Property(e => e.UpdatedByUserId).HasMaxLength(450);

            entity.HasOne(e => e.Asset)
                  .WithMany(a => a.MaintenancePlans)
                  .HasForeignKey(e => e.AssetId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PreferredProvider)
                  .WithMany()
                  .HasForeignKey(e => e.PreferredProviderId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.TenantId, e.AssetId });
            entity.HasIndex(e => new { e.TenantId, e.NextExecutionDate });
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.ToTable("erp_work_orders");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.OrderType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.Priority).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(e => e.Origin).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.RelatedPqrNumber).HasMaxLength(50);
            entity.Property(e => e.EstimatedCost).HasPrecision(18, 2);
            entity.Property(e => e.ActualCost).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.Outcome).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.OutcomeNotes).HasMaxLength(2000);
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);
            entity.Property(e => e.UpdatedByUserId).HasMaxLength(450);

            entity.HasOne(e => e.Asset)
                  .WithMany(a => a.WorkOrders)
                  .HasForeignKey(e => e.AssetId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.AssignedProvider)
                  .WithMany()
                  .HasForeignKey(e => e.AssignedProviderId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.BudgetAccount)
                  .WithMany()
                  .HasForeignKey(e => e.BudgetAccountId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AccountingEntry)
                  .WithMany()
                  .HasForeignKey(e => e.AccountingEntryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.HasIndex(e => new { e.TenantId, e.AssetId });
            entity.HasIndex(e => new { e.TenantId, e.ScheduledDate });
            entity.HasIndex(e => new { e.TenantId, e.AssignedProviderId });
        });

        modelBuilder.Entity<WorkOrderEvidence>(entity =>
        {
            entity.ToTable("erp_work_order_evidences");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CapturedByUserId).HasMaxLength(450);

            entity.HasOne(e => e.WorkOrder)
                  .WithMany(w => w.Evidences)
                  .HasForeignKey(e => e.WorkOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.WorkOrderId });
        });

        modelBuilder.Entity<Incident>(entity =>
        {
            entity.ToTable("erp_incidents");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.IncidentType).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.TotalDamageValue).HasPrecision(18, 2);
            entity.Property(e => e.InsurancePolicyNumber).HasMaxLength(100);
            entity.Property(e => e.InsuranceCompany).HasMaxLength(200);
            entity.Property(e => e.PolicyFilePath).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired();
            entity.Property(e => e.CreatedByUserId).HasMaxLength(450);
            entity.Property(e => e.UpdatedByUserId).HasMaxLength(450);

            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.HasIndex(e => new { e.TenantId, e.IncidentType });
        });

        modelBuilder.Entity<IncidentWorkOrder>(entity =>
        {
            entity.ToTable("erp_incident_work_orders");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);

            entity.HasOne(e => e.Incident)
                  .WithMany(i => i.IncidentWorkOrders)
                  .HasForeignKey(e => e.IncidentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WorkOrder)
                  .WithMany()
                  .HasForeignKey(e => e.WorkOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.IncidentId }).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.WorkOrderId });
        });

        modelBuilder.Entity<AssetStatusHistory>(entity =>
        {
            entity.ToTable("erp_asset_status_histories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TenantId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PreviousStatus).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.NewStatus).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(1000);
            entity.Property(e => e.ChangedByUserId).HasMaxLength(450);
            entity.Property(e => e.ChangedByUserName).HasMaxLength(300);

            entity.HasOne(e => e.Asset)
                  .WithMany(a => a.StatusHistory)
                  .HasForeignKey(e => e.AssetId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.TenantId, e.AssetId });
            entity.HasIndex(e => new { e.TenantId, e.ChangedAt });
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
