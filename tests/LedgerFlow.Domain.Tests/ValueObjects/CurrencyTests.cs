namespace LedgerFlow.Domain.Tests.ValueObjects;

using LedgerFlow.Domain.ValueObjects;

public class CurrencyTests
{
    // Verifies that FromCode normalizes lowercase and whitespace input to match predefined currency
    [Fact]
    public void FromCode_WhenCodeHasWhitespaceAndMixedCase_ShouldNormalizeAndReturnCurrency()
    {
        var result = Currency.FromCode(" usd ");

        Assert.True(result.IsSuccess);
        Assert.Equal(Currency.Usd, result.Value);
    }

    // Verifies that FromCode returns failure result when an unsupported code is supplied
    [Fact]
    public void FromCode_WhenCodeIsInvalid_ShouldReturnFailure()
    {
        var result = Currency.FromCode("XYZ");

        Assert.True(result.IsFailure);
        Assert.Equal("Currency.InvalidCode", result.Error.Code);
    }

    // Verifies that FromCode returns failure result when null or empty string is supplied
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromCode_WhenCodeIsNullOrEmpty_ShouldReturnFailure(string? code)
    {
        var result = Currency.FromCode(code);

        Assert.True(result.IsFailure);
        Assert.Equal("Currency.NullOrEmptyCode", result.Error.Code);
    }

    // Verifies that JPY currency is configured with zero decimals
    [Fact]
    public void Currency_Jpy_ShouldHaveZeroDecimals()
    {
        Assert.Equal(0, Currency.Jpy.Decimals);
        Assert.Equal("JPY", Currency.Jpy.Code);
    }

    // Verifies that predefined currencies are initialized properly with correct decimal places
    [Fact]
    public void PredefinedCurrencies_ShouldHaveCorrectDecimals()
    {
        Assert.Equal(2, Currency.Usd.Decimals);
        Assert.Equal(2, Currency.Eur.Decimals);
        Assert.Equal(2, Currency.Htg.Decimals);
        Assert.Equal(2, Currency.Dop.Decimals);
        Assert.Equal(2, Currency.Gbp.Decimals);
    }
}
