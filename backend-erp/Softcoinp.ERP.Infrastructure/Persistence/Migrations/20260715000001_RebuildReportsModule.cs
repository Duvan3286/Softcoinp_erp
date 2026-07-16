using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    public partial class RebuildReportsModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Remove old accounting report types ──────────────────────
            migrationBuilder.Sql(@"
DELETE FROM erp_generated_reports
WHERE ReportTypeId IN (
    SELECT Id FROM erp_report_types
    WHERE ReportTypeCode IN (
        'ContingencyFund', 'CashFlow', 'CommonAreaUsage',
        'CommunicationSummary', 'CouncilHistory', 'OwnerRegistry',
        'PortfolioProjection', 'ProviderPayments',
        'PortfolioAging', 'PortfolioByUnit', 'TopDebtors',
        'PeriodCollection', 'PQRSummary', 'MaintenanceSummary',
        'AssemblyMinutes', 'AssemblyDecisions', 'AssemblyQuorum'
    )
);

DELETE FROM erp_recurring_report_configs
WHERE ReportTypeId IN (
    SELECT Id FROM erp_report_types
    WHERE ReportTypeCode IN (
        'ContingencyFund', 'CashFlow', 'CommonAreaUsage',
        'CommunicationSummary', 'CouncilHistory', 'OwnerRegistry',
        'PortfolioProjection', 'ProviderPayments',
        'PortfolioAging', 'PortfolioByUnit', 'TopDebtors',
        'PeriodCollection', 'PQRSummary', 'MaintenanceSummary',
        'AssemblyMinutes', 'AssemblyDecisions', 'AssemblyQuorum'
    )
);

DELETE FROM erp_pdf_templates
WHERE ReportTypeCode IN (
    'ContingencyFund', 'CashFlow', 'CommonAreaUsage',
    'CommunicationSummary', 'CouncilHistory', 'OwnerRegistry',
    'PortfolioProjection', 'ProviderPayments',
    'PortfolioAging', 'PortfolioByUnit', 'TopDebtors',
    'PeriodCollection', 'PQRSummary', 'MaintenanceSummary',
    'AssemblyMinutes', 'AssemblyDecisions', 'AssemblyQuorum',
    'BudgetExecution', 'ActiveContracts', 'AnnualManagementReport'
);

DELETE FROM erp_report_types
WHERE ReportTypeCode IN (
    'ContingencyFund', 'CashFlow', 'CommonAreaUsage',
    'CommunicationSummary', 'CouncilHistory', 'OwnerRegistry',
    'PortfolioProjection', 'ProviderPayments',
    'PortfolioAging', 'PortfolioByUnit', 'TopDebtors',
    'PeriodCollection', 'PQRSummary', 'MaintenanceSummary',
    'AssemblyMinutes', 'AssemblyDecisions', 'AssemblyQuorum',
    'BudgetExecution', 'ActiveContracts', 'AnnualManagementReport'
);
");

            // ── Insert new organizational report types ─────────────────
            // Using a fixed seed GUID per type for reproducibility
            var portfolioId = "a1000001-0000-4000-8000-000000000001";
            var collectionId = "a1000001-0000-4000-8000-000000000002";
            var expenseId = "a1000001-0000-4000-8000-000000000003";
            var budgetId = "a1000001-0000-4000-8000-000000000004";
            var contractsId = "a1000001-0000-4000-8000-000000000005";
            var pqrId = "a1000001-0000-4000-8000-000000000006";
            var maintId = "a1000001-0000-4000-8000-000000000007";
            var assemblyId = "a1000001-0000-4000-8000-000000000008";
            var annualId = "a1000001-0000-4000-8000-000000000009";

            var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff");

            var insertSql = $@"
INSERT INTO erp_report_types (Id, TenantId, ReportTypeCode, Name, Description, Category, SourceModules, AllowedRoles, ContainsPersonalData, IsActive) VALUES
('{portfolioId}', '_DEFAULT_', 'PortfolioReport', 'Reporte de Cartera', 'Saldo pendiente por unidad con antiguedad en meses, propietario y total general', 'Portfolio', 'UnitFees,Units,Owners', 'SuperAdmin,Admin,Council,Accountant,Auditor', 1, 1),
('{collectionId}', '_DEFAULT_', 'CollectionReport', 'Reporte de Recaudo del Periodo', 'Pagos recibidos en el periodo con fecha, unidad, propietario, valor, medio de pago y comprobante', 'Portfolio', 'Payments,Units,Owners', 'SuperAdmin,Admin,Accountant,Auditor', 1, 1),
('{expenseId}', '_DEFAULT_', 'ExpenseReport', 'Reporte de Gastos Ejecutados', 'Gastos registrados en el periodo con fecha, proveedor, descripcion, rubro y comprobante', 'Financial', 'Expenses,Providers,BudgetItems', 'SuperAdmin,Admin,Accountant,Auditor', 0, 1),
('{budgetId}', '_DEFAULT_', 'BudgetExecution', 'Reporte de Ejecucion Presupuestal', 'Comparativo de rubros presupuestados vs ejecutados con porcentaje y semaforo', 'Financial', 'Budgets,Expenses', 'SuperAdmin,Admin,Council,Accountant,Auditor', 0, 1),
('{contractsId}', '_DEFAULT_', 'ActiveContracts', 'Reporte de Contratos Activos', 'Contratos vigentes con proveedor, objeto, valor, fechas y evaluacion', 'Operational', 'Contracts,Providers', 'SuperAdmin,Admin,Council', 0, 1),
('{pqrId}', '_DEFAULT_', 'PQRReport', 'Reporte de PQR del Periodo', 'PQR radicadas en el periodo con radicado, tipo, categoria, estado y tiempo de respuesta', 'Operational', 'PqrRecords', 'SuperAdmin,Admin,Council', 1, 1),
('{maintId}', '_DEFAULT_', 'MaintenanceReport', 'Reporte de Mantenimientos Ejecutados', 'Ordenes de trabajo completadas con bien, tipo, proveedor, costo real y resultado', 'Operational', 'WorkOrders,Assets,Providers', 'SuperAdmin,Admin,Council', 0, 1),
('{assemblyId}', '_DEFAULT_', 'AssemblyReport', 'Reporte de Asambleas y Decisiones', 'Asambleas realizadas con fecha, tipo, quorum y decisiones aprobadas', 'Assembly', 'Assemblies,AgendaItems', 'SuperAdmin,Admin,Council', 0, 1),
('{annualId}', '_DEFAULT_', 'AnnualManagementReport', 'Informe de Gestion Anual', 'Documento consolidado con resumen ejecutivo, cartera, presupuesto, PQR, contratos y asambleas', 'Annual', 'All', 'SuperAdmin,Admin,Council', 0, 1);
";

            migrationBuilder.Sql(insertSql);

            // ── Modify erp_pdf_templates: make ReportTypeCode nullable, add IsGlobal ──
            // MySQL: drop the index first, then modify column
            migrationBuilder.DropIndex(
                name: "IX_erp_pdf_templates_TenantId_ReportTypeCode",
                table: "erp_pdf_templates");

            // Drop the old column and recreate as nullable
            // Actually we need to handle the FK constraint first
            migrationBuilder.Sql(@"
ALTER TABLE erp_pdf_templates MODIFY COLUMN ReportTypeCode varchar(40) NULL;
ALTER TABLE erp_pdf_templates ADD COLUMN IsGlobal tinyint(1) NOT NULL DEFAULT 1;
");

            // Create new index on just TenantId
            migrationBuilder.CreateIndex(
                name: "IX_erp_pdf_templates_TenantId_IsGlobal",
                table: "erp_pdf_templates",
                columns: new[] { "TenantId", "IsGlobal" });

            // Insert a default global template
            migrationBuilder.Sql($@"
INSERT INTO erp_pdf_templates (Id, TenantId, ReportTypeCode, LogoFilePath, HeaderText, FooterText, SignatureName, SignatureRole, ConfidentialityNote, DisclaimerNote, PrimaryColor, SecondaryColor, IsGlobal, CreatedAt, UpdatedAt, CreatedByUserId)
VALUES ('b2000001-0000-4000-8000-000000000001', '_DEFAULT_', NULL, NULL, 'Propiedad Horizontal', 'Documento generado por el sistema de gestion', 'Administrador', 'Administrador', 'ESTE DOCUMENTO CONTIENE DATOS PERSONALES PROTEGIDOS POR LA LEY 1581 DE 2012', 'Los datos aqui contenidos corresponden al momento de generacion y pueden diferir de los datos actuales si se han registrado movimientos posteriores.', '#059669', '#1e293b', 1, '{now}', NULL, 'SYSTEM');
");

            // ── Add ConsecutiveNumber to erp_generated_reports ──
            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveNumber",
                table: "erp_generated_reports",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse: not practical for data changes. This is a destructive migration.
            // Would require restoring from backup.
        }
    }
}
