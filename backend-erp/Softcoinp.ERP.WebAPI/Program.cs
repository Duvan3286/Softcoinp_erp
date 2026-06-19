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
using Softcoinp.ERP.WebAPI.Middleware;

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
var jwtKey = builder.Configuration["JWT:Key"] ?? "YourSuperSecretKeyForDevelopmentOnly123!";
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

// Register Application DB Context (Multi-tenant)
builder.Services.AddDbContext<ApplicationDbContext>(options => {
    options.UseMySql(masterConnectionString, ServerVersion.AutoDetect(masterConnectionString));
});

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

builder.Services.AddControllers();
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
    try 
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        // 1. Seed Master Tenant
        await MasterDbInitializer.SeedTenantAsync(services);
        
        // 2. Migrate Application DB (Identity tables in Master)
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        // 3. Seed SuperAdmin
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await DbInitializer.SeedUsersAsync(userManager, roleManager, app.Configuration);

        // 4. Migrate and Seed all Tenants
        var migrationService = services.GetRequiredService<DatabaseMigrationService>();
        await migrationService.MigrateAllAsync();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(ex, "A fatal error occurred during startup migration/seeding.");
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

app.UseCors("AllowFrontend");
app.UseMiddleware<TenantDetectionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Logger.LogInformation("### SERVER READY - Waiting for requests on port 8080 (Mapped to 5005) ###");
app.Run();
