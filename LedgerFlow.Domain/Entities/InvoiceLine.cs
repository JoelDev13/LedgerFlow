namespace LedgerFlow.Domain.Entities;

using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.ValueObjects;

public sealed class InvoiceLine : Entity<Guid>
{
    public string Description { get; private set; } = default!;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = default!;

    public Money Total => UnitPrice.Multiply(Quantity).Value;

    private InvoiceLine()
    {
    }

    internal InvoiceLine(string description, int quantity, Money unitPrice)
        : base(Guid.NewGuid())
    {
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
