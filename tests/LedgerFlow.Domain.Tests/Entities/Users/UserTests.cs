namespace LedgerFlow.Domain.Tests.Entities;

using LedgerFlow.Domain.Entities;

public class UserTests
{
    // Valid user creation returns a User with normalized email and correct properties
    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = User.Create("Joel@Example.COM", "hashed_pw", "Joel", "smith");

        Assert.True(result.IsSuccess);
        Assert.Equal("joel@example.com", result.Value.Email);
        Assert.Equal("hashed_pw", result.Value.PasswordHash);
        Assert.Equal("Joel", result.Value.FirstName);
        Assert.Equal("smith", result.Value.LastName);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    // Email is required
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ReturnsEmailRequiredError(string? email)
    {
        var result = User.Create(email!, "hash", "Joel", "smith");

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.EmailRequired, result.Error);
    }

    // Email must contain @
    [Theory]
    [InlineData("notanemail")]
    [InlineData("missingatsign.com")]
    public void Create_WithInvalidEmail_ReturnsInvalidEmailError(string email)
    {
        var result = User.Create(email, "hash", "Joel", "smith");

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.InvalidEmail, result.Error);
    }

    // Password hash is required
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyPasswordHash_ReturnsPasswordRequiredError(string? passwordHash)
    {
        var result = User.Create("joel@example.com", passwordHash!, "Joel", "smith");

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.PasswordRequired, result.Error);
    }

    // First name is required
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyFirstName_ReturnsFirstNameRequiredError(string? firstName)
    {
        var result = User.Create("joel@example.com", "hash", firstName!, "smith");

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.FirstNameRequired, result.Error);
    }

    // Last name is required
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyLastName_ReturnsLastNameRequiredError(string? lastName)
    {
        var result = User.Create("joel@example.com", "hash", "Joel", lastName!);

        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.LastNameRequired, result.Error);
    }

    // Email is normalized (trimmed + lowercased)
    [Fact]
    public void Create_EmailIsNormalizedToLowercase()
    {
        var result = User.Create("  JOEL@EXAMPLE.COM  ", "hash", "Joel", "smith");

        Assert.True(result.IsSuccess);
        Assert.Equal("joel@example.com", result.Value.Email);
    }

    // Names are trimmed
    [Fact]
    public void Create_NamesAreTrimmed()
    {
        var result = User.Create("joel@example.com", "hash", "  Joel  ", "  Smith  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Joel", result.Value.FirstName);
        Assert.Equal("Smith", result.Value.LastName);
    }

    // Each user gets a unique Id
    [Fact]
    public void Create_TwoUsers_HaveDifferentIds()
    {
        var u1 = User.Create("a@a.com", "hash", "A", "A").Value;
        var u2 = User.Create("b@b.com", "hash", "B", "B").Value;

        Assert.NotEqual(u1.Id, u2.Id);
    }

    // CreatedAt is set to UTC now
    [Fact]
    public void Create_SetsCreatedAtToUtcNow()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var result = User.Create("joel@example.com", "hash", "Joel", "smith");
        var after = DateTime.UtcNow.AddSeconds(1);

        Assert.True(result.Value.CreatedAt >= before);
        Assert.True(result.Value.CreatedAt <= after);
    }
}

