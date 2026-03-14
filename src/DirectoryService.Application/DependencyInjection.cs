using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedService.Core.Abstractions;
using SharedService.Core.Caching;

namespace DirectoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDependencies(
        this IServiceCollection services, IConfiguration configuration)
        => services
            .AddCommands(typeof(DependencyInjection).Assembly)
            .AddQueries(typeof(DependencyInjection).Assembly)
            .AddRedisCache(configuration)
            .AddValidators();

    private static IServiceCollection AddValidators(this IServiceCollection services)
        => services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
}