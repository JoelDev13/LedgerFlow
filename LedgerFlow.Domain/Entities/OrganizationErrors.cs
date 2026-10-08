namespace LedgerFlow.Domain.Entities;

using LedgerFlow.Domain.Common;

public static class OrganizationErrors
{
    public static readonly Error NameRequired = new(
        "Organization.NameRequired",
        "Organization name is required");

    public static readonly Error InvalidUserId = new(
        "Organization.InvalidUserId",
        "User ID must be valid");

    public static readonly Error OnlyOwnerCanManageMembers = new(
        "Organization.OnlyOwnerCanManageMembers",
        "Only an organization owner can manage members");

    public static readonly Error MemberAlreadyExists = new(
        "Organization.MemberAlreadyExists",
        "User is already a member of this organization");

    public static readonly Error MemberNotFound = new(
        "Organization.MemberNotFound",
        "Member was not found in this organization");

    public static readonly Error CannotRemoveLastOwner = new(
        "Organization.CannotRemoveLastOwner",
        "Cannot remove the last owner of an organization");
}
