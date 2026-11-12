using LedgerFlow.Domain.Common;
using MediatR;

namespace LedgerFlow.Application.Features.Organizations.Commands.CreateOrganization;

public sealed record CreateOrganizationCommand(
    string Name,
    Guid OwnerUserId) : IRequest<Result<OrganizationResponse>>;

