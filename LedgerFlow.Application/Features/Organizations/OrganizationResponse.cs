namespace LedgerFlow.Application.Features.Organizations;

public sealed record OrganizationMemberResponse(
    Guid Id,
    Guid UserId,
    string Role);

public sealed record OrganizationResponse(
    Guid Id,
    string Name,
    IReadOnlyList<OrganizationMemberResponse> Members);

