using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure.Postgres.Database;
using DirectoryService.Infrastructure.Postgres.Departments;
using DirectoryService.Infrastructure.Postgres.Locations;
using DirectoryService.Infrastructure.Postgres.Positions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Postgres;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<DirectoryServiceDbContext>(
            _ => new DirectoryServiceDbContext(configuration.GetConnectionString("DirectoryServiceDb")!));

        services.AddScoped<IReadDbContext, DirectoryServiceDbContext>(
            _ => new DirectoryServiceDbContext(configuration.GetConnectionString("DirectoryServiceDb")!));

        services.AddScoped<ILocationsRepository, LocationsRepository>();
        services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
        services.AddScoped<IPositionsRepository, PositionsRepository>();

        services.AddScoped<ITransactionManager, TransactionManager>();

        return services;
    }
}