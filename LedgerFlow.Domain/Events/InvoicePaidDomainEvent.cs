namespace LedgerFlow.Domain.Events;

using LedgerFlow.Domain.Common.Interfaces;
using LedgerFlow.Domain.ValueObjects;

public sealed record InvoicePaidDomainEvent(Guid InvoiceId, Guid OrganizationId, Money Amount) : IDomainEvent;
