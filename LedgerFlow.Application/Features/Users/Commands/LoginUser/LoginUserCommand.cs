using LedgerFlow.Domain.Common;
using MediatR;

namespace LedgerFlow.Application.Features.Users.Commands.LoginUser;

public sealed record LoginUserCommand(string Email, string Password) : IRequest<Result<LoginUserResponse>>;

public sealed record LoginUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Token);

