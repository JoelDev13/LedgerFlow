namespace LedgerFlow.Domain.Events;

using LedgerFlow.Domain.Common.Interfaces;

public sealed record InvoiceCancelledDomainEvent(Guid InvoiceId, Guid OrganizationId) : IDomainEvent;
