namespace LedgerFlow.Domain.Tests.Entities;

using LedgerFlow.Domain.Entities;
using LedgerFlow.Domain.ValueObjects;

public class InvoiceTests
{
    // Verifies that Total correctly calculates the sum of line items for 3 added lines
    [Fact]
    public void Total_WithThreeLines_ShouldCalculateCorrectSum()
    {
        var orgId = Guid.NewGuid();
        var invoice = Invoice.Create(orgId, "INV-001", "Acme Corp", Currency.Usd).Value;

        //2 x $50.00 = $100.00
        invoice.AddLine("Consulting", 2, Money.FromDecimal(50.00m, Currency.Usd).Value);
        //1 x $150.00 = $150.00
        invoice.AddLine("Hosting", 1, Money.FromDecimal(150.00m, Currency.Usd).Value);
        //5 x $10.00 = $50.00
        invoice.AddLine("Domain licenses", 5, Money.FromDecimal(10.00m, Currency.Usd).Value);

        Assert.Equal(3, invoice.Lines.Count);
        Assert.Equal(300.00m, invoice.Total.ToDecimal());
        Assert.Equal(Currency.Usd, invoice.Total.Currency);
    }

    // Verifies that adding a line with a different currency than the invoice fails
    [Fact]
    public void AddLine_WhenCurrencyMismatch_ShouldReturnFailure()
    {
        var orgId = Guid.NewGuid();
        var invoice = Invoice.Create(orgId, "INV-002", "Globex", Currency.Usd).Value;
        var dopPrice = Money.FromDecimal(100.00m, Currency.Dop).Value;

        var result = invoice.AddLine("Development", 1, dopPrice);

        Assert.True(result.IsFailure);
        Assert.Equal("Invoice.CurrencyMismatch", result.Error.Code);
    }

    // Verifies that adding a line with zero quantity returns InvalidQuantity error
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddLine_WhenQuantityIsZeroOrNegative_ShouldReturnFailure(int quantity)
    {
        var orgId = Guid.NewGuid();
        var invoice = Invoice.Create(orgId, "INV-003", "Stark Industries", Currency.Usd).Value;
        var unitPrice = Money.FromDecimal(100.00m, Currency.Usd).Value;

        var result = invoice.AddLine("Widget", quantity, unitPrice);

        Assert.True(result.IsFailure);
        Assert.Equal("Invoice.InvalidQuantity", result.Error.Code);
    }

    // Verifies that Create fails when mandatory parameters (number or customer name) are invalid
    [Theory]
    [InlineData(null, "Customer Name", "Invoice.NumberRequired")]
    [InlineData("", "Customer Name", "Invoice.NumberRequired")]
    [InlineData("INV-001", null, "Invoice.CustomerNameRequired")]
    [InlineData("INV-001", "", "Invoice.CustomerNameRequired")]
    public void Create_WhenRequiredFieldsMissing_ShouldReturnFailure(string? number, string? customerName, string expectedErrorCode)
    {
        var result = Invoice.Create(Guid.NewGuid(), number!, customerName!, Currency.Usd);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedErrorCode, result.Error.Code);
    }

    // Verifies that adding a line with non-positive unit price returns InvalidUnitPrice error
    [Fact]
    public void AddLine_WhenUnitPriceIsZeroOrNegative_ShouldReturnFailure()
    {
        var orgId = Guid.NewGuid();
        var invoice = Invoice.Create(orgId, "INV-004", "Wayne Enterprises", Currency.Usd).Value;
        var zeroPrice = Money.Zero(Currency.Usd);

        var result = invoice.AddLine("Service", 1, zeroPrice);

        Assert.True(result.IsFailure);
        Assert.Equal("Invoice.InvalidUnitPrice", result.Error.Code);
    }

    // Verifies that empty line description returns DescriptionRequired error
    [Fact]
    public void AddLine_WhenDescriptionIsEmpty_ShouldReturnFailure()
    {
        var orgId = Guid.NewGuid();
        var invoice = Invoice.Create(orgId, "INV-005", "Cyberdyne", Currency.Usd).Value;
        var price = Money.FromDecimal(50.00m, Currency.Usd).Value;

        var result = invoice.AddLine("   ", 1, price);

        Assert.True(result.IsFailure);
        Assert.Equal("Invoice.DescriptionRequired", result.Error.Code);
    }
}
