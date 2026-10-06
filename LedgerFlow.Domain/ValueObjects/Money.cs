namespace LedgerFlow.Domain.ValueObjects;

using LedgerFlow.Domain.Common;

// Money allows negative amounts for reversals and accounting adjustments
public sealed record Money
{
    public long Amount { get; }
    public Currency Currency { get; }

    public bool IsPositive => Amount > 0;
    public bool IsZero => Amount == 0;
    public bool IsNegative => Amount < 0;

    public Money(long amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(Currency currency) => new(0, currency);

    public static Result<Money> FromDecimal(decimal amount, Currency currency)
    {
        ArgumentNullException.ThrowIfNull(currency);

        try
        {
            decimal factor = (decimal)Math.Pow(10, currency.Decimals);
            decimal rounded = Math.Round(amount * factor, 0, MidpointRounding.ToEven);
            long minorUnits = checked((long)rounded);
            return Result.Success(new Money(minorUnits, currency));
        }
        catch (OverflowException)
        {
            return Result.Failure<Money>(MoneyErrors.AmountOutOfRange);
        }
    }

    public decimal ToDecimal()
    {
        decimal factor = (decimal)Math.Pow(10, Currency.Decimals);
        return (decimal)Amount / factor;
    }

    public Result<Money> Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Currency != other.Currency)
        {
            return Result.Failure<Money>(MoneyErrors.CurrencyMismatch);
        }

        try
        {
            long newAmount = checked(Amount + other.Amount);
            return Result.Success(new Money(newAmount, Currency));
        }
        catch (OverflowException)
        {
            return Result.Failure<Money>(MoneyErrors.AmountOutOfRange);
        }
    }

    public Result<Money> Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (Currency != other.Currency)
        {
            return Result.Failure<Money>(MoneyErrors.CurrencyMismatch);
        }

        try
        {
            long newAmount = checked(Amount - other.Amount);
            return Result.Success(new Money(newAmount, Currency));
        }
        catch (OverflowException)
        {
            return Result.Failure<Money>(MoneyErrors.AmountOutOfRange);
        }
    }

    public Result<Money> Multiply(long multiplier)
    {
        try
        {
            long newAmount = checked(Amount * multiplier);
            return Result.Success(new Money(newAmount, Currency));
        }
        catch (OverflowException)
        {
            return Result.Failure<Money>(MoneyErrors.AmountOutOfRange);
        }
    }
}
