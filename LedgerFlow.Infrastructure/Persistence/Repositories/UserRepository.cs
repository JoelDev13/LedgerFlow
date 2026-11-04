using LedgerFlow.Application.Interfaces.Repositories;
using LedgerFlow.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        string normalized = email.Trim().ToLowerInvariant();
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalized, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        string normalized = email.Trim().ToLowerInvariant();
        return await _context.Users
            .AnyAsync(u => u.Email == normalized, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
