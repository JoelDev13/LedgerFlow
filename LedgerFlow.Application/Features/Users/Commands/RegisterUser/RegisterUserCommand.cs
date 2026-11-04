using LedgerFlow.Domain.Common;
using MediatR;

namespace LedgerFlow.Application.Features.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<Result<RegisterUserResponse>>;

public sealed record RegisterUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName);

