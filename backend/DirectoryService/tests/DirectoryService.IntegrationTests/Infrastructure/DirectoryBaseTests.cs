using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    private readonly IServiceProvider _services;

    private readonly Func<Task> _resetDatabaseAsync;

    protected DirectoryBaseTests(DirectoryTestWebFactory factory)
    {
        _services = factory.Services;
        _resetDatabaseAsync = factory.ResetDatabaseAsync;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _resetDatabaseAsync();
    }

    protected async Task<T> ExecuteInDb<T>(Func<DirectoryServiceDbContext, Task<T>> action)
    {
        await using var scope = _services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        return await action(dbContext);
    }

    protected async Task ExecuteInDb(Func<DirectoryServiceDbContext, Task> action)
    {
        await using var scope = _services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        await action(dbContext);
    }

    protected async Task<TResult> Execute<TResult, TService>(Func<TService, Task<TResult>> action)
        where TService : notnull
    {
        await using var scope = _services.CreateAsyncScope();

        var handler = scope.ServiceProvider.GetRequiredService<TService>();

        return await action(handler);
    }

    protected async Task<LocationId> CreateLocation(
        string name = "Test Name",
        string city = "Test City",
        string country = "Test Country",
        string street = "Test Street",
        string house = "10 test house",
        string timezone = "Europe/Moscow")
    {
        return await ExecuteInDb(
            async dbContext =>
            {
                var location = new Location(
                    LocationId.CreateNew(), LocationName.Create(name).Value,
                    Timezone.Create(timezone).Value,
                    Address.Create(city, country, street, house).Value);

                dbContext.Locations.Add(location);
                await dbContext.SaveChangesAsync();

                return location.Id;
            });
    }

    protected async Task MarkLocationAsDeleted(LocationId locationId)
    {
        await ExecuteInDb(
            async dbContext =>
            {
                var location = await dbContext.Locations
                    .SingleAsync(l => l.Id == locationId, CancellationToken.None);

                location.MarkAsDelete();

                await dbContext.SaveChangesAsync();

                return location.Id;
            });
    }

    protected async Task<Department> CreateParentDepartment(
        string name, string identifier, IEnumerable<LocationId> locationIds)
    {
        var departmentId = DepartmentId.CreateNew();

        var departmentLocations = locationIds.Select(
            locationId => new DepartmentLocation(DepartmentLocationId.CreateNew(), departmentId, locationId));

        var parent = Department.CreateParent(
                DepartmentName.Create(name).Value, Identifier.Create(identifier).Value, departmentLocations,
                departmentId)
            .Value;

        await ExecuteInDb(
            async dbContext =>
            {
                dbContext.Departments.Add(parent);
                await dbContext.SaveChangesAsync();
            });

        return parent;
    }

    protected async Task<Department> CreateChildDepartment(
        string name, string identifier, Department parent, IEnumerable<LocationId> locationIds)
    {
        var departmentId = DepartmentId.CreateNew();

        var departmentLocations = locationIds.Select(
            locationId => new DepartmentLocation(DepartmentLocationId.CreateNew(), departmentId, locationId));

        var child = Department.CreateChild(
                DepartmentName.Create(name).Value, Identifier.Create(identifier).Value, parent, departmentLocations,
                departmentId)
            .Value;

        await ExecuteInDb(
            async dbContext =>
            {
                dbContext.Departments.Add(child);
                await dbContext.SaveChangesAsync();
            });

        return child;
    }
}