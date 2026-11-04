using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using LedgerFlow.Api.Tests.Infrastructure;
using LedgerFlow.Application.Features.Users.Commands.LoginUser;
using LedgerFlow.Application.Features.Users.Commands.RegisterUser;

namespace LedgerFlow.Api.Tests.Controllers;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreatedAndUser()
    {
        // Arrange
        var command = new RegisterUserCommand(
            $"test_{Guid.NewGuid():N}@ledgerflow.com",
            "SecureP@ssw0rd123!",
            "Joel",
            "Pro");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var user = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
        Assert.NotNull(user);
        Assert.Equal(command.Email.ToLowerInvariant(), user.Email);
        Assert.Equal("Joel", user.FirstName);
        Assert.Equal("Pro", user.LastName);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsBadRequest()
    {
        // Arrange
        string email = $"duplicate_{Guid.NewGuid():N}@ledgerflow.com";
        var command1 = new RegisterUserCommand(email, "Password123!", "First", "User");
        var command2 = new RegisterUserCommand(email, "Password456!", "Second", "User");

        var firstResponse = await _client.PostAsJsonAsync("/api/auth/register", command1);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act
        var secondResponse = await _client.PostAsJsonAsync("/api/auth/register", command2);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkAndValidJwtToken()
    {
        // Arrange
        string email = $"login_{Guid.NewGuid():N}@ledgerflow.com";
        string password = "CorrectP@ssword999!";

        var registerCommand = new RegisterUserCommand(email, password, "Alice", "Wonderland");
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerCommand);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginCommand = new LoginUserCommand(email, password);

        // Act
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);

        // Assert
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var result = await loginResponse.Content.ReadFromJsonAsync<LoginUserResponse>();
        Assert.NotNull(result);
        Assert.Equal(email.ToLowerInvariant(), result.Email);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));

        // Verify JWT format and claims
        var tokenHandler = new JwtSecurityTokenHandler();
        Assert.True(tokenHandler.CanReadToken(result.Token));

        var jwtToken = tokenHandler.ReadJwtToken(result.Token);
        Assert.Equal(email.ToLowerInvariant(), jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("Alice", jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.GivenName).Value);
        Assert.Equal("Wonderland", jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.FamilyName).Value);
        Assert.Equal(result.Id.ToString(), jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        string email = $"wrongpass_{Guid.NewGuid():N}@ledgerflow.com";
        var registerCommand = new RegisterUserCommand(email, "RealPassword123!", "Bob", "Builder");
        await _client.PostAsJsonAsync("/api/auth/register", registerCommand);

        var loginCommand = new LoginUserCommand(email, "WrongPassword!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
