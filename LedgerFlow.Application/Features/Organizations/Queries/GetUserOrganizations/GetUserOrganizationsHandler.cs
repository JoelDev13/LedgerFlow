using LedgerFlow.Domain.Repositories;
using LedgerFlow.Domain.Common;
using MediatR;

namespace LedgerFlow.Application.Features.Organizations.Queries.GetUserOrganizations;

public sealed class GetUserOrganizationsHandler : IRequestHandler<GetUserOrganizationsQuery, Result<IReadOnlyList<OrganizationResponse>>>
{
    private readonly IOrganizationRepository _organizationRepository;

    public GetUserOrganizationsHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<Result<IReadOnlyList<OrganizationResponse>>> Handle(
        GetUserOrganizationsQuery query,
        CancellationToken cancellationToken)
    {
        var organizations = await _organizationRepository.GetByUserIdAsync(query.UserId, cancellationToken);

        var responses = organizations
            .Select(o => new OrganizationResponse(
                o.Id,
                o.Name,
                o.Members.Select(m => new OrganizationMemberResponse(m.Id, m.UserId, m.Role.ToString())).ToList()))
            .ToList();

        return Result.Success<IReadOnlyList<OrganizationResponse>>(responses);
    }
}

