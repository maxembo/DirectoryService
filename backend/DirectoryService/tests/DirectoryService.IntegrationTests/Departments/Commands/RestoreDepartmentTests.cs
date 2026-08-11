using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.RestoreDepartments;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Departments.Commands;

public class RestoreDepartmentTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task
        RestoreDepartment_WhenSharedResourcesWereArchivedAfterLastDepartmentDeletion_ShouldRestoreResources()
    {
        // arrange
        var locationId = await CreateLocation();

        var firstCompany =
            await CreateParentDepartment("firstCompany", "first-company", [locationId]);

        var secondCompany =
            await CreateParentDepartment("secondCompany", "second-company", [locationId]);

        var positionId = await CreatePosition("position", null, [firstCompany.Id, secondCompany.Id]);

        var command = new RestoreDepartmentCommand(firstCompany.Id.Value);

        // soft delete
        var softDeleteFirstCompanyResult =
            await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(firstCompany.Id.Value));

        Assert.True(softDeleteFirstCompanyResult.IsSuccess);
        Assert.Equal(softDeleteFirstCompanyResult.Value, firstCompany.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .SingleAsync(d => d.Id == locationId, CancellationToken.None);

            Assert.True(location.IsActive);
            Assert.Null(location.DeletedAt);
            Assert.Null(location.DeletionReason);

            var position = await dbContext.Positions
                .SingleAsync(p => p.Id == positionId, CancellationToken.None);

            Assert.True(position.IsActive);
            Assert.Null(position.DeletedAt);
            Assert.Null(position.DeletionReason);
        });

        // soft delete
        var softDeleteSecondCompanyResult =
            await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(secondCompany.Id.Value));

        Assert.True(softDeleteSecondCompanyResult.IsSuccess);
        Assert.Equal(softDeleteSecondCompanyResult.Value, secondCompany.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            var softDeleteLocation = await dbContext.Locations
                .SingleAsync(d => d.Id == locationId, CancellationToken.None);

            Assert.False(softDeleteLocation.IsActive);
            Assert.NotNull(softDeleteLocation.DeletedAt);
            Assert.Equal(softDeleteLocation.DeletionReason, DeletionReason.NO_ACTIVE_DEPARTMENTS);

            var softDeletePosition = await dbContext.Positions
                .SingleAsync(p => p.Id == positionId, CancellationToken.None);

            Assert.False(softDeletePosition.IsActive);
            Assert.NotNull(softDeletePosition.DeletedAt);
            Assert.Equal(softDeletePosition.DeletionReason, DeletionReason.NO_ACTIVE_DEPARTMENTS);
        });

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, firstCompany.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            var restoredDepartment = await dbContext.Departments.SingleAsync(
                d => d.Id == DepartmentId.Create(result.Value), CancellationToken.None);

            Assert.True(restoredDepartment.IsActive);

            var restoredLocation = await dbContext.Locations.SingleAsync(
                d => d.Id == locationId, CancellationToken.None);

            Assert.True(restoredLocation.IsActive);
            Assert.Null(restoredLocation.DeletedAt);

            Assert.Null(restoredLocation.DeletionReason);

            var restoredPosition = await dbContext.Positions.SingleAsync(
                d => d.Id == positionId, CancellationToken.None);

            Assert.True(restoredPosition.IsActive);
            Assert.Null(restoredPosition.DeletedAt);

            Assert.Null(restoredPosition.DeletionReason);
        });
    }

    [Fact]
    public async Task RestoreDepartment_WhenPositionWasManuallyArchived_ShouldNotRestorePosition()
    {
        // arrange
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var positionId = await CreatePosition("position", null, [company.Id]);

        await MarkPositionAsDeleted(positionId);

        var command = new RestoreDepartmentCommand(company.Id.Value);

        // soft delete
        var softDeleteCompanyResult = await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(company.Id.Value));

        Assert.True(softDeleteCompanyResult.IsSuccess);
        Assert.Equal(softDeleteCompanyResult.Value, company.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, company.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            var restoredDepartment = await dbContext.Departments.SingleAsync(
                d => d.Id == DepartmentId.Create(result.Value), CancellationToken.None);

            Assert.True(restoredDepartment.IsActive);

            var restorePosition = await dbContext.Positions.SingleAsync(
                d => d.Id == positionId, CancellationToken.None);

            Assert.False(restorePosition.IsActive);
            Assert.NotNull(restorePosition.DeletedAt);

            Assert.Equal(DeletionReason.MANUAL, restorePosition.DeletionReason);
        });
    }

    [Fact]
    public async Task RestoreDepartment_WhenLocationWasManuallyArchived_ShouldNotRestoreLocation()
    {
        // arrange
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        await MarkLocationAsDeleted(locationId);

        var command = new RestoreDepartmentCommand(company.Id.Value);

        // soft delete
        var softDeleteCompanyResult = await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(company.Id.Value));

        Assert.True(softDeleteCompanyResult.IsSuccess);
        Assert.Equal(softDeleteCompanyResult.Value, company.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, company.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            var restoredDepartment = await dbContext.Departments.SingleAsync(
                d => d.Id == DepartmentId.Create(result.Value), CancellationToken.None);

            Assert.True(restoredDepartment.IsActive);

            var restoredLocation = await dbContext.Locations.SingleAsync(
                d => d.Id == locationId, CancellationToken.None);

            Assert.False(restoredLocation.IsActive);
            Assert.NotNull(restoredLocation.DeletedAt);

            Assert.Equal(DeletionReason.MANUAL, restoredLocation.DeletionReason);
        });
    }

    [Fact]
    public async Task RestoreDepartment_WhenPositionWasAutomaticallyArchived_ShouldRestorePosition()
    {
        // arrange
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var positionId = await CreatePosition("position", null, [company.Id]);

        var command = new RestoreDepartmentCommand(company.Id.Value);

        // soft delete
        var softDeleteCompanyResult = await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(company.Id.Value));

        Assert.True(softDeleteCompanyResult.IsSuccess);
        Assert.Equal(softDeleteCompanyResult.Value, company.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            var softDeletePosition = await dbContext.Positions.SingleAsync(
                l => l.Id == positionId, CancellationToken.None);

            Assert.False(softDeletePosition.IsActive);
            Assert.NotNull(softDeletePosition.DeletedAt);

            Assert.Equal(DeletionReason.NO_ACTIVE_DEPARTMENTS, softDeletePosition.DeletionReason);
        });

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, company.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            var restoredPosition = await dbContext.Positions.SingleAsync(
                d => d.Id == positionId, CancellationToken.None);

            Assert.True(restoredPosition.IsActive);
            Assert.Null(restoredPosition.DeletedAt);

            Assert.Null(restoredPosition.DeletionReason);
        });
    }

    [Fact]
    public async Task RestoreDepartment_WhenLocationWasAutomaticallyArchived_ShouldRestoreLocation()
    {
        // arrange
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var command = new RestoreDepartmentCommand(company.Id.Value);

        // soft delete
        var softDeleteCompanyResult = await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(company.Id.Value));

        Assert.True(softDeleteCompanyResult.IsSuccess);
        Assert.Equal(softDeleteCompanyResult.Value, company.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            var softDeleteLocation = await dbContext.Locations.SingleAsync(
                l => l.Id == locationId, CancellationToken.None);

            Assert.False(softDeleteLocation.IsActive);
            Assert.NotNull(softDeleteLocation.DeletedAt);

            Assert.Equal(DeletionReason.NO_ACTIVE_DEPARTMENTS, softDeleteLocation.DeletionReason);
        });

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, company.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            var restoredLocation = await dbContext.Locations.SingleAsync(
                d => d.Id == locationId, CancellationToken.None);

            Assert.True(restoredLocation.IsActive);
            Assert.Null(restoredLocation.DeletedAt);

            Assert.Null(restoredLocation.DeletionReason);
        });
    }

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