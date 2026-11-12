using LedgerFlow.Domain.Repositories;
using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Entities.Organizations;
using MediatR;

namespace LedgerFlow.Application.Features.Organizations.Queries.GetOrganizationById;

public sealed class GetOrganizationByIdHandler : IRequestHandler<GetOrganizationByIdQuery, Result<OrganizationResponse>>
{
    private readonly IOrganizationRepository _organizationRepository;

    public GetOrganizationByIdHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<Result<OrganizationResponse>> Handle(
        GetOrganizationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(query.OrganizationId, cancellationToken);

        if (organization is null)
        {
            return Result.Failure<OrganizationResponse>(OrganizationErrors.MemberNotFound);
        }

        bool isMember = organization.Members.Any(m => m.UserId == query.UserId);
        if (!isMember)
        {
            return Result.Failure<OrganizationResponse>(OrganizationErrors.MemberNotFound);
        }

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

