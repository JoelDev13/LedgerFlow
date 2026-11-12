using LedgerFlow.Application.Interfaces.Authentication;
using LedgerFlow.Domain.Repositories;
using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Entities.Users;
using MediatR;

namespace LedgerFlow.Application.Features.Users.Commands.LoginUser;

public sealed class LoginUserHandler : IRequestHandler<LoginUserCommand, Result<LoginUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<LoginUserResponse>> Handle(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
        {
            return Result.Failure<LoginUserResponse>(UserErrors.InvalidCredentials);
        }

        User? user = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null)
        {
            return Result.Failure<LoginUserResponse>(UserErrors.InvalidCredentials);
        }

        bool isPasswordValid = _passwordHasher.Verify(command.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return Result.Failure<LoginUserResponse>(UserErrors.InvalidCredentials);
        }

        string token = _jwtTokenGenerator.GenerateToken(user);

        return Result.Success(new LoginUserResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            token));
    }
}

