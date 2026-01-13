using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Infrastructure.Postgres.Database;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Infrastructure;

public class DirectoryBaseTests : IClassFixture<DirectoryTestWebFactory>, IAsyncLifetime
{
    protected IServiceProvider Services { get; set; }

    private readonly Func<Task> _resetDatabase;

    protected DirectoryBaseTests(DirectoryTestWebFactory factory)
    {
        Services = factory.Services;
        _resetDatabase = factory.ResetDatabaseAsync;
    }

    protected async Task<T> ExecuteInDb<T>(Func<DirectoryServiceDbContext, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        return await action(dbContext);
    }

    protected async Task ExecuteInDb(Func<DirectoryServiceDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DirectoryServiceDbContext>();

        await action(dbContext);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _resetDatabase();
    }

    protected async Task<LocationId> CreateLocation(string name)
    {
        return await ExecuteInDb(
            async dbContext =>
            {
                var location = new Location(
                    LocationId.CreateNew(), LocationName.Create(name).Value,
                    Timezone.Create("Europe/Moscow").Value,
                    Address.Create("Воронеж", "Россия", "ул. Проспект Революции", "5").Value);

                dbContext.Locations.Add(location);

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