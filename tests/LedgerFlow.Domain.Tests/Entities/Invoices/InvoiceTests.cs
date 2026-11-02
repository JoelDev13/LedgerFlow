namespace LedgerFlow.Domain.Tests.Entities;

using LedgerFlow.Domain.Entities;
using LedgerFlow.Domain.Events;
using LedgerFlow.Domain.ValueObjects;

public class InvoiceTests
{
    // Verifies that Total correctly calculates the sum of line items for 3 added lines
    [Fact]
    public void Total_WithThreeLines_ShouldCalculateCorrectSum()
    {
        var orgId = Guid.NewGuid();
        var invoice = Invoice.Create(orgId, "INV-001", "Acme Corp", Currency.Usd).Value;

        invoice.AddLine("Consulting", 2, Money.FromDecimal(50.00m, Currency.Usd).Value);
        invoice.AddLine("Hosting", 1, Money.FromDecimal(150.00m, Currency.Usd).Value);
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

    // Verifies that sending an invoice without lines returns EmptyLines error
    [Fact]
    public void Send_WhenInvoiceHasNoLines_ShouldReturnFailure()
    {
        var orgId = Guid.NewGuid();
        var invoice = Invoice.Create(orgId, "INV-006", "Oscorp", Currency.Usd).Value;

        var result = invoice.Send();

        Assert.True(result.IsFailure);
        Assert.Equal("Invoice.EmptyLines", result.Error.Code);
    }

    // Verifies the complete happy path transitions from Draft to Paid and records InvoicePaidDomainEvent
    [Fact]
    public void Lifecycle_WhenProgressingFromDraftToPaid_ShouldTransitionAndEmitDomainEvent()
    {
        var orgId = Guid.NewGuid();
        var invoice = Invoice.Create(orgId, "INV-007", "Stark Corp", Currency.Usd).Value;
        invoice.AddLine("Armor Maintenance", 1, Money.FromDecimal(1000.00m, Currency.Usd).Value);

        var sendResult = invoice.Send();
        Assert.True(sendResult.IsSuccess);
        Assert.Equal(InvoiceStatus.Sent, invoice.Status);

        var approveResult = invoice.Approve();
        Assert.True(approveResult.IsSuccess);
        Assert.Equal(InvoiceStatus.Approved, invoice.Status);

        var payResult = invoice.MarkAsPaid();
        Assert.True(payResult.IsSuccess);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);

        Assert.Single(invoice.DomainEvents);
        var paidEvent = Assert.IsType<InvoicePaidDomainEvent>(invoice.DomainEvents.First());
        Assert.Equal(invoice.Id, paidEvent.InvoiceId);
        Assert.Equal(orgId, paidEvent.OrganizationId);
        Assert.Equal(invoice.Total, paidEvent.Amount);
    }

    // Verifies that marking an already paid invoice as paid returns AlreadyPaid error
    [Fact]
    public void MarkAsPaid_WhenAlreadyPaid_ShouldReturnFailure()
    {
        var orgId = Guid.NewGuid();
        var invoice = Invoice.Create(orgId, "INV-008", "Wayne Tech", Currency.Usd).Value;
        invoice.AddLine("Equipment", 1, Money.FromDecimal(500.00m, Currency.Usd).Value);
        invoice.Send();
        invoice.Approve();
        invoice.MarkAsPaid();

        var result = invoice.MarkAsPaid();

        Assert.True(result.IsFailure);
        Assert.Equal("Invoice.AlreadyPaid", result.Error.Code);
    }

    // Verifies that a paid invoice cannot be cancelled and returns CannotCancelPaid error
    [Fact]
    public void Cancel_WhenInvoiceIsAlreadyPaid_ShouldReturnFailure()
    {
        var orgId = Guid.NewGuid();
        var invoice = Invoice.Create(orgId, "INV-009", "LexCorp", Currency.Usd).Value;
        invoice.AddLine("Software", 1, Money.FromDecimal(250.00m, Currency.Usd).Value);
        invoice.Send();
        invoice.Approve();
        invoice.MarkAsPaid();

        var result = invoice.Cancel();

        Assert.True(result.IsFailure);
        Assert.Equal("Invoice.CannotCancelPaid", result.Error.Code);
    }

    // Verifies that cancelling an approved invoice succeeds and emits InvoiceCancelledDomainEvent
    [Fact]
    public void Cancel_WhenInvoiceIsApproved_ShouldSucceedAndEmitDomainEvent()
    {
        var orgId = Guid.NewGuid();
        var invoice = Invoice.Create(orgId, "INV-010", "Daily Planet", Currency.Usd).Value;
        invoice.AddLine("Newsprint", 1, Money.FromDecimal(100.00m, Currency.Usd).Value);
        invoice.Send();
        invoice.Approve();

        var result = invoice.Cancel();

        Assert.True(result.IsSuccess);
        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
        Assert.Single(invoice.DomainEvents);
        var cancelledEvent = Assert.IsType<InvoiceCancelledDomainEvent>(invoice.DomainEvents.First());
        Assert.Equal(invoice.Id, cancelledEvent.InvoiceId);
        Assert.Equal(orgId, cancelledEvent.OrganizationId);
    }

    // Verifies that adding lines to an invoice after it has been sent returns NotEditable error
    [Fact]
    public void AddLine_WhenInvoiceIsNotInDraft_ShouldReturnFailure()
    {
        var orgId = Guid.NewGuid();
        var invoice = Invoice.Create(orgId, "INV-011", "Umbrella Corp", Currency.Usd).Value;
        invoice.AddLine("Lab supplies", 1, Money.FromDecimal(200.00m, Currency.Usd).Value);
        invoice.Send();

        var result = invoice.AddLine("Extra test tubes", 2, Money.FromDecimal(50.00m, Currency.Usd).Value);

        Assert.True(result.IsFailure);
        Assert.Equal("Invoice.NotEditable", result.Error.Code);
    }
}
