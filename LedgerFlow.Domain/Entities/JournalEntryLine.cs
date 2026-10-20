namespace LedgerFlow.Domain.Entities;

using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.ValueObjects;

public enum DebitCredit
{
    Debit = 1,
    Credit = 2
}

public sealed class JournalEntryLine : Entity<Guid>
{
    public Guid AccountId { get; private set; }
    public DebitCredit Side { get; private set; }
    public Money Amount { get; private set; } = default!;

    private JournalEntryLine()
    {
    }

    internal JournalEntryLine(Guid accountId, DebitCredit side, Money amount)
        : base(Guid.NewGuid())
    {
        AccountId = accountId;
        Side = side;
        Amount = amount;
    }
}
