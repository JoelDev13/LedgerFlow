namespace LedgerFlow.Domain.ValueObjects;

using LedgerFlow.Domain.Common;

public static class CurrencyErrors
{
    public static readonly Error NullOrEmptyCode = new(
        "Currency.NullOrEmptyCode",
        "Currency code cannot be null or empty");

    public static Error InvalidCode(string code) => new(
        "Currency.InvalidCode",
        $"Currency code '{code}' is not supported");
}
