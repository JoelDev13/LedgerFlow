namespace LedgerFlow.Domain.ValueObjects;

using LedgerFlow.Domain.Common;

public static class MoneyErrors
{
    public static readonly Error CurrencyMismatch = new(
        "Money.CurrencyMismatch",
        "Cannot perform arithmetic operations on money with different currencies");

    public static readonly Error AmountOutOfRange = new(
        "Money.AmountOutOfRange",
        "Money amount is out of range");
}
