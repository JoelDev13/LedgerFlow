using LedgerFlow.Domain.Common.Interfaces;
using LedgerFlow.Domain.ValueObjects;

namespace LedgerFlow.Domain.Events;

public sealed record InvoicePaidDomainEvent(Guid InvoiceId, Guid OrganizationId, Money Amount) : IDomainEvent;
