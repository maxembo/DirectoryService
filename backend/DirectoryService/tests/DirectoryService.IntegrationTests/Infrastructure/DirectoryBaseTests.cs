using DirectoryService.Application.Constants;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Infrastructure;

public abstract class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    private readonly Func<Task> _resetDatabaseAsync;
    private readonly IServiceProvider _services;

    protected DirectoryBaseTests(DirectoryTestWebFactory factory)
    {
        _services = factory.Services;
        _resetDatabaseAsync = factory.ResetDatabaseAsync;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _resetDatabaseAsync();

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
        return await ExecuteInDb(async dbContext =>
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
        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .SingleAsync(l => l.Id == locationId, CancellationToken.None);

            location.MarkAsDelete();

            await dbContext.SaveChangesAsync();
        });
    }

    protected async Task<Department> CreateParentDepartment(
        string name, string identifier, IEnumerable<LocationId> locationIds)
    {
        var departmentId = DepartmentId.CreateNew();

        var departmentLocations = locationIds.Select(locationId =>
            new DepartmentLocation(DepartmentLocationId.CreateNew(), departmentId, locationId));

        var parent = Department.CreateParent(
                DepartmentName.Create(name).Value, Identifier.Create(identifier).Value, departmentLocations,
                departmentId)
            .Value;

        await ExecuteInDb(async dbContext =>
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

        var departmentLocations = locationIds.Select(locationId =>
            new DepartmentLocation(DepartmentLocationId.CreateNew(), departmentId, locationId));

        var child = Department.CreateChild(
                DepartmentName.Create(name).Value, Identifier.Create(identifier).Value, parent, departmentLocations,
                departmentId)
            .Value;

        await ExecuteInDb(async dbContext =>
        {
            dbContext.Departments.Add(child);
            await dbContext.SaveChangesAsync();
        });

        return child;
    }

    protected async Task MarkDepartmentAsDeleted(DepartmentId departmentId)
    {
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.SingleAsync(d => d.Id == departmentId);

            department.MarkAsDelete();

            await dbContext.SaveChangesAsync();
        });
    }

    protected async Task<PositionId> CreatePosition(
        string? name, string? description, IEnumerable<DepartmentId> departmentIds)
    {
        var positionId = PositionId.CreateNew();

        var departmentPositions = departmentIds.Select(departmentId =>
            new DepartmentPosition(DepartmentPositionId.CreateNew(), departmentId, positionId));

        var position = new Position(
            positionId, PositionName.Create(name!).Value,
            Description.Create(description).Value, departmentPositions);

        await ExecuteInDb(async dbContext =>
        {
            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync();
        });

        return position.Id;
    }

    protected async Task MarkPositionAsDeleted(PositionId positionId)
    {
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .SingleAsync(p => p.Id == positionId);

            position.MarkAsDelete();

            await dbContext.SaveChangesAsync();
        });
    }

    protected async Task MarkDepartmentAsDeletedAt(DepartmentId departmentId, DateTime deletedAt)
    {
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.SingleAsync(d => d.Id == departmentId);

            department.MarkAsDelete();

            dbContext.Entry(department)
                .Property(d => d.DeletedAt)
                .CurrentValue = deletedAt;

            await dbContext.SaveChangesAsync();
        });
    }

    protected async Task MarkLocationAsDeletedAt(LocationId departmentId, DateTime deletedAt)
    {
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Locations.SingleAsync(d => d.Id == departmentId);

            department.MarkAsDelete();

            dbContext.Entry(department)
                .Property(d => d.DeletedAt)
                .CurrentValue = deletedAt;

            await dbContext.SaveChangesAsync();
        });
    }

    protected async Task MarkPositionAsDeletedAt(PositionId positionId, DateTime deletedAt)
    {
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions.SingleAsync(p => p.Id == positionId);

            position.MarkAsDelete();

            dbContext.Entry(position)
                .Property(p => p.DeletedAt)
                .CurrentValue = deletedAt;

            await dbContext.SaveChangesAsync();
        });
    }

    protected async Task ClearDepartmentCache()
    {
        await using var scope = _services.CreateAsyncScope();

        var cache = scope.ServiceProvider.GetRequiredService<HybridCache>();

        await cache.RemoveByTagAsync(CacheKeys.DEPARTMENT_KEY, CancellationToken.None);
    }
}