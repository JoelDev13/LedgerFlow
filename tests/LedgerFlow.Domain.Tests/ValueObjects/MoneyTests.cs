namespace LedgerFlow.Domain.Tests.ValueObjects;

using LedgerFlow.Domain.ValueObjects;

public class MoneyTests
{
    // Verifies that Zero creates a Money instance with zero amount for the specified currency
    [Fact]
    public void Zero_ShouldReturnMoneyWithZeroAmount()
    {
        var money = Money.Zero(Currency.Usd);

        Assert.Equal(0, money.Amount);
        Assert.Equal(Currency.Usd, money.Currency);
        Assert.True(money.IsZero);
        Assert.False(money.IsPositive);
    }

    // Verifies value equality between two Money instances with the same amount and currency
    [Fact]
    public void Equals_WhenSameAmountAndCurrency_ShouldReturnTrue()
    {
        var money1 = new Money(1050, Currency.Usd);
        var money2 = new Money(1050, Currency.Usd);

        Assert.Equal(money1, money2);
        Assert.True(money1 == money2);
        Assert.False(money1 != money2);
    }

    // Verifies that FromDecimal converts decimal amounts to minor units using banker's rounding
    [Theory]
    [InlineData(10.50, 1050)]
    [InlineData(10.005, 1000)] // Banker's rounding rounds 1000.5 to even (1000)
    [InlineData(10.015, 1002)] // Banker's rounding rounds 1001.5 to even (1002)
    [InlineData(10.545, 1054)] // Banker's rounding rounds .545 to even (.54) -> 1054
    [InlineData(10.555, 1056)] // Banker's rounding rounds .555 to even (.56) -> 1056
    public void FromDecimal_ShouldConvertToMinorUnitsWithBankersRounding(decimal input, long expectedMinorUnits)
    {
        var result = Money.FromDecimal(input, Currency.Usd);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedMinorUnits, result.Value.Amount);
    }

    // Verifies that FromDecimal handles zero decimal currencies like JPY correctly
    [Fact]
    public void FromDecimal_WithZeroDecimalCurrency_ShouldNotScaleAmount()
    {
        var result = Money.FromDecimal(500m, Currency.Jpy);

        Assert.True(result.IsSuccess);
        Assert.Equal(500, result.Value.Amount);
        Assert.Equal(500m, result.Value.ToDecimal());
    }

    // Verifies ToDecimal accurately converts minor units back to standard decimal representation
    [Fact]
    public void ToDecimal_ShouldReturnOriginalDecimalRepresentation()
    {
        var money = new Money(12345, Currency.Dop); // 123.45 DOP

        Assert.Equal(123.45m, money.ToDecimal());
    }

    // Verifies that Add succeeds when both Money instances share the same currency
    [Fact]
    public void Add_WhenSameCurrency_ShouldReturnSum()
    {
        var money1 = Money.FromDecimal(10.50m, Currency.Usd).Value;
        var money2 = Money.FromDecimal(5.25m, Currency.Usd).Value;

        var result = money1.Add(money2);

        Assert.True(result.IsSuccess);
        Assert.Equal(15.75m, result.Value.ToDecimal());
    }

    // Verifies that Add fails when Money instances have different currencies
    [Fact]
    public void Add_WhenDifferentCurrency_ShouldReturnFailure()
    {
        var usdMoney = Money.FromDecimal(10m, Currency.Usd).Value;
        var dopMoney = Money.FromDecimal(10m, Currency.Dop).Value;

        var result = usdMoney.Add(dopMoney);

        Assert.True(result.IsFailure);
        Assert.Equal("Money.CurrencyMismatch", result.Error.Code);
    }

    // Verifies that Subtract succeeds when currencies match and allows negative results
    [Fact]
    public void Subtract_WhenSameCurrency_ShouldReturnDifference()
    {
        var money1 = Money.FromDecimal(10m, Currency.Usd).Value;
        var money2 = Money.FromDecimal(15m, Currency.Usd).Value;

        var result = money1.Subtract(money2);

        Assert.True(result.IsSuccess);
        Assert.Equal(-5.00m, result.Value.ToDecimal());
        Assert.True(result.Value.IsNegative);
    }

    // Verifies that Subtract fails when currencies differ
    [Fact]
    public void Subtract_WhenDifferentCurrency_ShouldReturnFailure()
    {
        var usdMoney = Money.FromDecimal(10m, Currency.Usd).Value;
        var dopMoney = Money.FromDecimal(5m, Currency.Dop).Value;

        var result = usdMoney.Subtract(dopMoney);

        Assert.True(result.IsFailure);
        Assert.Equal("Money.CurrencyMismatch", result.Error.Code);
    }

    // Verifies that Multiply correctly multiplies the amount by an integer factor and preserves currency
    [Fact]
    public void Multiply_ByIntegerFactor_ShouldReturnMultipliedMoneyAndPreserveCurrency()
    {
        var money = Money.FromDecimal(12.50m, Currency.Eur).Value;

        var result = money.Multiply(4);

        Assert.True(result.IsSuccess);
        Assert.Equal(50.00m, result.Value.ToDecimal());
        Assert.Equal(Currency.Eur, result.Value.Currency);
    }

    // Verifies that IsPositive returns true for positive amounts and false for zero or negative amounts
    [Theory]
    [InlineData(100, true)]
    [InlineData(0, false)]
    [InlineData(-50, false)]
    public void IsPositive_ShouldReturnExpectedResult(long amount, bool expectedIsPositive)
    {
        var money = new Money(amount, Currency.Usd);

        Assert.Equal(expectedIsPositive, money.IsPositive);
    }

    // Verifies that arithmetic operations catch overflow and return an AmountOutOfRange error
    [Fact]
    public void Arithmetic_WhenOverflowOccurs_ShouldReturnFailure()
    {
        var hugeMoney = new Money(long.MaxValue, Currency.Usd);
        var oneCent = new Money(1, Currency.Usd);

        var result = hugeMoney.Add(oneCent);

        Assert.True(result.IsFailure);
        Assert.Equal("Money.AmountOutOfRange", result.Error.Code);
    }

    // Verifies that FromDecimal with a value exceeding long.MaxValue returns an AmountOutOfRange error
    [Fact]
    public void FromDecimal_WhenValueExceedsRange_ShouldReturnFailure()
    {
        decimal hugeDecimal = decimal.MaxValue;

        var result = Money.FromDecimal(hugeDecimal, Currency.Usd);

        Assert.True(result.IsFailure);
        Assert.Equal("Money.AmountOutOfRange", result.Error.Code);
    }
}
