using CSharpFunctionalExtensions;
using DirectoryService.Application.Cleanup;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Cleanup;

public class DeletedEntitiesCleanupServiceTests(DirectoryTestWebFactory factory)
    : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task Process_WhenTwoExpiredDepartmentsHaveActiveChild_ShouldReparentChildAndUpdatePath()
    {
        // arrange
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var development =
            await CreateChildDepartment("development", "development", company, [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", development, [locationId]);

        var team = await CreateChildDepartment("team", "team", backend, [locationId]);

        await MarkDepartmentAsDeletedAt(development.Id, DateTime.UtcNow.AddMonths(-2));
        await MarkDepartmentAsDeletedAt(backend.Id, DateTime.UtcNow.AddMonths(-1));

        // act
        var result = await ExecuteCleanup();

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var updatedTeam = await dbContext.Departments
                .SingleAsync(d => d.Id == team.Id);

            Assert.True(await dbContext.Departments.AnyAsync(d => d.Id == company.Id));

            Assert.False(await dbContext.Departments.AnyAsync(d => d.Id == development.Id));

            Assert.False(await dbContext.Departments.AnyAsync(d => d.Id == backend.Id));

            Assert.True(await dbContext.Departments.AnyAsync(d => d.Id == team.Id));

            Assert.Equal(company.Id, updatedTeam.ParentId);

            Assert.Equal("company.team", updatedTeam.Path.Value);

            Assert.Equal(1, updatedTeam.Path.Depth);
        });
    }

    [Fact]
    public async Task Process_WhenExpiredDepartmentHasActiveChild_ShouldReparentChildAndUpdatePath()
    {
        // arrange
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var development =
            await CreateChildDepartment("development", "development", company, [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", development, [locationId]);

        await MarkDepartmentAsDeletedAt(development.Id, DateTime.UtcNow.AddMonths(-2));

        // act
        var result = await ExecuteCleanup();

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var updateBackend = await dbContext.Departments
                .SingleAsync(d => d.Id == backend.Id);

            Assert.True(await dbContext.Departments.AnyAsync(d => d.Id == company.Id));

            Assert.False(await dbContext.Departments.AnyAsync(d => d.Id == development.Id));

            Assert.True(await dbContext.Departments.AnyAsync(d => d.Id == backend.Id));

            Assert.Equal(company.Id, updateBackend.ParentId);
            Assert.Equal("company.backend", updateBackend.Path.Value);
            Assert.Equal(1, updateBackend.Path.Depth);
        });
    }

    [Fact]
    public async Task Process_WithExpiredAndRecentPositions_ShouldDeleteOnlyExpiredPosition()
    {
        // arrange
        var locationId = await CreateLocation();

        var department =
            await CreateParentDepartment("department", "department", [locationId]);

        var expiredPositionId =
            await CreatePosition("developer", "description", [department.Id]);

        var recentPositionId = await CreatePosition("recent", "recent", [department.Id]);

        await MarkPositionAsDeletedAt(expiredPositionId, DateTime.UtcNow.AddDays(-31));
        await MarkPositionAsDeletedAt(recentPositionId, DateTime.UtcNow.AddDays(-14));

        // act
        var result = await ExecuteCleanup();

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            Assert.False(await dbContext.Positions.AnyAsync(p => p.Id == expiredPositionId));
            Assert.False(await dbContext.DepartmentPositions.AnyAsync(dp => dp.PositionId == expiredPositionId));

            Assert.True(await dbContext.Positions.AnyAsync(p => p.Id == recentPositionId));
            Assert.True(await dbContext.DepartmentPositions.AnyAsync(dp => dp.PositionId == recentPositionId));

            Assert.True(await dbContext.Departments.AnyAsync(d => d.Id == department.Id));
        });
    }

    [Fact]
    public async Task Process_WithExpiredAndRecentLocations_ShouldDeleteOnlyExpiredLocation()
    {
        // arrange
        var expiredLocationId = await CreateLocation();
        var recentLocationId = await CreateLocation("expired", country: "expired");

        var department = await CreateParentDepartment(
            "department", "department", [expiredLocationId, recentLocationId]);

        await MarkLocationAsDeletedAt(expiredLocationId, DateTime.UtcNow.AddMonths(-1).AddDays(-1));
        await MarkLocationAsDeletedAt(recentLocationId, DateTime.UtcNow.AddDays(-5));

        // act
        var result = await ExecuteCleanup();

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            Assert.False(await dbContext.Locations.AnyAsync(l => l.Id == expiredLocationId));
            Assert.False(await dbContext.DepartmentLocations.AnyAsync(dl => dl.LocationId == expiredLocationId));

            Assert.True(await dbContext.Locations.AnyAsync(l => l.Id == recentLocationId));
            Assert.True(await dbContext.DepartmentLocations.AnyAsync(dl => dl.LocationId == recentLocationId));

            Assert.True(await dbContext.Departments.AnyAsync(d => d.Id == department.Id));
        });
    }

    [Fact]
    public async Task Process_WhenCalledTwice_ShouldBeIdempotent()
    {
        // arrange
        var locationId = await CreateLocation();

        var department =
            await CreateParentDepartment("first-department", "first-department", [locationId]);

        await MarkDepartmentAsDeletedAt(department.Id, DateTime.UtcNow.AddMonths(-1).AddDays(-1));

        // act
        var firstResult = await ExecuteCleanup();
        var secondResult = await ExecuteCleanup();

        // assert
        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            Assert.False(await dbContext.Departments.AnyAsync(d => d.Id == department.Id));

            Assert.False(await dbContext.DepartmentLocations.AnyAsync(dl => dl.DepartmentId == department.Id));

            Assert.True(await dbContext.Locations.AnyAsync(l => l.Id == locationId));
        });
    }

    [Fact]
    public async Task Process_WithExpiredAndRecentDepartments_ShouldDeleteOnlyExpiredDepartment()
    {
        // arrange
        var locationId = await CreateLocation();

        var expiredDepartment = await CreateParentDepartment("expired", "expired", [locationId]);

        var recentDepartment = await CreateParentDepartment("recent", "recent", [locationId]);

        await MarkDepartmentAsDeletedAt(expiredDepartment.Id, DateTime.UtcNow.AddMonths(-1).AddDays(-1));

        await MarkDepartmentAsDeletedAt(recentDepartment.Id, DateTime.UtcNow.AddDays(-10));

        // act
        var result = await ExecuteCleanup();

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var deletedDepartment = await dbContext.Departments
                .SingleAsync(d => d.Id == recentDepartment.Id);

            Assert.False(await dbContext.Departments.AnyAsync(d => d.Id == expiredDepartment.Id));

            Assert.False(await dbContext.DepartmentLocations.AnyAsync(dl => dl.DepartmentId == expiredDepartment.Id));

            Assert.False(
                await dbContext.Departments.AnyAsync(d =>
                    d.Id == deletedDepartment.Id && d.IsActive));

            Assert.True(await dbContext.Departments.AnyAsync(p => p.Id == deletedDepartment.Id));

            Assert.True(await dbContext.Locations.AnyAsync(l => l.Id == locationId));
        });
    }

    [Fact]
    public async Task Process_WhenDepartmentIsExpired_ShouldDeleteItsRelations()
    {
        // arrange
        var locationId = await CreateLocation();

        var expiredDepartment =
            await CreateParentDepartment("Expired", "expired", [locationId]);

        var activeDepartment =
            await CreateParentDepartment("active", "active", [locationId]);

        var positionId =
            await CreatePosition("developer", "description", [expiredDepartment.Id, activeDepartment.Id]);

        await MarkDepartmentAsDeletedAt(
            expiredDepartment.Id,
            DateTime.UtcNow.AddMonths(-2));

        // act
        var result = await ExecuteCleanup();

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            Assert.False(await dbContext.DepartmentLocations.AnyAsync(dl => dl.DepartmentId == expiredDepartment.Id));

            Assert.False(await dbContext.DepartmentPositions.AnyAsync(dp => dp.DepartmentId == expiredDepartment.Id));

            Assert.True(await dbContext.Departments.AnyAsync(d => d.Id == activeDepartment.Id));

            Assert.True(await dbContext.DepartmentLocations.AnyAsync(dl => dl.LocationId == locationId));
            Assert.True(await dbContext.DepartmentPositions.AnyAsync(dp => dp.PositionId == positionId));

            Assert.True(await dbContext.Locations.AnyAsync(l => l.Id == locationId));
            Assert.True(await dbContext.Positions.AnyAsync(p => p.Id == positionId));
        });
    }

    [Fact]
    public async Task Process_WhenDepartmentIsActive_ShouldKeepDepartment()
    {
        // arrange
        var locationId = await CreateLocation();

        var activeDepartment = await CreateParentDepartment("active", "active", [locationId]);

        // act
        var result = await ExecuteCleanup();

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .SingleAsync(d => d.Id == activeDepartment.Id);

            Assert.True(department.IsActive);
            Assert.Null(department.DeletedAt);
            Assert.True(await dbContext.DepartmentLocations.AnyAsync(dl => dl.DepartmentId == department.Id));
            Assert.True(await dbContext.Locations.AnyAsync(dl => dl.Id == locationId));
        });
    }

    [Fact]
    public async Task Process_WhenDepartmentDeletedRecently_ShouldKeepDepartment()
    {
        // arrange
        var locationId = await CreateLocation();

        var recentDepartment =
            await CreateParentDepartment("department", "department", [locationId]);

        await MarkDepartmentAsDeletedAt(recentDepartment.Id, DateTime.UtcNow.AddDays(-20));

        // act
        var result = await ExecuteCleanup();

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .SingleAsync(d => d.Id == recentDepartment.Id);

            Assert.False(department.IsActive);
            Assert.NotNull(department.DeletedAt);
            Assert.True(await dbContext.DepartmentLocations.AnyAsync(dl => dl.DepartmentId == department.Id));
        });
    }

    [Fact]
    public async Task Process_WhenDepartmentDeletedMoreThanMonthAgo_ShouldPhysicallyDeleteDepartment()
    {
        // arrange
        var locationId = await CreateLocation();

        var department = await CreateParentDepartment("department", "department", [locationId]);

        var oldDeletedAt = DateTime.UtcNow
            .AddMonths(-1)
            .AddDays(-1);

        var positionId = await CreatePosition("position", "description", [department.Id]);

        await MarkDepartmentAsDeletedAt(department.Id, oldDeletedAt);

        // act
        var result = await ExecuteCleanup();

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            Assert.False(await dbContext.Departments.AnyAsync(d => d.Id == department.Id));

            Assert.False(await dbContext.DepartmentLocations.AnyAsync(dl => dl.DepartmentId == department.Id));

            Assert.False(await dbContext.DepartmentPositions.AnyAsync(dp => dp.DepartmentId == department.Id));

            Assert.True(await dbContext.Positions.AnyAsync(p => p.Id == positionId));

            Assert.True(await dbContext.Locations.AnyAsync(l => l.Id == locationId));
        });
    }

    private Task<UnitResult<Error>> ExecuteCleanup() =>
        Execute<UnitResult<Error>, IDeletedEntitiesCleanupService>(service => service.Process(CancellationToken.None));
}