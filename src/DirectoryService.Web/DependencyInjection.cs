using DirectoryService.Application;
using DirectoryService.Infrastructure.Postgres;

namespace DirectoryService.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(this IServiceCollection services)
        => services
            .AddWebDependencies()
            .AddInfrastructureDependencies()
            .AddApplicationDependencies();

    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddControllers();

        services.AddLogging();

        return services;
    }
}