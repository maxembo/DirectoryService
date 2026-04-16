using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.UpdateDepartments;
using DirectoryService.Contracts.Departments.UpdateDepartment;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateDepartmentLocationIdsTests : DirectoryBaseTests
{
    public UpdateDepartmentLocationIdsTests(DirectoryTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task UpdateDepartmentLocationsWithValidShouldSucceed()
    {
        // arrange
        var locationId = await CreateLocation("location 1");

        var cancellationToken = CancellationToken.None;

        var parent = await CreateParentDepartment("подразделение", "company", [locationId]);

        // act
        var result = await Execute(
            new UpdateDepartmentLocationIdsCommand(
                parent.Id.Value, new UpdateDepartmentLocationIdsRequest([locationId.Value])));

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

        // act
        var result = await Execute(
            new UpdateDepartmentLocationIdsCommand(departmentId.Value, new UpdateDepartmentLocationIdsRequest([])));

        // assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    private Task<Result<Guid, Errors>> Execute(UpdateDepartmentLocationIdsCommand command)
        => Execute<Result<Guid, Errors>, UpdateDepartmentLocationIdsHandler>(
            handler => handler.Handle(command, CancellationToken.None));
}