namespace Softcoinp.ERP.Domain.Enums;

public enum AlertRuleType
{
    ProviderContractExpiring,
    PreventiveMaintenanceDue,
    PqrOverdue,
    PaymentAgreementInstallmentOverdue,
    BudgetAccountExceeded,
    AccountingPeriodNotClosed,
    LatePaymentThreshold
}
