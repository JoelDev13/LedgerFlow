using LedgerFlow.Domain.Entities.Accounts;
using LedgerFlow.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Persistence.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Account>> GetByOrganizationIdAsync(
        Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.OrganizationId == organizationId)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(
        Guid organizationId, string code, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AnyAsync(a => a.OrganizationId == organizationId && a.Code == code, cancellationToken);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
