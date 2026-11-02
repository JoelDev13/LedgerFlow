namespace LedgerFlow.Domain.Entities;

public enum InvoiceStatus
{
    Draft = 1,
    Sent = 2,
    Approved = 3,
    Paid = 4,
    Cancelled = 5
}
