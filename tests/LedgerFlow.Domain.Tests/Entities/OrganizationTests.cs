namespace LedgerFlow.Domain.Tests.Entities;

using LedgerFlow.Domain.Entities;

public class OrganizationTests
{
    // Verifies that Create fails when an empty or whitespace name is provided
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WhenNameIsNullOrEmpty_ShouldReturnFailure(string? name)
    {
        var ownerId = Guid.NewGuid();

        var result = Organization.Create(name!, ownerId);

        Assert.True(result.IsFailure);
        Assert.Equal("Organization.NameRequired", result.Error.Code);
    }

    // Verifies that Create successfully instantiates Organization with the initial owner member
    [Fact]
    public void Create_WithValidParameters_ShouldCreateOrganizationWithInitialOwner()
    {
        var ownerId = Guid.NewGuid();

        var result = Organization.Create("Acme Corp", ownerId);

        Assert.True(result.IsSuccess);
        Assert.Equal("Acme Corp", result.Value.Name);
        Assert.Single(result.Value.Members);
        var initialMember = result.Value.Members.First();
        Assert.Equal(ownerId, initialMember.UserId);
        Assert.Equal(OrganizationRole.Owner, initialMember.Role);
    }

    // Verifies that an Owner can add a new member to the organization
    [Fact]
    public void AddMember_WhenActorIsOwner_ShouldAddMemberSuccessfully()
    {
        var ownerId = Guid.NewGuid();
        var newMemberId = Guid.NewGuid();
        var org = Organization.Create("Acme Corp", ownerId).Value;

        var result = org.AddMember(ownerId, newMemberId, OrganizationRole.Accountant);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, org.Members.Count);
        Assert.Contains(org.Members, m => m.UserId == newMemberId && m.Role == OrganizationRole.Accountant);
    }

    // Verifies that non-owner members cannot add members to the organization
    [Fact]
    public void AddMember_WhenActorIsNotOwner_ShouldReturnFailure()
    {
        var ownerId = Guid.NewGuid();
        var accountantId = Guid.NewGuid();
        var thirdUserId = Guid.NewGuid();
        var org = Organization.Create("Acme Corp", ownerId).Value;
        org.AddMember(ownerId, accountantId, OrganizationRole.Accountant);

        var result = org.AddMember(accountantId, thirdUserId, OrganizationRole.Member);

        Assert.True(result.IsFailure);
        Assert.Equal("Organization.OnlyOwnerCanManageMembers", result.Error.Code);
    }

    // Verifies that adding an existing member returns a MemberAlreadyExists error
    [Fact]
    public void AddMember_WhenMemberAlreadyExists_ShouldReturnFailure()
    {
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var org = Organization.Create("Acme Corp", ownerId).Value;
        org.AddMember(ownerId, memberId, OrganizationRole.Member);

        var result = org.AddMember(ownerId, memberId, OrganizationRole.Accountant);

        Assert.True(result.IsFailure);
        Assert.Equal("Organization.MemberAlreadyExists", result.Error.Code);
    }

    // Verifies that an Owner can remove a member from the organization
    [Fact]
    public void RemoveMember_WhenActorIsOwnerAndTargetExists_ShouldRemoveMemberSuccessfully()
    {
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var org = Organization.Create("Acme Corp", ownerId).Value;
        org.AddMember(ownerId, memberId, OrganizationRole.Member);

        var result = org.RemoveMember(ownerId, memberId);

        Assert.True(result.IsSuccess);
        Assert.Single(org.Members);
        Assert.DoesNotContain(org.Members, m => m.UserId == memberId);
    }

    // Verifies that non-owner members cannot remove members
    [Fact]
    public void RemoveMember_WhenActorIsNotOwner_ShouldReturnFailure()
    {
        var ownerId = Guid.NewGuid();
        var accountantId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var org = Organization.Create("Acme Corp", ownerId).Value;
        org.AddMember(ownerId, accountantId, OrganizationRole.Accountant);
        org.AddMember(ownerId, memberId, OrganizationRole.Member);

        var result = org.RemoveMember(accountantId, memberId);

        Assert.True(result.IsFailure);
        Assert.Equal("Organization.OnlyOwnerCanManageMembers", result.Error.Code);
    }

    // Verifies that removing a non-existent member returns MemberNotFound error
    [Fact]
    public void RemoveMember_WhenTargetDoesNotExist_ShouldReturnFailure()
    {
        var ownerId = Guid.NewGuid();
        var org = Organization.Create("Acme Corp", ownerId).Value;

        var result = org.RemoveMember(ownerId, Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal("Organization.MemberNotFound", result.Error.Code);
    }

    // Verifies that attempting to remove the last owner of an organization returns CannotRemoveLastOwner error
    [Fact]
    public void RemoveMember_WhenRemovingLastOwner_ShouldReturnFailure()
    {
        var ownerId = Guid.NewGuid();
        var org = Organization.Create("Acme Corp", ownerId).Value;

        var result = org.RemoveMember(ownerId, ownerId);

        Assert.True(result.IsFailure);
        Assert.Equal("Organization.CannotRemoveLastOwner", result.Error.Code);
    }

    // Verifies that an owner can be removed if another owner exists
    [Fact]
    public void RemoveMember_WhenMultipleOwnersExist_ShouldAllowRemovingAnOwner()
    {
        var owner1Id = Guid.NewGuid();
        var owner2Id = Guid.NewGuid();
        var org = Organization.Create("Acme Corp", owner1Id).Value;
        org.AddMember(owner1Id, owner2Id, OrganizationRole.Owner);

        var result = org.RemoveMember(owner1Id, owner2Id);

        Assert.True(result.IsSuccess);
        Assert.Single(org.Members);
        Assert.Equal(owner1Id, org.Members.First().UserId);
    }
}
