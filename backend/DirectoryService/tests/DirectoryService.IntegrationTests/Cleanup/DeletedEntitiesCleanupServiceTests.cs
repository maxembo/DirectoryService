using CSharpFunctionalExtensions;
using DirectoryService.Application.Cleanup;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
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
        LocationId? locationId = await CreateLocation();

        Department? company =
            await CreateParentDepartment("company", "company", [locationId]);

        Department? development =
            await CreateChildDepartment("development", "development", company, [locationId]);

        Department? backend =
            await CreateChildDepartment("backend", "backend", development, [locationId]);

        Department? team = await CreateChildDepartment("team", "team", backend, [locationId]);

        await MarkDepartmentAsDeletedAt(development.Id, DateTime.UtcNow.AddMonths(-2));
        await MarkDepartmentAsDeletedAt(backend.Id, DateTime.UtcNow.AddMonths(-1));

        // act
        UnitResult<Error> result = await ExecuteCleanup();

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            Department? updatedTeam = await dbContext.Departments
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
        LocationId? locationId = await CreateLocation();

        Department? company =
            await CreateParentDepartment("company", "company", [locationId]);

        Department? development =
            await CreateChildDepartment("development", "development", company, [locationId]);

        Department? backend =
            await CreateChildDepartment("backend", "backend", development, [locationId]);

        await MarkDepartmentAsDeletedAt(development.Id, DateTime.UtcNow.AddMonths(-2));

        // act
        UnitResult<Error> result = await ExecuteCleanup();

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            Department? updateBackend = await dbContext.Departments
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
        LocationId? locationId = await CreateLocation();

        Department? department =
            await CreateParentDepartment("department", "department", [locationId]);

        PositionId? expiredPositionId =
            await CreatePosition("developer", "description", [department.Id]);

        PositionId? recentPositionId = await CreatePosition("recent", "recent", [department.Id]);

        await MarkPositionAsDeletedAt(expiredPositionId, DateTime.UtcNow.AddDays(-31));
        await MarkPositionAsDeletedAt(recentPositionId, DateTime.UtcNow.AddDays(-14));

        // act
        UnitResult<Error> result = await ExecuteCleanup();

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
        LocationId? expiredLocationId = await CreateLocation();
        LocationId? recentLocationId = await CreateLocation("expired", country: "expired");

        Department? department = await CreateParentDepartment(
            "department", "department", [expiredLocationId, recentLocationId]);

        await MarkLocationAsDeletedAt(expiredLocationId, DateTime.UtcNow.AddMonths(-1).AddDays(-1));
        await MarkLocationAsDeletedAt(recentLocationId, DateTime.UtcNow.AddDays(-5));

        // act
        UnitResult<Error> result = await ExecuteCleanup();

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
        LocationId? locationId = await CreateLocation();

        Department? department =
            await CreateParentDepartment("first-department", "first-department", [locationId]);

        await MarkDepartmentAsDeletedAt(department.Id, DateTime.UtcNow.AddMonths(-1).AddDays(-1));

        // act
        UnitResult<Error> firstResult = await ExecuteCleanup();
        UnitResult<Error> secondResult = await ExecuteCleanup();

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
        LocationId? locationId = await CreateLocation();

        Department? expiredDepartment = await CreateParentDepartment("expired", "expired", [locationId]);

        Department? recentDepartment = await CreateParentDepartment("recent", "recent", [locationId]);

        await MarkDepartmentAsDeletedAt(expiredDepartment.Id, DateTime.UtcNow.AddMonths(-1).AddDays(-1));

        await MarkDepartmentAsDeletedAt(recentDepartment.Id, DateTime.UtcNow.AddDays(-10));

        // act
        UnitResult<Error> result = await ExecuteCleanup();

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            Department? deletedDepartment = await dbContext.Departments
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
        LocationId? locationId = await CreateLocation();

        Department? expiredDepartment =
            await CreateParentDepartment("Expired", "expired", [locationId]);

        Department? activeDepartment =
            await CreateParentDepartment("active", "active", [locationId]);

        PositionId? positionId =
            await CreatePosition("developer", "description", [expiredDepartment.Id, activeDepartment.Id]);

        await MarkDepartmentAsDeletedAt(
            expiredDepartment.Id,
            DateTime.UtcNow.AddMonths(-2));

        // act
        UnitResult<Error> result = await ExecuteCleanup();

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
        LocationId? locationId = await CreateLocation();

        Department? activeDepartment = await CreateParentDepartment("active", "active", [locationId]);

        // act
        UnitResult<Error> result = await ExecuteCleanup();

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            Department? department = await dbContext.Departments
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
        LocationId? locationId = await CreateLocation();

        Department? recentDepartment =
            await CreateParentDepartment("department", "department", [locationId]);

        await MarkDepartmentAsDeletedAt(recentDepartment.Id, DateTime.UtcNow.AddDays(-20));

        // act
        UnitResult<Error> result = await ExecuteCleanup();

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            Department? department = await dbContext.Departments
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
        LocationId? locationId = await CreateLocation();

        Department? department = await CreateParentDepartment("department", "department", [locationId]);

        DateTime oldDeletedAt = DateTime.UtcNow
            .AddMonths(-1)
            .AddDays(-1);

        PositionId? positionId = await CreatePosition("position", "description", [department.Id]);

        await MarkDepartmentAsDeletedAt(department.Id, oldDeletedAt);

        // act
        UnitResult<Error> result = await ExecuteCleanup();

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