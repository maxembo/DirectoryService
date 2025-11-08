using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Infrastructure.Postgres.Locations;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Postgres;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
    {
        services.AddScoped<DirectoryServiceDbContext>();

        services.AddScoped<ILocationsRepository, LocationsRepository>();

        return services;
    }
}