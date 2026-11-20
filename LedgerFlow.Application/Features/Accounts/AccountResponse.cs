namespace LedgerFlow.Application.Features.Accounts;

public sealed record AccountResponse(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string Type,
    bool IsActive);
