using DirectoryService.Application.Departments.Commands.CreateDepartments;
using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTests : DirectoryBaseTests
{
    public CreateDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }

    [Fact]
    public async Task CreateDepartmentWithoutLocationShouldFailed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteCreateDepartmentHandler(
            sut =>
            {
                var command =
                    new CreateDepartmentCommand(new CreateDepartmentRequest("подразделение", "company", null, []));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartmentDuplicateShouldFailed()
    {
        // arrange
        var locationId = await CreateLocation("локация 1");

        var cancellationToken = CancellationToken.None;

        await CreateParentDepartment("подразделение", "company", [locationId]);

        // act
        var result = await ExecuteCreateDepartmentHandler(
            sut =>
            {
                var command = new CreateDepartmentCommand(
                    new CreateDepartmentRequest("подразделение", "company", null, [locationId.Value]));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartmentInvalidDataShouldFailed()
    {
        // arrange
        var locationId = await CreateLocation("локация");

        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteCreateDepartmentHandler(
            sut =>
            {
                var command = new CreateDepartmentCommand(
                    new CreateDepartmentRequest("", "company", null, [locationId.Value]));

                return sut.Handle(command, cancellationToken);
            });

        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartmentWithParentAndChildShouldSucceed()
    {
        // arrange
        LocationId locationId = await CreateLocation("Location 1");

        var cancellationToken = CancellationToken.None;

        var parent = await CreateParentDepartment("подразделение", "company", [locationId]);

        // act
        var result = await ExecuteCreateDepartmentHandler(
            sut =>
            {
                var command = new CreateDepartmentCommand(
                    new CreateDepartmentRequest("подразделение 1", "sales", parent.Id.Value, [locationId.Value]));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        await ExecuteInDb(
            async dbContext =>
            {
                var department = await dbContext.Departments.FirstAsync(
                    d => d.Id == DepartmentId.Create(result.Value), cancellationToken);

                Assert.NotNull(department);
                Assert.Equal(department.Id.Value, result.Value);
            });

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task CreateDepartmentWithoutParentShouldSucceed()
    {
        // arrange
        LocationId locationId = await CreateLocation("Location 1");

        var cancellationToken = CancellationToken.None;

        // act
        var result = await ExecuteCreateDepartmentHandler(
            sut =>
            {
                var command = new CreateDepartmentCommand(
                    new CreateDepartmentRequest("подразделение", "company", null, [locationId.Value]));

                return sut.Handle(command, cancellationToken);
            });

        // assert
        await ExecuteInDb(
            async dbContext =>
            {
                var department = await dbContext.Departments.FirstAsync(
                    d => d.Id == DepartmentId.Create(result.Value), cancellationToken);

                Assert.NotNull(department);
                Assert.Equal(department.Id.Value, result.Value);
            });

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    private async Task<T> ExecuteCreateDepartmentHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

        return await action(sut);
    }
}