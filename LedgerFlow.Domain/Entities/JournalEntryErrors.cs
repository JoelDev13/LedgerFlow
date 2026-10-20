namespace LedgerFlow.Domain.Entities;

using LedgerFlow.Domain.Common;

public static class JournalEntryErrors
{
    public static readonly Error InvalidOrganizationId = new(
        "JournalEntry.InvalidOrganizationId",
        "Organization ID is invalid");

    public static readonly Error DescriptionRequired = new(
        "JournalEntry.DescriptionRequired",
        "Journal entry description is required");

    public static readonly Error ImbalancedEntry = new(
        "JournalEntry.ImbalancedEntry",
        "Total debits must equal total credits");

    public static readonly Error NotEnoughLines = new(
        "JournalEntry.NotEnoughLines",
        "A journal entry must have at least two lines");

    public static readonly Error AlreadyPosted = new(
        "JournalEntry.AlreadyPosted",
        "A posted journal entry cannot be modified");

    public static readonly Error InvalidAmount = new(
        "JournalEntry.InvalidAmount",
        "Amount must be greater than zero");

    public static readonly Error InvalidAccountId = new(
        "JournalEntry.InvalidAccountId",
        "Account ID is invalid");

    public static readonly Error CurrencyMismatch = new(
        "JournalEntry.CurrencyMismatch",
        "All lines must use the same currency");
}
