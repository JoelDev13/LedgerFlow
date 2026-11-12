using LedgerFlow.Domain.Common;
using MediatR;

namespace LedgerFlow.Application.Features.Organizations.Queries.GetUserOrganizations;

public sealed record GetUserOrganizationsQuery(Guid UserId) : IRequest<Result<IReadOnlyList<OrganizationResponse>>>;

