using LedgerFlow.Domain.Common;

namespace LedgerFlow.Domain.Entities.Organizations;

public sealed class OrganizationMember : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public OrganizationRole Role { get; private set; }

    private OrganizationMember()
    {
    }

    public OrganizationMember(Guid userId, OrganizationRole role)
        : base(Guid.NewGuid())
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        UserId = userId;
        Role = role;
    }

    public void UpdateRole(OrganizationRole newRole)
    {
        Role = newRole;
    }
}
