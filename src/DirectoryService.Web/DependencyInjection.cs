using DirectoryService.Application;
using DirectoryService.Infrastructure.Postgres;
using Serilog;
using Serilog.Exceptions;

namespace DirectoryService.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddProgramDependencies(
        this IServiceCollection services, IConfiguration configuration)
        => services
            .AddSerilogLogging(configuration)
            .AddWebDependencies()
            .AddInfrastructureDependencies()
            .AddApplicationDependencies();

    private static IServiceCollection AddWebDependencies(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddControllers();

        return services;
    }

    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog(
            (sp, ls) => ls
                .ReadFrom.Configuration(configuration)
                .ReadFrom.Services(sp)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithProperty("ServiceName", "LocationsService"));

        return services;
    }
}