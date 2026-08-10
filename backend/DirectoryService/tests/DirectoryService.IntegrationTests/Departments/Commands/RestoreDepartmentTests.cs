using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.RestoreDepartments;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Departments.Commands;

public class RestoreDepartmentTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task RestoreDepartment_WhenDepartmentDoesNotExist_ShouldFail()
    {
        // arrange
        var department = Guid.NewGuid();

        var command = new RestoreDepartmentCommand(department);

        // act
        var result = await Execute(command);

        Assert.True(result.IsFailure);

        Assert.Contains(result.Error, e => e is { Code: "value.not.found", Type: ErrorType.NOT_FOUND });
    }

    [Fact]
    public async Task RestoreDepartment_WhenParentIsArchived_ShouldFail()
    {
        // arrange
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        var parentCommand = new SoftDeleteDepartmentCommand(company.Id.Value);

        var childCommand = new SoftDeleteDepartmentCommand(backend.Id.Value);

        var command = new RestoreDepartmentCommand(backend.Id.Value);

        // act
        var deleteParentResult = await ExecuteSoftDelete(parentCommand);
        var deleteChildResult = await ExecuteSoftDelete(childCommand);

        var result = await Execute(command);

        // assert
        Assert.True(deleteParentResult.IsSuccess);
        Assert.True(deleteChildResult.IsSuccess);
        Assert.True(result.IsFailure);

        await ExecuteInDb(async dbContext =>
        {
            var archivedChildDepartment = await dbContext.Departments.SingleAsync(
                d => d.Id == DepartmentId.Create(deleteChildResult.Value), CancellationToken.None);

            Assert.Equal("delete-company.delete-backend", archivedChildDepartment.Path.Value);
            Assert.False(archivedChildDepartment.IsActive);
            Assert.NotNull(archivedChildDepartment.DeletedAt);
        });

        Assert.Contains(
            result.Error,
            e => e is { Code: "department.parent.is.archived", Type: ErrorType.CONFLICT, InvalidField: "parentId" });
    }

    [Fact]
    public async Task RestoreDepartment_WhenDescendantIsArchived_ShouldPreserveDescendantArchiveMarker()
    {
        // arrange
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        var team =
            await CreateChildDepartment("team", "team", backend, [locationId]);

        var backendCommand = new SoftDeleteDepartmentCommand(backend.Id.Value);

        var companyCommand = new SoftDeleteDepartmentCommand(company.Id.Value);

        var command = new RestoreDepartmentCommand(company.Id.Value);

        // act
        var backendResult = await ExecuteSoftDelete(backendCommand);
        var commandResult = await ExecuteSoftDelete(companyCommand);

        await ExecuteInDb(async dbContext =>
        {
            var archivedDepartment = await dbContext.Departments.SingleAsync(
                d => d.Id == team.Id, CancellationToken.None);

            Assert.Equal("delete-company.delete-backend.team", archivedDepartment.Path.Value);
            Assert.True(archivedDepartment.IsActive);
            Assert.Null(archivedDepartment.DeletedAt);
        });
        var result = await Execute(command);

        // assert
        Assert.True(backendResult.IsSuccess);
        Assert.True(commandResult.IsSuccess);
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var restoredCompany = await dbContext.Departments
                .SingleAsync(d => d.Id == company.Id);

            Assert.Equal("company", restoredCompany.Path.Value);
            Assert.True(restoredCompany.IsActive);
            Assert.Null(restoredCompany.DeletedAt);

            var archivedBackend = await dbContext.Departments
                .SingleAsync(d => d.Id == backend.Id);

            Assert.Equal("company.delete-backend", archivedBackend.Path.Value);
            Assert.False(archivedBackend.IsActive);
            Assert.NotNull(archivedBackend.DeletedAt);

            var activeTeam = await dbContext.Departments
                .SingleAsync(d => d.Id == team.Id);

            Assert.Equal("company.delete-backend.team", activeTeam.Path.Value);
            Assert.True(activeTeam.IsActive);
            Assert.Null(activeTeam.DeletedAt);
        });
    }

    [Fact]
    public async Task RestoreDepartment_WhenDepartmentHasDescendants_ShouldSucceed()
    {
        // arrange
        var locationId = await CreateLocation();

        var company = await CreateParentDepartment("company", "company", [locationId]);

        var backend = await CreateChildDepartment("backend", "backend", company, [locationId]);

        var companyCommand = new SoftDeleteDepartmentCommand(company.Id.Value);

        var command = new RestoreDepartmentCommand(company.Id.Value);

        // act
        var companyResult = await ExecuteSoftDelete(companyCommand);
        var result = await Execute(command);

        Assert.True(companyResult.IsSuccess);
        Assert.True(result.IsSuccess);

        Assert.Equal(company.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var restoredCompany = await dbContext.Departments
                .SingleAsync(d => d.Id == DepartmentId.Create(result.Value), CancellationToken.None);

            Assert.Equal("company", restoredCompany.Path.Value);
            Assert.True(restoredCompany.IsActive);
            Assert.Null(restoredCompany.DeletedAt);

            var activeBackend = await dbContext.Departments.SingleAsync(
                d => d.Id == backend.Id, CancellationToken.None);

            Assert.Equal("company.backend", activeBackend.Path.Value);
            Assert.True(activeBackend.IsActive);
            Assert.Null(activeBackend.DeletedAt);
        });
    }

    [Fact]
    public async Task RestoreDepartment_WhenDepartmentIsAlreadyActive_ShouldSucceed()
    {
        // arrange
        var locationId = await CreateLocation();

        var company = await CreateParentDepartment("company", "company", [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        var updatedAtBeforeRestore = backend.UpdatedAt;

        var command = new RestoreDepartmentCommand(backend.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, backend.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            var restoredBackend = await dbContext.Departments.SingleAsync(
                d => d.Id == DepartmentId.Create(result.Value), CancellationToken.None);

            Assert.Equal("company.backend", restoredBackend.Path.Value);
            Assert.True(restoredBackend.IsActive);
            Assert.Null(restoredBackend.DeletedAt);
            Assert.Equal(updatedAtBeforeRestore, restoredBackend.UpdatedAt);
        });
    }

    [Fact]
    public async Task RestoreDepartment_WhenParentIsActive_ShouldSucceed()
    {
        // arrange
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var backend = await CreateChildDepartment("backend", "backend", company, [locationId]);

        var childCommand = new SoftDeleteDepartmentCommand(backend.Id.Value);

        var command = new RestoreDepartmentCommand(backend.Id.Value);

        // act
        var childResult = await ExecuteSoftDelete(childCommand);
        var result = await Execute(command);

        // assert
        Assert.True(childResult.IsSuccess);
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var restoredDepartment = await dbContext.Departments.SingleAsync(
                d => d.Id == DepartmentId.Create(childResult.Value), CancellationToken.None);

            Assert.Equal("company.backend", restoredDepartment.Path.Value);
            Assert.True(restoredDepartment.IsActive);
            Assert.Null(restoredDepartment.DeletedAt);
        });
    }

    [Fact]
    public async Task RestoreDepartment_WithValidData_ShouldSucceed()
    {
        // arrange
        var locationId = await CreateLocation();

        var department =
            await CreateParentDepartment("department restore department", "department", [locationId]);

        var softCommand = new SoftDeleteDepartmentCommand(department.Id.Value);

        var command = new RestoreDepartmentCommand(department.Id.Value);

        // act
        var delete = await ExecuteSoftDelete(softCommand);
        var result = await Execute(command);

        // assert
        Assert.True(delete.IsSuccess);
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var restoredDepartment =
                await dbContext.Departments.SingleAsync(
                    d => d.Id == DepartmentId.Create(result.Value), CancellationToken.None);

            Assert.Equal(department.Id.Value, result.Value);
            Assert.Equal("department", restoredDepartment.Path.Value);
            Assert.True(restoredDepartment.IsActive);
            Assert.Null(restoredDepartment.DeletedAt);
        });
    }

    private Task<Result<Guid, Errors>> Execute(RestoreDepartmentCommand command) =>
        Execute<Result<Guid, Errors>, RestoreDepartmentHandler>(handler => handler.Handle(
            command, CancellationToken.None));

    private Task<Result<Guid, Errors>> ExecuteSoftDelete(SoftDeleteDepartmentCommand command) =>
        Execute<Result<Guid, Errors>, SoftDeleteDepartmentHandler>(handler => handler.Handle(
            command, CancellationToken.None));
}