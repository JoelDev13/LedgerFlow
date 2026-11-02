namespace LedgerFlow.Domain.Entities;

using LedgerFlow.Domain.Common;

public sealed class Account : AggregateRoot<Guid>
{
    public Guid OrganizationId { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public AccountType Type { get; private set; }
    public bool IsActive { get; private set; }

    private Account()
    {
    }

    private Account(Guid id, Guid organizationId, string code, string name, AccountType type)
        : base(id)
    {
        OrganizationId = organizationId;
        Code = code;
        Name = name;
        Type = type;
        IsActive = true;
    }

    public static Result<Account> Create(Guid organizationId, string code, string name, AccountType type)
    {
        if (organizationId == Guid.Empty)
        {
            return Result.Failure<Account>(AccountErrors.InvalidOrganizationId);
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<Account>(AccountErrors.CodeRequired);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Account>(AccountErrors.NameRequired);
        }

        var account = new Account(Guid.NewGuid(), organizationId, code.Trim(), name.Trim(), type);
        return Result.Success(account);
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
