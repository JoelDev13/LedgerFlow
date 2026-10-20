namespace LedgerFlow.Domain.Tests.Entities;

using LedgerFlow.Domain.Entities;
using LedgerFlow.Domain.ValueObjects;

public class AccountTests
{
    // Verifies that a valid Account is created with correct properties and IsActive = true
    [Fact]
    public void Create_WithValidParameters_ShouldReturnActiveAccount()
    {
        var orgId = Guid.NewGuid();

        var result = Account.Create(orgId, "1001", "Cash", AccountType.Asset);

        Assert.True(result.IsSuccess);
        Assert.Equal("1001", result.Value.Code);
        Assert.Equal("Cash", result.Value.Name);
        Assert.Equal(AccountType.Asset, result.Value.Type);
        Assert.Equal(orgId, result.Value.OrganizationId);
        Assert.True(result.Value.IsActive);
    }

    // Verifies that Create fails when code is null or empty
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenCodeIsNullOrEmpty_ShouldReturnFailure(string? code)
    {
        var result = Account.Create(Guid.NewGuid(), code!, "Cash", AccountType.Asset);

        Assert.True(result.IsFailure);
        Assert.Equal("Account.CodeRequired", result.Error.Code);
    }

    // Verifies that Create fails when name is null or empty
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenNameIsNullOrEmpty_ShouldReturnFailure(string? name)
    {
        var result = Account.Create(Guid.NewGuid(), "1001", name!, AccountType.Asset);

        Assert.True(result.IsFailure);
        Assert.Equal("Account.NameRequired", result.Error.Code);
    }

    // Verifies that Deactivate sets IsActive to false
    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var account = Account.Create(Guid.NewGuid(), "1001", "Cash", AccountType.Asset).Value;

        account.Deactivate();

        Assert.False(account.IsActive);
    }

    // Verifies that Activate sets IsActive back to true
    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        var account = Account.Create(Guid.NewGuid(), "1001", "Cash", AccountType.Asset).Value;
        account.Deactivate();

        account.Activate();

        Assert.True(account.IsActive);
    }
}
