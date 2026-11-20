using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Entities.Accounts;
using LedgerFlow.Domain.Repositories;
using MediatR;

namespace LedgerFlow.Application.Features.Accounts.Commands.CreateAccount;

public sealed class CreateAccountHandler : IRequestHandler<CreateAccountCommand, Result<AccountResponse>>
{
    private readonly IAccountRepository _accountRepository;

    public CreateAccountHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Result<AccountResponse>> Handle(
        CreateAccountCommand command,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AccountType>(command.Type, ignoreCase: true, out var accountType))
        {
            return Result.Failure<AccountResponse>(
                new Error("Account.InvalidType", $"'{command.Type}' is not a valid account type"));
        }

        bool codeExists = await _accountRepository.ExistsByCodeAsync(
            command.OrganizationId, command.Code, cancellationToken);

        if (codeExists)
        {
            return Result.Failure<AccountResponse>(AccountErrors.DuplicateCode);
        }

        var creationResult = Account.Create(
            command.OrganizationId, command.Code, command.Name, accountType);

        if (creationResult.IsFailure)
        {
            return Result.Failure<AccountResponse>(creationResult.Error);
        }

        var account = creationResult.Value;

        await _accountRepository.AddAsync(account, cancellationToken);

        var response = new AccountResponse(
            account.Id,
            account.OrganizationId,
            account.Code,
            account.Name,
            account.Type.ToString(),
            account.IsActive);

        return Result.Success(response);
    }
}
