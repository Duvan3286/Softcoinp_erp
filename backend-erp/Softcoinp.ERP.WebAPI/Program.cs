using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Softcoinp.ERP.Domain.Entities;
using Softcoinp.ERP.Domain.Interfaces;
using Softcoinp.ERP.Infrastructure.Persistence;
using Softcoinp.ERP.Infrastructure.Persistence.Repositories;
using Softcoinp.ERP.Infrastructure.Services;
using Softcoinp.ERP.Infrastructure.External;
using Softcoinp.ERP.Application.Services;
using Softcoinp.ERP.WebAPI.Services;
using Softcoinp.ERP.WebAPI.Middleware;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var masterConnectionString = builder.Configuration.GetConnectionString("MasterConnection");

// Register Master DB Context
builder.Services.AddDbContext<MasterDbContext>(options =>
    options.UseMySql(masterConnectionString, ServerVersion.AutoDetect(masterConnectionString)));

// Register Tenant Resolver and dependencies
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITenantResolver, SubdomainTenantResolver>();
builder.Services.AddScoped<DatabaseMigrationService>();

// Register UnitOfWork and Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Identity
builder.Services.AddIdentity<User, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Register JWT Authentication
var jwtKey = builder.Configuration["JWT:Key"];
if (builder.Environment.IsProduction())
{
    if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
        throw new InvalidOperationException("JWT:Key must be configured with at least 32 characters in production.");
}
jwtKey ??= "YourSuperSecretKeyForDevelopmentOnly123!";
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"] ?? "SoftcoinpERP",
        ValidAudience = builder.Configuration["JWT:Audience"] ?? "SoftcoinpERP",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Register External Integration Client
builder.Services.AddHttpClient<ICoreIntegrationClient, CoreIntegrationClient>();

// Register Application Services
builder.Services.AddScoped<BillingService>();
builder.Services.AddScoped<BudgetService>();
builder.Services.AddScoped<BudgetExecutionService>();
builder.Services.AddScoped<BudgetMovementService>();
builder.Services.AddScoped<ContingencyFundService>();
builder.Services.AddScoped<BillingEngineService>();
builder.Services.AddScoped<LateInterestService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<PaymentAgreementService>();
builder.Services.AddScoped<StatementService>();
builder.Services.AddScoped<AccountingPeriodService>();
builder.Services.AddScoped<JournalEntryService>();
builder.Services.AddScoped<AccountingReportService>();
builder.Services.AddScoped<AccountingIntegrationService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<FixedAssetService>();
builder.Services.AddScoped<BankReconciliationService>();
builder.Services.AddScoped<IndicatorCacheService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<PQRRadicationService>();
builder.Services.AddScoped<ClaimResolutionService>();
builder.Services.AddScoped<ProviderService>();
builder.Services.AddScoped<ContractService>();
builder.Services.AddScoped<RetentionService>();
builder.Services.AddScoped<MaintenanceService>();
builder.Services.AddScoped<AssemblyQuorumEngine>();
builder.Services.AddScoped<AssemblyVotingEngine>();
builder.Services.AddScoped<AssemblyMinutesGenerator>();
builder.Services.AddScoped<AssemblyDecisionPropagationService>();
builder.Services.AddScoped<AssemblyService>();
builder.Services.AddScoped<ReservationAvailabilityEngine>();
builder.Services.AddScoped<ReservationReminderEngine>();
builder.Services.AddScoped<ReservationService>();

// ── Comunicados y Notificaciones ──────────────────────────────
builder.Services.AddScoped<DeliveryTrackerEngine>();
builder.Services.AddScoped<NotificationEngine>();
builder.Services.AddScoped<CommunicationService>();
builder.Services.AddScoped<NotificationTemplateService>();
builder.Services.AddScoped<BulletinBoardService>();
builder.Services.AddScoped<CommunicationPreferenceService>();
builder.Services.AddScoped<DelinquencySequenceEngine>();

