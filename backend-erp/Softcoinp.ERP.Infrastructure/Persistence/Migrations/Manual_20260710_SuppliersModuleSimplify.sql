-- ============================================================================
-- Migration: Simplify Suppliers and Contracts Module
-- Description: Removes tax/fiscal logic, contract policies, and unused columns.
-- Applied after: 20260710161358_RemovePaymentAgreementsAndInterest
-- ============================================================================

-- Step 1: Drop obsolete tables
DROP TABLE IF EXISTS `erp_retention_configurations`;
DROP TABLE IF EXISTS `erp_contract_policies`;

-- Step 2: Alter erp_providers - remove unused columns
ALTER TABLE `erp_providers`
  DROP COLUMN IF EXISTS `EconomicActivity`,
  DROP COLUMN IF EXISTS `LegalRepDocumentType`,
  DROP COLUMN IF EXISTS `LegalRepDocumentNumber`,
  DROP COLUMN IF EXISTS `LegalRepName`,
  DROP COLUMN IF EXISTS `LegalRepEmail`,
  DROP COLUMN IF EXISTS `TradeName`,
  DROP COLUMN IF EXISTS `City`,
  DROP COLUMN IF EXISTS `VerificationDigit`,
  DROP COLUMN IF EXISTS `IsPreferred`,
  ADD COLUMN `ChamberOfCommerceFilePath` varchar(1000) DEFAULT '' AFTER `RutFilePath`;

-- Step 3: Alter erp_contracts - remove/rename columns
ALTER TABLE `erp_contracts`
  DROP COLUMN IF EXISTS `AssemblyMeetingActNumber`,
  ADD COLUMN `Observations` varchar(2000) DEFAULT '' AFTER `SignedContractFilePath`;

-- Step 4: Alter erp_provider_invoices - replace tax fields with simplified model
ALTER TABLE `erp_provider_invoices`
  DROP COLUMN IF EXISTS `Subtotal`,
  DROP COLUMN IF EXISTS `IvaAmount`,
  DROP COLUMN IF EXISTS `RetentionFuelAmount`,
  DROP COLUMN IF EXISTS `RetentionIcaAmount`,
  DROP COLUMN IF EXISTS `NetAmount`,
  DROP COLUMN IF EXISTS `Description`,
  DROP COLUMN IF EXISTS `InvoiceFilePath`,
  ADD COLUMN `TotalAmount` decimal(18,2) NOT NULL DEFAULT 0 AFTER `DueDate`,
  ADD COLUMN `AmountPaid` decimal(18,2) NOT NULL DEFAULT 0 AFTER `TotalAmount`,
  ADD COLUMN `PaymentDate` datetime(6) NULL AFTER `AmountPaid`,
  ADD COLUMN `PaymentMethod` varchar(20) NULL AFTER `PaymentDate`,
  ADD COLUMN `PaymentReferenceNumber` varchar(100) DEFAULT '' AFTER `PaymentMethod`,
  ADD COLUMN `BudgetItemId` char(36) NULL AFTER `PaymentReferenceNumber`;

-- Step 5: Alter erp_provider_payments - remove unused columns
ALTER TABLE `erp_provider_payments`
  DROP COLUMN IF EXISTS `BankAccount`,
  DROP COLUMN IF EXISTS `Notes`,
  DROP COLUMN IF EXISTS `ReceiptFilePath`;

-- Step 6: Alter erp_provider_evaluations - rename score columns and drop ContractId
SET @exists = (SELECT COUNT(*) FROM information_schema.COLUMNS 
  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'erp_provider_evaluations' AND COLUMN_NAME = 'ContractId');
SET @stmt = IF(@exists > 0, 'ALTER TABLE `erp_provider_evaluations` DROP FOREIGN KEY IF EXISTS `FK_erp_provider_evaluations_erp_contracts_ContractId`, DROP COLUMN `ContractId`', 'SELECT 1');
PREPARE stmt FROM @stmt;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

ALTER TABLE `erp_provider_evaluations`
  CHANGE COLUMN `ServiceQualityScore` `QualityScore` int NOT NULL,
  CHANGE COLUMN `PriceFairnessScore` `PriceScore` int NOT NULL,
  CHANGE COLUMN `AfterSalesScore` `AttentionScore` int NOT NULL;
