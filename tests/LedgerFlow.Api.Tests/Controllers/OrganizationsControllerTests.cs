using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LedgerFlow.Api.Controllers;
using LedgerFlow.Api.Tests.Infrastructure;
using LedgerFlow.Application.Features.Organizations;
using LedgerFlow.Application.Features.Users.Commands.LoginUser;
using LedgerFlow.Application.Features.Users.Commands.RegisterUser;
using LedgerFlow.Domain.Entities.Organizations;

namespace LedgerFlow.Api.Tests.Controllers;

public class OrganizationsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OrganizationsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_WithAuthenticatedUser_ReturnsCreatedAndOrganization()
    {
        // Arrange
        var client = _factory.CreateClient();
        var (userId, token) = await RegisterAndLoginAsync(client, "Joel", "Pro");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new CreateOrganizationRequest("LedgerFlow Inc");

        // Act
        var response = await client.PostAsJsonAsync("/api/organizations", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var org = await response.Content.ReadFromJsonAsync<OrganizationResponse>();
        Assert.NotNull(org);
        Assert.NotEqual(Guid.Empty, org.Id);
        Assert.Equal("LedgerFlow Inc", org.Name);
        Assert.Single(org.Members);
        Assert.Equal(userId, org.Members[0].UserId);
        Assert.Equal(OrganizationRole.Owner.ToString(), org.Members[0].Role);
    }

    [Fact]
    public async Task Create_WithoutAuth_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateOrganizationRequest("Unauthorized Org");

        // Act
        var response = await client.PostAsJsonAsync("/api/organizations", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserOrganizations_ReturnsOkAndListOfOrganizations()
    {
        // Arrange
        var client = _factory.CreateClient();
        var (_, token) = await RegisterAndLoginAsync(client, "Joel", "Pro");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await client.PostAsJsonAsync("/api/organizations", new CreateOrganizationRequest("Org One"));
        await client.PostAsJsonAsync("/api/organizations", new CreateOrganizationRequest("Org Two"));

        // Act
        var response = await client.GetAsync("/api/organizations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var orgs = await response.Content.ReadFromJsonAsync<List<OrganizationResponse>>();
        Assert.NotNull(orgs);
        Assert.True(orgs.Count >= 2);
        Assert.Contains(orgs, o => o.Name == "Org One");
        Assert.Contains(orgs, o => o.Name == "Org Two");
    }

    [Fact]
    public async Task GetById_WhenUserIsMember_ReturnsOkAndOrganization()
    {
        // Arrange
        var client = _factory.CreateClient();
        var (_, token) = await RegisterAndLoginAsync(client, "Joel", "Pro");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await client.PostAsJsonAsync("/api/organizations", new CreateOrganizationRequest("Dedicated Org"));
        var createdOrg = await createResponse.Content.ReadFromJsonAsync<OrganizationResponse>();
        Assert.NotNull(createdOrg);

        // Act
        var response = await client.GetAsync($"/api/organizations/{createdOrg.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var org = await response.Content.ReadFromJsonAsync<OrganizationResponse>();
        Assert.NotNull(org);
        Assert.Equal(createdOrg.Id, org.Id);
        Assert.Equal("Dedicated Org", org.Name);
    }

    [Fact]
    public async Task GetById_WhenUserIsNotMember_ReturnsNotFound()
    {
        // Arrange
        var clientA = _factory.CreateClient();
        var (_, tokenA) = await RegisterAndLoginAsync(clientA, "Owner", "User");
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var createResponse = await clientA.PostAsJsonAsync("/api/organizations", new CreateOrganizationRequest("Private Org"));
        var createdOrg = await createResponse.Content.ReadFromJsonAsync<OrganizationResponse>();
        Assert.NotNull(createdOrg);

        var clientB = _factory.CreateClient();
        var (_, tokenB) = await RegisterAndLoginAsync(clientB, "Stranger", "User");
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        // Act
        var response = await clientB.GetAsync($"/api/organizations/{createdOrg.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddMember_WhenActorIsOwner_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var (_, ownerToken) = await RegisterAndLoginAsync(client, "Joel", "Pro");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        var createResponse = await client.PostAsJsonAsync("/api/organizations", new CreateOrganizationRequest("Team Org"));
        var createdOrg = await createResponse.Content.ReadFromJsonAsync<OrganizationResponse>();
        Assert.NotNull(createdOrg);

        var (memberId, _) = await RegisterAndLoginAsync(_factory.CreateClient(), "Member", "Joel");

        var addMemberRequest = new AddOrganizationMemberRequest(memberId, OrganizationRole.Accountant);

        // Act
        var response = await client.PostAsJsonAsync($"/api/organizations/{createdOrg.Id}/members", addMemberRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getResponse = await client.GetAsync($"/api/organizations/{createdOrg.Id}");
        var org = await getResponse.Content.ReadFromJsonAsync<OrganizationResponse>();
        Assert.NotNull(org);
        Assert.Equal(2, org.Members.Count);
        Assert.Contains(org.Members, m => m.UserId == memberId && m.Role == OrganizationRole.Accountant.ToString());
    }

    [Fact]
    public async Task AddMember_WhenActorIsNotOwner_ReturnsForbidden()
    {
        // Arrange
        var ownerClient = _factory.CreateClient();
        var (_, ownerToken) = await RegisterAndLoginAsync(ownerClient, "Joel", "Pro");
        ownerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);

        var createResponse = await ownerClient.PostAsJsonAsync("/api/organizations", new CreateOrganizationRequest("Restricted Org"));
        var createdOrg = await createResponse.Content.ReadFromJsonAsync<OrganizationResponse>();
        Assert.NotNull(createdOrg);

        // Add user as regular Member
        var (regularMemberId, regularMemberToken) = await RegisterAndLoginAsync(_factory.CreateClient(), "Regular", "Member");
        var addFirstMember = new AddOrganizationMemberRequest(regularMemberId, OrganizationRole.Member);
        var addFirstResponse = await ownerClient.PostAsJsonAsync($"/api/organizations/{createdOrg.Id}/members", addFirstMember);
        Assert.Equal(HttpStatusCode.OK, addFirstResponse.StatusCode);

        // Third user to be invited
        var (thirdUserId, _) = await RegisterAndLoginAsync(_factory.CreateClient(), "Third", "Person");

        // Act - Regular member attempts to invite third user
        var memberClient = _factory.CreateClient();
        memberClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", regularMemberToken);

        var unauthorizedAddRequest = new AddOrganizationMemberRequest(thirdUserId, OrganizationRole.Member);
        var response = await memberClient.PostAsJsonAsync($"/api/organizations/{createdOrg.Id}/members", unauthorizedAddRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<(Guid UserId, string Token)> RegisterAndLoginAsync(
        HttpClient client,
        string firstName,
        string lastName)
    {
        string email = $"user_{Guid.NewGuid():N}@ledgerflow.com";
        string password = "P@ssword123!";

        var registerCommand = new RegisterUserCommand(email, password, firstName, lastName);
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerCommand);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var registeredUser = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();
        Assert.NotNull(registeredUser);

        var loginCommand = new LoginUserCommand(email, password);
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginCommand);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginUserResponse>();
        Assert.NotNull(loginResult);
        Assert.False(string.IsNullOrWhiteSpace(loginResult.Token));

        return (registeredUser.Id, loginResult.Token);
    }
}

