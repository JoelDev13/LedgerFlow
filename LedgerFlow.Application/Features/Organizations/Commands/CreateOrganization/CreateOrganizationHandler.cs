using LedgerFlow.Domain.Repositories;
using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Entities.Organizations;
using MediatR;

namespace LedgerFlow.Application.Features.Organizations.Commands.CreateOrganization;

public sealed class CreateOrganizationHandler : IRequestHandler<CreateOrganizationCommand, Result<OrganizationResponse>>
{
    private readonly IOrganizationRepository _organizationRepository;

    public CreateOrganizationHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<Result<OrganizationResponse>> Handle(
        CreateOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        var creationResult = Organization.Create(command.Name, command.OwnerUserId);

        if (creationResult.IsFailure)
        {
            return Result.Failure<OrganizationResponse>(creationResult.Error);
        }

        var organization = creationResult.Value;

        await _organizationRepository.AddAsync(organization, cancellationToken);

        var memberResponses = organization.Members
            .Select(m => new OrganizationMemberResponse(m.Id, m.UserId, m.Role.ToString()))
            .ToList();

        var response = new OrganizationResponse(
            organization.Id,
            organization.Name,
            memberResponses);

        return Result.Success(response);
    }
}

