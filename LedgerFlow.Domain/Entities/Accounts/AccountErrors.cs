namespace LedgerFlow.Domain.Entities;

using LedgerFlow.Domain.Common;

public static class AccountErrors
{
    public static readonly Error CodeRequired = new(
        "Account.CodeRequired",
        "Account code is required");

    public static readonly Error NameRequired = new(
        "Account.NameRequired",
        "Account name is required");

    public static readonly Error InvalidOrganizationId = new(
        "Account.InvalidOrganizationId",
        "Organization ID is invalid");
}
