namespace LedgerFlow.Domain.Entities;

using LedgerFlow.Domain.Common;

public static class UserErrors
{
    public static readonly Error EmailRequired = new(
        "User.EmailRequired",
        "Email is required");

    public static readonly Error InvalidEmail = new(
        "User.InvalidEmail",
        "Email is not valid");

    public static readonly Error PasswordRequired = new(
        "User.PasswordRequired",
        "Password hash is required");

    public static readonly Error FirstNameRequired = new(
        "User.FirstNameRequired",
        "First name is required");

    public static readonly Error LastNameRequired = new(
        "User.LastNameRequired",
        "Last name is required");

    public static readonly Error NotFound = new(
        "User.NotFound",
        "User was not found");

    public static readonly Error EmailAlreadyInUse = new(
        "User.EmailAlreadyInUse",
        "A user with this email already exists");

    public static readonly Error InvalidCredentials = new(
        "User.InvalidCredentials",
        "Email or password is incorrect");
}

