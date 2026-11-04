using LedgerFlow.Application.Interfaces.Authentication;
using LedgerFlow.Application.Interfaces.Repositories;
using LedgerFlow.Infrastructure.Authentication;
using LedgerFlow.Infrastructure.Persistence;
using LedgerFlow.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerFlow.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                options.UseNpgsql(connectionString);
            }
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
