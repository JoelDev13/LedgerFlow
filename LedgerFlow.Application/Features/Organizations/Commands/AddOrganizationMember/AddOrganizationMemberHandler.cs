using LedgerFlow.Domain.Repositories;
using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Entities.Organizations;
using MediatR;

namespace LedgerFlow.Application.Features.Organizations.Commands.AddOrganizationMember;

public sealed class AddOrganizationMemberHandler : IRequestHandler<AddOrganizationMemberCommand, Result>
{
    private readonly IOrganizationRepository _organizationRepository;

    public AddOrganizationMemberHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<Result> Handle(
        AddOrganizationMemberCommand command,
        CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(command.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result.Failure(OrganizationErrors.MemberNotFound);
        }

        var addMemberResult = organization.AddMember(
            command.ActorUserId,
            command.NewUserId,
            command.Role);

        if (addMemberResult.IsFailure)
        {
            return addMemberResult;
        }

        await _organizationRepository.UpdateAsync(organization, cancellationToken);

        return Result.Success();
    }
}

