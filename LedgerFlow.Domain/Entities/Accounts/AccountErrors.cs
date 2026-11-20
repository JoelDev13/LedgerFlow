using LedgerFlow.Domain.Common;

namespace LedgerFlow.Domain.Entities.Accounts;

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

    public static readonly Error DuplicateCode = new(
        "Account.DuplicateCode",
        "An account with this code already exists in the organization");

    public static readonly Error AccountNotFound = new(
        "Account.NotFound",
        "The requested account was not found");
}
