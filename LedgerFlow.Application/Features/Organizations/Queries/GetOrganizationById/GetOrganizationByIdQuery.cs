using LedgerFlow.Domain.Common;
using MediatR;

namespace LedgerFlow.Application.Features.Organizations.Queries.GetOrganizationById;

public sealed record GetOrganizationByIdQuery(
    Guid OrganizationId,
    Guid UserId) : IRequest<Result<OrganizationResponse>>;

