using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Abstractions;

namespace DirectoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        => services
            .AddCommands(typeof(DependencyInjection).Assembly)
            .AddQueries(typeof(DependencyInjection).Assembly)
            .AddRedisCache()
            .AddValidators();

    private static IServiceCollection AddValidators(this IServiceCollection services)
        => services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

    private static IServiceCollection AddRedisCache(this IServiceCollection services)
    {
        services.AddStackExchangeRedisCache(options => { options.Configuration = "localhost:6379"; });

        services.AddHybridCache(
            options =>
            {
                options.DefaultEntryOptions = new HybridCacheEntryOptions()
                {
                    LocalCacheExpiration = TimeSpan.FromMinutes(5), Expiration = TimeSpan.FromMinutes(5),
                };
            });

        return services;
    }
}