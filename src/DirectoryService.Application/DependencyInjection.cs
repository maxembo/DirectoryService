using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Abstractions;

namespace DirectoryService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
        => services
            .AddCommands(typeof(DependencyInjection).Assembly)
            .AddQueries(typeof(DependencyInjection).Assembly)
            .AddValidators();

    private static IServiceCollection AddValidators(this IServiceCollection services)
        => services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
}