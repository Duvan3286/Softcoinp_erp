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

builder.Services.AddCors(options => {
    options.AddPolicy("AllowFrontend", policy => {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Optional: Automatic migrations on startup
if (app.Configuration.GetValue<bool>("AUTO_MIGRATE") || Environment.GetEnvironmentVariable("AUTO_MIGRATE") == "true")
{
    using var scope = app.Services.CreateScope();
    
    // Seed Master Tenant if not exists
    await MasterDbInitializer.SeedTenantAsync(app.Services);
    
    // For development, we apply migrations directly to the Master DB context 
    // to ensure Identity tables are present there and we can seed users.
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await DbInitializer.SeedUsersAsync(userManager, roleManager);

    var migrationService = scope.ServiceProvider.GetRequiredService<DatabaseMigrationService>();
    await migrationService.MigrateAllAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

// Custom Multi-tenancy Middleware
app.UseMiddleware<TenantDetectionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
