using LedgerFlow.Domain.Entities.Accounts;

namespace LedgerFlow.Domain.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Account>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(Guid organizationId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Account account, CancellationToken cancellationToken = default);
    Task UpdateAsync(Account account, CancellationToken cancellationToken = default);
}
