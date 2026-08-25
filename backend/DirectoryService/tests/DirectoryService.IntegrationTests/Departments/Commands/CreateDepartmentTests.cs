using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.CreateDepartments;
using DirectoryService.Contracts.Departments.CreateDepartments;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Departments.Commands;

public class CreateDepartmentTests : DirectoryBaseTests
{
    public CreateDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }

    [Fact]
    public async Task CreateDepartmentWithoutLocationShouldFailed()
    {
        // arrange
        var command = CreateCommand("подразделение", "company", null, []);

        // act
        var result = await Execute(command);

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

        var command = CreateCommand("подразделение", "company", null, [locationId.Value]);

        // act
        var result = await Execute(command);

        // assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartmentInvalidDataShouldFailed()
    {
        // arrange
        var locationId = await CreateLocation("локация");

        var command = CreateCommand(string.Empty, "company", null, [locationId.Value]);

        // act
        var result = await Execute(command);

        // assert
        Assert.NotEmpty(result.Error);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task CreateDepartment_WhenIdentifierUsesArchivedPathPrefix_ShouldFail()
    {
        // arrange
        var locationId = await CreateLocation("локация");

        var command = CreateCommand("подразделение", "delete-company", null, [locationId.Value]);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Error, error => error.InvalidField == "department.identifier");
    }

    [Fact]
    public async Task CreateDepartmentWithParentAndChildShouldSucceed()
    {
        // arrange
        var locationId = await CreateLocation("Location 1");

        var cancellationToken = CancellationToken.None;

        var parent = await CreateParentDepartment("подразделение", "company", [locationId]);

        var command = CreateCommand("подразделение 1", "sales", parent.Id.Value, [locationId.Value]);

        // act
        var result = await Execute(command);

        // assert
        await ExecuteInDb(async dbContext =>
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
        var locationId = await CreateLocation("Location 1");

        var cancellationToken = CancellationToken.None;

        var command = CreateCommand("подразделение", "company", null, [locationId.Value]);

        // act
        var result = await Execute(command);

        // assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments.FirstAsync(
                d => d.Id == DepartmentId.Create(result.Value), cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(department.Id.Value, result.Value);
        });

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    private static CreateDepartmentCommand CreateCommand(
        string name, string identifier, Guid? parentId, Guid[] locationIds) =>
        new(new CreateDepartmentRequest(name, identifier, parentId, locationIds));

    private Task<Result<Guid, Errors>> Execute(CreateDepartmentCommand command)
        => Execute<Result<Guid, Errors>, CreateDepartmentHandler>(handler => handler.Handle(
            command, CancellationToken.None));
}