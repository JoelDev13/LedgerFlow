namespace LedgerFlow.Domain.Entities;

using LedgerFlow.Domain.Common;

public sealed class Organization : AggregateRoot<Guid>
{
    private readonly List<OrganizationMember> _members = [];

    public string Name { get; private set; } = default!;

    public IReadOnlyCollection<OrganizationMember> Members => _members.AsReadOnly();

    private Organization()
    {
    }

    private Organization(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public static Result<Organization> Create(string name, Guid ownerUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Organization>(OrganizationErrors.NameRequired);
        }

        if (ownerUserId == Guid.Empty)
        {
            return Result.Failure<Organization>(OrganizationErrors.InvalidUserId);
        }

        var organization = new Organization(Guid.NewGuid(), name.Trim());
        organization._members.Add(new OrganizationMember(ownerUserId, OrganizationRole.Owner));

        return Result.Success(organization);
    }

    public Result AddMember(Guid actorUserId, Guid newUserId, OrganizationRole role)
    {
        if (!IsOwner(actorUserId))
        {
            return Result.Failure(OrganizationErrors.OnlyOwnerCanManageMembers);
        }

        if (newUserId == Guid.Empty)
        {
            return Result.Failure(OrganizationErrors.InvalidUserId);
        }

        if (_members.Any(m => m.UserId == newUserId))
        {
            return Result.Failure(OrganizationErrors.MemberAlreadyExists);
        }

        _members.Add(new OrganizationMember(newUserId, role));
        return Result.Success();
    }

    public Result RemoveMember(Guid actorUserId, Guid targetUserId)
    {
        if (!IsOwner(actorUserId))
        {
            return Result.Failure(OrganizationErrors.OnlyOwnerCanManageMembers);
        }

        var memberToRemove = _members.FirstOrDefault(m => m.UserId == targetUserId);
        if (memberToRemove is null)
        {
            return Result.Failure(OrganizationErrors.MemberNotFound);
        }

        if (memberToRemove.Role == OrganizationRole.Owner && _members.Count(m => m.Role == OrganizationRole.Owner) <= 1)
        {
            return Result.Failure(OrganizationErrors.CannotRemoveLastOwner);
        }

        _members.Remove(memberToRemove);
        return Result.Success();
    }

    private bool IsOwner(Guid userId)
    {
        return _members.Any(m => m.UserId == userId && m.Role == OrganizationRole.Owner);
    }
}
