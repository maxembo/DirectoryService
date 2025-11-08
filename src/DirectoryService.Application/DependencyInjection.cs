using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        var assembly = typeof(DependencyInjection).Assembly;

        services.Scan(
            scan => scan
                .FromAssemblies(assembly)
                .AddClasses(
                    classes =>
                        classes.AssignableToAny(typeof(ICommandHandler<,>), typeof(ICommandHandler<>)))
                .AsSelfWithInterfaces()
                .WithScopedLifetime());

        return services;
    }
}