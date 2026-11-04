using LedgerFlow.Domain.Common.Interfaces;

namespace LedgerFlow.Domain.Events;

public sealed record InvoiceCancelledDomainEvent(Guid InvoiceId, Guid OrganizationId) : IDomainEvent;