// ── Reportes y Exportaciones ───────────────────────────────────
builder.Services.AddScoped<PDFGenerationEngine>();
builder.Services.AddScoped<ExcelGenerationEngine>();
builder.Services.AddScoped<ReportAccessControlService>();

// Register Background Services
builder.Services.AddHostedService<RecurringReportEngine>();
builder.Services.AddHostedService<PQRAlertEngineService>();
builder.Services.AddHostedService<ContractAlertEngineService>();
builder.Services.AddHostedService<PreventiveMaintenanceEngineService>();
builder.Services.AddHostedService<ScheduledCommunicationService>();

// Register Application DB Context (Multi-tenant)
// IMPORTANT: Do NOT pre-configure the connection string here.
// OnConfiguring() in ApplicationDbContext resolves the tenant's connection string
// dynamically via ITenantResolver on each request. Passing a fixed string here
// would mark options as "IsConfigured = true", bypassing the tenant resolution.
builder.Services.AddDbContext<ApplicationDbContext>();

builder.Services.AddHealthChecks()
    .AddMySql(masterConnectionString!, name: "master-mysql");

builder.Services.AddDataProtection();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy => {
        policy.SetIsOriginAllowed(origin => {
            var host = new Uri(origin).Host;
            return host == "localhost" || host.EndsWith(".localhost");
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Global Logger for debugging requests in Docker
app.Use(async (context, next) => {
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation(">>> Request Received: {Method} {Path} from {Origin}", 
        context.Request.Method, context.Request.Path, context.Request.Headers["Origin"]);
    await next();
});

// Automatic migrations and seeding on startup
if (app.Configuration.GetValue<bool>("AUTO_MIGRATE") || Environment.GetEnvironmentVariable("AUTO_MIGRATE") == "true")
{
    var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    // Step 1: Ensure erp_master DB exists and the 'test' tenant record is present.
    // This MUST succeed before anything else can run.
    bool masterSeeded = false;
    try
    {
        await MasterDbInitializer.SeedTenantAsync(services);
        startupLogger.LogInformation("Master DB seeded successfully.");
        masterSeeded = true;
    }
    catch (Exception ex)
    {
        startupLogger.LogCritical(ex, "FATAL: Could not seed master tenant. Aborting startup migrations.");
    }

    if (masterSeeded)
    {
        // Step 2: Attempt to seed Identity users. This may fail on first boot because
        // ApplicationDbContext.OnConfiguring() calls ITenantResolver which has no HTTP
        // context at startup. The failure is non-fatal — per-tenant seeding in Step 3
        // handles the actual user/role creation for each tenant database.
        try
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            await DbInitializer.SeedUsersAsync(userManager, roleManager, app.Configuration);
            startupLogger.LogInformation("Master Identity seed completed.");
        }
        catch (Exception ex)
        {
            startupLogger.LogWarning(ex,
                "Master Identity seed skipped (expected on first boot — tenant DB does not exist yet). " +
                "Per-tenant users will be seeded in Step 3.");
        }

        // Step 3: Create and migrate every active tenant's database, then seed per-tenant users.
        // Always runs regardless of whether Step 2 succeeded.
        try
        {
            var migrationService = services.GetRequiredService<DatabaseMigrationService>();
            var migrationResults = await migrationService.MigrateAllAsync();
            foreach (var entry in migrationResults)
            {
                startupLogger.LogInformation("Migration [{Tenant}]: {Status}", entry.Key, entry.Value);
            }
        }
        catch (Exception ex)
        {
            startupLogger.LogCritical(ex, "FATAL: Tenant database migration failed.");
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else 
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles(); // Habilitar archivos estáticos (wwwroot)
app.UseCors("AllowFrontend");
app.UseMiddleware<TenantDetectionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Logger.LogInformation("### SERVER READY - Waiting for requests on port 8080 (Mapped to 5005) ###");
app.Run();
