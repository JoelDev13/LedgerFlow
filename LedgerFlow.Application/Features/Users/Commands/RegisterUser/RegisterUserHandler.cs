using LedgerFlow.Application.Interfaces.Authentication;
using LedgerFlow.Application.Interfaces.Repositories;
using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Entities.Users;
using MediatR;

namespace LedgerFlow.Application.Features.Users.Commands.RegisterUser;

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Password))
        {
            return Result.Failure<RegisterUserResponse>(UserErrors.PasswordRequired);
        }

        bool emailExists = await _userRepository.ExistsByEmailAsync(command.Email, cancellationToken);
        if (emailExists)
        {
            return Result.Failure<RegisterUserResponse>(UserErrors.EmailAlreadyInUse);
        }

        string passwordHash = _passwordHasher.Hash(command.Password);

        var userResult = User.Create(command.Email, passwordHash, command.FirstName, command.LastName);
        if (userResult.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(userResult.Error);
        }

        User user = userResult.Value;
        await _userRepository.AddAsync(user, cancellationToken);

        return Result.Success(new RegisterUserResponse(user.Id, user.Email, user.FirstName, user.LastName));
    }
}
