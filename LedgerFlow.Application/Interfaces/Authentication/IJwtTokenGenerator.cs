using LedgerFlow.Domain.Entities.Users;

namespace LedgerFlow.Application.Interfaces.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}

