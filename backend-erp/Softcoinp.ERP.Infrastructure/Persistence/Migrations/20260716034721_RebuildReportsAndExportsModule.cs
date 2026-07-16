using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Softcoinp.ERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Reemplaza a la migracion escrita a mano 20260715000001_RebuildReportsModule, que nunca
    /// tuvo su archivo .Designer.cs y por lo tanto EF Core nunca la reconocio ni la aplico en
    /// ningun entorno (el atributo [Migration] vive en el Designer.cs). Esta migracion cubre el
    /// mismo objetivo: elimina los tipos de reporte contables/mixtos originales, siembra los 10
    /// tipos organizacionales del modulo reescrito (incluido AccountantExport) y alinea el
    /// esquema de erp_pdf_templates/erp_generated_reports con las entidades actuales.
    /// </remarks>
    public partial class RebuildReportsAndExportsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @tenant_id = (SELECT TenantId FROM erp_report_types LIMIT 1);

DELETE FROM erp_generated_reports
WHERE ReportTypeId IN (SELECT Id FROM erp_report_types WHERE TenantId = @tenant_id);

DELETE FROM erp_recurring_report_configs
WHERE ReportTypeId IN (SELECT Id FROM erp_report_types WHERE TenantId = @tenant_id);

DELETE FROM erp_pdf_templates WHERE TenantId = @tenant_id;

