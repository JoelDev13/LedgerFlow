namespace LedgerFlow.Domain.Entities;

using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.ValueObjects;

public sealed class Invoice : AggregateRoot<Guid>
{
    private readonly List<InvoiceLine> _lines = [];

    public Guid OrganizationId { get; private set; }
    public string Number { get; private set; } = default!;
    public string CustomerName { get; private set; } = default!;
    public Currency Currency { get; private set; } = default!;
    public InvoiceStatus Status { get; private set; }

    public IReadOnlyCollection<InvoiceLine> Lines => _lines.AsReadOnly();

    public Money Total
    {
        get
        {
            if (_lines.Count == 0)
            {
                return Money.Zero(Currency);
            }

            long totalAmount = 0;
            foreach (var line in _lines)
            {
                totalAmount += line.Total.Amount;
            }

            return new Money(totalAmount, Currency);
        }
    }

    private Invoice()
    {
    }

    private Invoice(Guid id, Guid organizationId, string number, string customerName, Currency currency)
        : base(id)
    {
        OrganizationId = organizationId;
        Number = number;
        CustomerName = customerName;
        Currency = currency;
        Status = InvoiceStatus.Draft;
    }

    public static Result<Invoice> Create(Guid organizationId, string number, string customerName, Currency currency)
    {
        if (organizationId == Guid.Empty)
        {
            return Result.Failure<Invoice>(InvoiceErrors.InvalidOrganizationId);
        }

        if (string.IsNullOrWhiteSpace(number))
        {
            return Result.Failure<Invoice>(InvoiceErrors.NumberRequired);
        }

        if (string.IsNullOrWhiteSpace(customerName))
        {
            return Result.Failure<Invoice>(InvoiceErrors.CustomerNameRequired);
        }

        ArgumentNullException.ThrowIfNull(currency);

        var invoice = new Invoice(
            Guid.NewGuid(),
            organizationId,
            number.Trim(),
            customerName.Trim(),
            currency);

        return Result.Success(invoice);
    }

    public Result AddLine(string description, int quantity, Money unitPrice)
    {
        if (Status != InvoiceStatus.Draft)
        {
            return Result.Failure(InvoiceErrors.NotEditable);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure(InvoiceErrors.DescriptionRequired);
        }

        if (quantity <= 0)
        {
            return Result.Failure(InvoiceErrors.InvalidQuantity);
        }

        ArgumentNullException.ThrowIfNull(unitPrice);

        if (!unitPrice.IsPositive)
        {
            return Result.Failure(InvoiceErrors.InvalidUnitPrice);
        }

        if (unitPrice.Currency != Currency)
        {
            return Result.Failure(InvoiceErrors.CurrencyMismatch);
        }

        var line = new InvoiceLine(description.Trim(), quantity, unitPrice);
        _lines.Add(line);

        return Result.Success();
    }

    public Result Send()
    {
        if (Status != InvoiceStatus.Draft)
        {
            return Result.Failure(InvoiceErrors.InvalidStatusTransition);
        }

        if (_lines.Count == 0)
        {
            return Result.Failure(InvoiceErrors.EmptyLines);
        }

        Status = InvoiceStatus.Sent;
        return Result.Success();
    }

    public Result Approve()
    {
        if (Status != InvoiceStatus.Sent)
        {
            return Result.Failure(InvoiceErrors.InvalidStatusTransition);
        }

        Status = InvoiceStatus.Approved;
        return Result.Success();
    }

    public Result MarkAsPaid()
    {
        if (Status == InvoiceStatus.Paid)
        {
            return Result.Failure(InvoiceErrors.AlreadyPaid);
        }

        if (Status != InvoiceStatus.Approved)
        {
            return Result.Failure(InvoiceErrors.InvalidStatusTransition);
        }

        Status = InvoiceStatus.Paid;
        AddDomainEvent(new Events.InvoicePaidDomainEvent(Id, OrganizationId, Total));
        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == InvoiceStatus.Paid)
        {
            return Result.Failure(InvoiceErrors.CannotCancelPaid);
        }

        if (Status == InvoiceStatus.Cancelled)
        {
            return Result.Failure(InvoiceErrors.InvalidStatusTransition);
        }

        Status = InvoiceStatus.Cancelled;
        AddDomainEvent(new Events.InvoiceCancelledDomainEvent(Id, OrganizationId));
        return Result.Success();
    }
}
