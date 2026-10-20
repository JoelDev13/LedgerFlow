namespace LedgerFlow.Domain.Tests.Entities;

using LedgerFlow.Domain.Entities;
using LedgerFlow.Domain.ValueObjects;

public class JournalEntryTests
{
    private static readonly Guid OrgId = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    private static Money Usd(decimal amount) =>
        Money.FromDecimal(amount, Currency.Usd).Value;

    // Verifies that a valid balanced entry posts successfully
    [Fact]
    public void Post_WhenDebitsEqualCredits_ShouldSucceed()
    {
        var entry = JournalEntry.Create(OrgId, "Sales revenue", Today).Value;
        var cashAccount = Guid.NewGuid();
        var revenueAccount = Guid.NewGuid();

        entry.AddLine(cashAccount, DebitCredit.Debit, Usd(1000));
        entry.AddLine(revenueAccount, DebitCredit.Credit, Usd(1000));

        var result = entry.Post();

        Assert.True(result.IsSuccess);
        Assert.True(entry.IsPosted);
    }

    // Verifies that posting fails when debits do not equal credits
    [Fact]
    public void Post_WhenDebitsDoNotEqualCredits_ShouldReturnImbalancedError()
    {
        var entry = JournalEntry.Create(OrgId, "Bad entry", Today).Value;

        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, Usd(1000));
        entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, Usd(800));

        var result = entry.Post();

        Assert.True(result.IsFailure);
        Assert.Equal("JournalEntry.ImbalancedEntry", result.Error.Code);
    }

    // Verifies that posting fails when there are fewer than 2 lines
    [Fact]
    public void Post_WhenLessThanTwoLines_ShouldReturnNotEnoughLinesError()
    {
        var entry = JournalEntry.Create(OrgId, "Incomplete entry", Today).Value;
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, Usd(100));

        var result = entry.Post();

        Assert.True(result.IsFailure);
        Assert.Equal("JournalEntry.NotEnoughLines", result.Error.Code);
    }

    // Verifies that a posted entry cannot have lines added to it
    [Fact]
    public void AddLine_WhenAlreadyPosted_ShouldReturnAlreadyPostedError()
    {
        var entry = JournalEntry.Create(OrgId, "Posted entry", Today).Value;
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, Usd(500));
        entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, Usd(500));
        entry.Post();

        var result = entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, Usd(100));

        Assert.True(result.IsFailure);
        Assert.Equal("JournalEntry.AlreadyPosted", result.Error.Code);
    }

    // Verifies that posting an already-posted entry fails
    [Fact]
    public void Post_WhenAlreadyPosted_ShouldReturnAlreadyPostedError()
    {
        var entry = JournalEntry.Create(OrgId, "Posted twice", Today).Value;
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, Usd(200));
        entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, Usd(200));
        entry.Post();

        var result = entry.Post();

        Assert.True(result.IsFailure);
        Assert.Equal("JournalEntry.AlreadyPosted", result.Error.Code);
    }

    // Verifies that adding a line with zero or negative amount fails
    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void AddLine_WhenAmountIsZeroOrNegative_ShouldReturnInvalidAmountError(decimal amount)
    {
        var entry = JournalEntry.Create(OrgId, "Entry with bad amount", Today).Value;
        var badMoney = new Money((long)amount, Currency.Usd);

        var result = entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, badMoney);

        Assert.True(result.IsFailure);
        Assert.Equal("JournalEntry.InvalidAmount", result.Error.Code);
    }

    // Verifies that mixing currencies across lines returns CurrencyMismatch error
    [Fact]
    public void AddLine_WhenCurrencyDiffers_ShouldReturnCurrencyMismatchError()
    {
        var entry = JournalEntry.Create(OrgId, "Multi-currency entry", Today).Value;
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, Usd(500));

        var dopMoney = Money.FromDecimal(500m, Currency.Dop).Value;
        var result = entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, dopMoney);

        Assert.True(result.IsFailure);
        Assert.Equal("JournalEntry.CurrencyMismatch", result.Error.Code);
    }

    // Verifies that Create fails when description is empty
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenDescriptionIsNullOrEmpty_ShouldReturnFailure(string? description)
    {
        var result = JournalEntry.Create(OrgId, description!, Today);

        Assert.True(result.IsFailure);
        Assert.Equal("JournalEntry.DescriptionRequired", result.Error.Code);
    }

    // Verifies a complex balanced entry with multiple debits and credits posts correctly
    [Fact]
    public void Post_WhenMultipleLinesBalanced_ShouldSucceed()
    {
        var entry = JournalEntry.Create(OrgId, "Multi-line purchase", Today).Value;

        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, Usd(600));
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, Usd(400));
        entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, Usd(700));
        entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, Usd(300));

        var result = entry.Post();

        Assert.True(result.IsSuccess);
        Assert.Equal(4, entry.Lines.Count);
    }
}