DELETE FROM erp_report_types WHERE TenantId = @tenant_id;
");

            migrationBuilder.DropIndex(
                name: "IX_erp_pdf_templates_TenantId_ReportTypeCode",
                table: "erp_pdf_templates");

            migrationBuilder.DropColumn(
                name: "ReportTypeCode",
                table: "erp_pdf_templates");

            migrationBuilder.RenameColumn(
                name: "IsDefault",
                table: "erp_pdf_templates",
                newName: "IsGlobal");

            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveNumber",
                table: "erp_generated_reports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_erp_pdf_templates_TenantId_IsGlobal",
                table: "erp_pdf_templates",
                columns: new[] { "TenantId", "IsGlobal" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_generated_reports_TenantId_ReportTypeId_ConsecutiveNumber",
                table: "erp_generated_reports",
                columns: new[] { "TenantId", "ReportTypeId", "ConsecutiveNumber" });

            var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff");

            // Los INSERT usan INSERT...SELECT...WHERE @tenant_id IS NOT NULL en vez de VALUES
            // literales porque un tenant recien aprovisionado no tiene ninguna fila previa en
            // erp_report_types cuando esta migracion corre (DbInitializer.SeedReportTypesAsync
            // siembra despues de aplicar todas las migraciones). En ese caso @tenant_id queda
            // NULL y este bloque debe insertar cero filas sin error, dejando que
            // SeedReportTypesAsync haga la siembra inicial normalmente.
            migrationBuilder.Sql($@"
INSERT INTO erp_report_types (Id, TenantId, ReportTypeCode, Name, Description, Category, SourceModules, AllowedRoles, ContainsPersonalData, IsActive)
SELECT UUID(), @tenant_id, v.ReportTypeCode, v.Name, v.Description, v.Category, v.SourceModules, v.AllowedRoles, v.ContainsPersonalData, 1
FROM (
    SELECT 'PortfolioReport' AS ReportTypeCode, 'Reporte de Cartera' AS Name, 'Saldo pendiente por unidad con antiguedad en meses, propietario y total general' AS Description, 'Portfolio' AS Category, 'UnitFees,Units,Owners' AS SourceModules, 'SuperAdmin,Admin,Council,Accountant,Auditor' AS AllowedRoles, 1 AS ContainsPersonalData
    UNION ALL SELECT 'CollectionReport', 'Reporte de Recaudo del Periodo', 'Pagos recibidos en el periodo con fecha, unidad, propietario, valor, medio de pago y comprobante', 'Portfolio', 'Payments,Units,Owners', 'SuperAdmin,Admin,Accountant,Auditor', 1
    UNION ALL SELECT 'ExpenseReport', 'Reporte de Gastos Ejecutados', 'Gastos registrados en el periodo con fecha, proveedor, descripcion, rubro y comprobante', 'Financial', 'Expenses,Providers,BudgetItems', 'SuperAdmin,Admin,Accountant,Auditor', 0
    UNION ALL SELECT 'BudgetExecution', 'Reporte de Ejecucion Presupuestal', 'Comparativo de rubros presupuestados vs ejecutados con porcentaje y semaforo', 'Financial', 'Budgets,Expenses', 'SuperAdmin,Admin,Council,Accountant,Auditor', 0
    UNION ALL SELECT 'ActiveContracts', 'Reporte de Contratos Activos', 'Contratos vigentes con proveedor, objeto, valor, fechas y evaluacion', 'Operational', 'Contracts,Providers', 'SuperAdmin,Admin,Council', 0
    UNION ALL SELECT 'PQRReport', 'Reporte de PQR del Periodo', 'PQR radicadas en el periodo con radicado, tipo, categoria, estado y tiempo de respuesta', 'Operational', 'PqrRecords', 'SuperAdmin,Admin,Council', 1
    UNION ALL SELECT 'MaintenanceReport', 'Reporte de Mantenimientos Ejecutados', 'Ordenes de trabajo completadas con bien, tipo, proveedor, costo real y resultado', 'Operational', 'WorkOrders,Assets,Providers', 'SuperAdmin,Admin,Council', 0
    UNION ALL SELECT 'AssemblyReport', 'Reporte de Asambleas y Decisiones', 'Asambleas realizadas con fecha, tipo, quorum y decisiones aprobadas', 'Assembly', 'Assemblies,AgendaItems', 'SuperAdmin,Admin,Council', 0
    UNION ALL SELECT 'AnnualManagementReport', 'Informe de Gestion Anual', 'Documento consolidado con resumen ejecutivo, cartera, presupuesto, PQR, contratos y asambleas', 'Annual', 'All', 'SuperAdmin,Admin,Council', 0
    UNION ALL SELECT 'AccountantExport', 'Exportacion para el Contador', 'Exportacion en Excel con hojas separadas de ingresos y egresos del periodo, lista para el contador externo', 'Financial', 'Payments,ProviderPayments', 'SuperAdmin,Admin,Accountant,Auditor', 1
) v
WHERE @tenant_id IS NOT NULL;

INSERT INTO erp_pdf_templates (Id, TenantId, LogoFilePath, HeaderText, FooterText, SignatureName, SignatureRole, ConfidentialityNote, DisclaimerNote, PrimaryColor, SecondaryColor, IsGlobal, CreatedAt, UpdatedAt, CreatedByUserId)
SELECT UUID(), @tenant_id, NULL, 'Propiedad Horizontal', 'Documento generado por el sistema de gestion', 'Administrador', 'Administrador', 'ESTE DOCUMENTO CONTIENE DATOS PERSONALES PROTEGIDOS POR LA LEY 1581 DE 2012', 'Los datos aqui contenidos corresponden al momento de generacion y pueden diferir de los datos actuales si se han registrado movimientos posteriores.', '#059669', '#1e293b', 1, '{now}', NULL, 'SYSTEM'
WHERE @tenant_id IS NOT NULL;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_erp_pdf_templates_TenantId_IsGlobal",
                table: "erp_pdf_templates");

            migrationBuilder.DropIndex(
                name: "IX_erp_generated_reports_TenantId_ReportTypeId_ConsecutiveNumber",
                table: "erp_generated_reports");

            migrationBuilder.DropColumn(
                name: "ConsecutiveNumber",
                table: "erp_generated_reports");

            migrationBuilder.RenameColumn(
                name: "IsGlobal",
                table: "erp_pdf_templates",
                newName: "IsDefault");

            migrationBuilder.AddColumn<string>(
                name: "ReportTypeCode",
                table: "erp_pdf_templates",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_erp_pdf_templates_TenantId_ReportTypeCode",
                table: "erp_pdf_templates",
                columns: new[] { "TenantId", "ReportTypeCode" });
        }
    }
}
