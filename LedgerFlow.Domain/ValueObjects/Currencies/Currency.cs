namespace LedgerFlow.Domain.ValueObjects;

using LedgerFlow.Domain.Common;

public sealed record Currency
{
    public static readonly Currency Usd = new("USD", 2);
    public static readonly Currency Eur = new("EUR", 2);
    public static readonly Currency Htg = new("HTG", 2);
    public static readonly Currency Dop = new("DOP", 2);
    public static readonly Currency Gbp = new("GBP", 2);
    public static readonly Currency Jpy = new("JPY", 0);

    private static readonly Dictionary<string, Currency> AllCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        { Usd.Code, Usd },
        { Eur.Code, Eur },
        { Htg.Code, Htg },
        { Dop.Code, Dop },
        { Gbp.Code, Gbp },
        { Jpy.Code, Jpy }
    };

    public string Code { get; }
    public int Decimals { get; }

    private Currency(string code, int decimals)
    {
        Code = code;
        Decimals = decimals;
    }

    public static Result<Currency> FromCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<Currency>(CurrencyErrors.NullOrEmptyCode);
        }

        string normalized = code.Trim().ToUpperInvariant();

        if (!AllCurrencies.TryGetValue(normalized, out var currency))
        {
            return Result.Failure<Currency>(CurrencyErrors.InvalidCode(normalized));
        }

        return Result.Success(currency);
    }
}
