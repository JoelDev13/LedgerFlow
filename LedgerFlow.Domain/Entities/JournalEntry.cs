namespace LedgerFlow.Domain.Entities;

using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.ValueObjects;

public sealed class JournalEntry : AggregateRoot<Guid>
{
    private readonly List<JournalEntryLine> _lines = [];

    public Guid OrganizationId { get; private set; }
    public string Description { get; private set; } = default!;
    public DateOnly Date { get; private set; }
    public bool IsPosted { get; private set; }

    public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

    private JournalEntry()
    {
    }

    private JournalEntry(Guid id, Guid organizationId, string description, DateOnly date)
        : base(id)
    {
        OrganizationId = organizationId;
        Description = description;
        Date = date;
        IsPosted = false;
    }

    public static Result<JournalEntry> Create(Guid organizationId, string description, DateOnly date)
    {
        if (organizationId == Guid.Empty)
        {
            return Result.Failure<JournalEntry>(JournalEntryErrors.InvalidOrganizationId);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<JournalEntry>(JournalEntryErrors.DescriptionRequired);
        }

        var entry = new JournalEntry(Guid.NewGuid(), organizationId, description.Trim(), date);
        return Result.Success(entry);
    }

    public Result AddLine(Guid accountId, DebitCredit side, Money amount)
    {
        if (IsPosted)
        {
            return Result.Failure(JournalEntryErrors.AlreadyPosted);
        }

        if (accountId == Guid.Empty)
        {
            return Result.Failure(JournalEntryErrors.InvalidAccountId);
        }

        if (!amount.IsPositive)
        {
            return Result.Failure(JournalEntryErrors.InvalidAmount);
        }

        // Ensure all lines share the same currency
        if (_lines.Count > 0 && _lines[0].Amount.Currency != amount.Currency)
        {
            return Result.Failure(JournalEntryErrors.CurrencyMismatch);
        }

        _lines.Add(new JournalEntryLine(accountId, side, amount));
        return Result.Success();
    }

    public Result Post()
    {
        if (IsPosted)
        {
            return Result.Failure(JournalEntryErrors.AlreadyPosted);
        }

        if (_lines.Count < 2)
        {
            return Result.Failure(JournalEntryErrors.NotEnoughLines);
        }

        long totalDebits = _lines
            .Where(l => l.Side == DebitCredit.Debit)
            .Sum(l => l.Amount.Amount);

        long totalCredits = _lines
            .Where(l => l.Side == DebitCredit.Credit)
            .Sum(l => l.Amount.Amount);

        if (totalDebits != totalCredits)
        {
            return Result.Failure(JournalEntryErrors.ImbalancedEntry);
        }

        IsPosted = true;
        return Result.Success();
    }
}
