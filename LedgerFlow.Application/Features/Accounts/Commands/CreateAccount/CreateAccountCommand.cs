using LedgerFlow.Domain.Common;
using MediatR;

namespace LedgerFlow.Application.Features.Accounts.Commands.CreateAccount;

public sealed record CreateAccountCommand(
    Guid OrganizationId,
    string Code,
    string Name,
    string Type) : IRequest<Result<AccountResponse>>;
