using DirectoryService.Application.Departments.Commands.UpdateDepartments;
using DirectoryService.Contracts.Departments.UpdateDepartment;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateDepartmentLocationIdsTests : DirectoryBaseTests
{
    public UpdateDepartmentLocationIdsTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }

    [Fact]
    public async Task UpdateDepartmentLocationsWithValidShouldSucceed()
    {
        // arrange
        var locationId = await CreateLocation("location 1");

        var cancellationToken = CancellationToken.None;

        var parent = await CreateParentDepartment("подразделение", "company", [locationId]);

        // act
        var result = await ExecuteUpdateDepartmentLocationIdsHandler(
            sut =>
            {
                var command = new UpdateDepartmentLocationIdsCommand(
                    parent.Id.Value, new UpdateDepartmentLocationIdsRequest([locationId.Value]));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        await ExecuteInDb(
            async dbContext =>
            {
                var department = await dbContext.Departments.FirstAsync(
                    d => d.Id == DepartmentId.Create(result.Value), cancellationToken);

                var departmentLocations = await dbContext.DepartmentLocations
                    .FirstAsync(dl => dl.LocationId == locationId, cancellationToken);

                Assert.NotNull(department);
                Assert.Equal(department.Id.Value, result.Value);
                Assert.NotNull(departmentLocations);
            });

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task UpdateDepartmentLocationsWithInvalidShouldFailed()
    {
        // arrange
        var departmentId = DepartmentId.CreateNew();

        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteUpdateDepartmentLocationIdsHandler(
            sut =>
            {
                var command = new UpdateDepartmentLocationIdsCommand(
                    departmentId.Value, new UpdateDepartmentLocationIdsRequest([]));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    private async Task<T> ExecuteUpdateDepartmentLocationIdsHandler<T>(Func<UpdateDepartmentLocationIdsHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<UpdateDepartmentLocationIdsHandler>();

        return await action(sut);
    }
}