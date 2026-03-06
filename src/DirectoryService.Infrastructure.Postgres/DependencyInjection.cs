using System.Reflection;
using Dapper;
using DirectoryService.Application;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Infrastructure.Postgres.Database;
using DirectoryService.Infrastructure.Postgres.Departments;
using DirectoryService.Infrastructure.Postgres.Locations;
using DirectoryService.Infrastructure.Postgres.Positions;
using DirectoryService.Infrastructure.Postgres.SoftDelete;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Dapper;
using Shared.Core.Database;

namespace DirectoryService.Infrastructure.Postgres;

public static class DependencyInjection
{
    private const string DATABASE = "DirectoryServiceDb";
    private const string INACTIVE_DEPARTMENTS_CLEANUP_SECTION = "InactiveDepartmentsCleanup";

    public static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services, IConfiguration configuration)
        => services
            .AddRepositories()
            .AddDatabase(configuration)
            .AddInactiveDepartmentsCleanup(configuration);

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ILocationsRepository, LocationsRepository>();
        services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
        services.AddScoped<IPositionsRepository, PositionsRepository>();

        services.AddScoped<ITransactionManager, TransactionManager>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<DirectoryServiceDbContext>(
            _ => new DirectoryServiceDbContext(configuration.GetConnectionString(DATABASE)!));

        services.AddScoped<IReadDbContext, DirectoryServiceDbContext>(
            _ => new DirectoryServiceDbContext(configuration.GetConnectionString(DATABASE)!));

        services.AddDapper(configuration);

        return services;
    }

    private static IServiceCollection AddDapper(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDbConnectionFactory, DirectoryServiceDbContext>(
            _ => new DirectoryServiceDbContext(configuration.GetConnectionString("DirectoryServiceDb")!));

        DefaultTypeMap.MatchNamesWithUnderscores = true;

        var assembly = Assembly.GetAssembly(typeof(Contracts.DependencyInjection));

        if (assembly != null)
        {
            var jsonTypes = assembly.GetTypes()
                .Where(t => t.IsClass && typeof(IDapperJson).IsAssignableFrom(t))
                .ToList();

            foreach (var type in jsonTypes)
            {
                var handlerType = typeof(JsonTypeHandler<>).MakeGenericType(type);
                object? handler = Activator.CreateInstance(handlerType);
                SqlMapper.AddTypeHandler(
                    type, handler as SqlMapper.ITypeHandler ?? throw new InvalidOperationException());
            }
        }

        return services;
    }

    private static IServiceCollection AddInactiveDepartmentsCleanup(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IDeleteDepartmentsService, DeleteDepartmentsService>();

        services.AddHostedService<CleaningInactiveDepartmentsBackgroundService>();

        services.Configure<InactiveDepartmentsCleanupOptions>(
            configuration.GetSection(INACTIVE_DEPARTMENTS_CLEANUP_SECTION));

        return services;
    }
}