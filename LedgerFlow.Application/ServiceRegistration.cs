using LedgerFlow.Application.Features.Users.Commands.RegisterUser;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerFlow.Application;


public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServiceRegistration).Assembly));

        return services;
    }
}

