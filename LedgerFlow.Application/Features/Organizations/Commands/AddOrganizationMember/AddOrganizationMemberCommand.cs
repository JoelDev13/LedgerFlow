using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Entities.Organizations;
using MediatR;

namespace LedgerFlow.Application.Features.Organizations.Commands.AddOrganizationMember;

public sealed record AddOrganizationMemberCommand(
    Guid OrganizationId,
    Guid ActorUserId,
    Guid NewUserId,
    OrganizationRole Role) : IRequest<Result>;

