using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.RestoreDepartments;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
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
        LocationId? locationId = await CreateLocation();

        Department? firstCompany =
            await CreateParentDepartment("firstCompany", "first-company", [locationId]);

        Department? secondCompany =
            await CreateParentDepartment("secondCompany", "second-company", [locationId]);

        PositionId? positionId = await CreatePosition("position", null, [firstCompany.Id, secondCompany.Id]);

        var command = new RestoreDepartmentCommand(firstCompany.Id.Value);

        // soft delete
        Result<Guid, Errors> softDeleteFirstCompanyResult =
            await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(firstCompany.Id.Value));

        Assert.True(softDeleteFirstCompanyResult.IsSuccess);
        Assert.Equal(softDeleteFirstCompanyResult.Value, firstCompany.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            Location? location = await dbContext.Locations
                .SingleAsync(d => d.Id == locationId, CancellationToken.None);

            Assert.True(location.IsActive);
            Assert.Null(location.DeletedAt);
            Assert.Null(location.DeletionReason);

            Position? position = await dbContext.Positions
                .SingleAsync(p => p.Id == positionId, CancellationToken.None);

            Assert.True(position.IsActive);
            Assert.Null(position.DeletedAt);
            Assert.Null(position.DeletionReason);
        });

        // soft delete
        Result<Guid, Errors> softDeleteSecondCompanyResult =
            await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(secondCompany.Id.Value));

        Assert.True(softDeleteSecondCompanyResult.IsSuccess);
        Assert.Equal(softDeleteSecondCompanyResult.Value, secondCompany.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            Location? softDeleteLocation = await dbContext.Locations
                .SingleAsync(d => d.Id == locationId, CancellationToken.None);

            Assert.False(softDeleteLocation.IsActive);
            Assert.NotNull(softDeleteLocation.DeletedAt);
            Assert.Equal(softDeleteLocation.DeletionReason, DeletionReason.NO_ACTIVE_DEPARTMENTS);

            Position? softDeletePosition = await dbContext.Positions
                .SingleAsync(p => p.Id == positionId, CancellationToken.None);

            Assert.False(softDeletePosition.IsActive);
            Assert.NotNull(softDeletePosition.DeletedAt);
            Assert.Equal(softDeletePosition.DeletionReason, DeletionReason.NO_ACTIVE_DEPARTMENTS);
        });

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, firstCompany.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            Department? restoredDepartment = await dbContext.Departments.SingleAsync(
                d => d.Id == DepartmentId.Create(result.Value), CancellationToken.None);

            Assert.True(restoredDepartment.IsActive);

            Location? restoredLocation = await dbContext.Locations.SingleAsync(
                d => d.Id == locationId, CancellationToken.None);

            Assert.True(restoredLocation.IsActive);
            Assert.Null(restoredLocation.DeletedAt);

            Assert.Null(restoredLocation.DeletionReason);

            Position? restoredPosition = await dbContext.Positions.SingleAsync(
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
        LocationId? locationId = await CreateLocation();

        Department? company =
            await CreateParentDepartment("company", "company", [locationId]);

        PositionId? positionId = await CreatePosition("position", null, [company.Id]);

        await MarkPositionAsDeleted(positionId);

        var command = new RestoreDepartmentCommand(company.Id.Value);

        // soft delete
        Result<Guid, Errors> softDeleteCompanyResult =
            await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(company.Id.Value));

        Assert.True(softDeleteCompanyResult.IsSuccess);
        Assert.Equal(softDeleteCompanyResult.Value, company.Id.Value);

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, company.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            Department? restoredDepartment = await dbContext.Departments.SingleAsync(
                d => d.Id == DepartmentId.Create(result.Value), CancellationToken.None);

            Assert.True(restoredDepartment.IsActive);

            Position? restorePosition = await dbContext.Positions.SingleAsync(
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
        LocationId? locationId = await CreateLocation();

        Department? company =
            await CreateParentDepartment("company", "company", [locationId]);

        await MarkLocationAsDeleted(locationId);

        var command = new RestoreDepartmentCommand(company.Id.Value);

        // soft delete
        Result<Guid, Errors> softDeleteCompanyResult =
            await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(company.Id.Value));

        Assert.True(softDeleteCompanyResult.IsSuccess);
        Assert.Equal(softDeleteCompanyResult.Value, company.Id.Value);

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, company.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            Department? restoredDepartment = await dbContext.Departments.SingleAsync(
                d => d.Id == DepartmentId.Create(result.Value), CancellationToken.None);

            Assert.True(restoredDepartment.IsActive);

            Location? restoredLocation = await dbContext.Locations.SingleAsync(
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
        LocationId? locationId = await CreateLocation();

        Department? company =
            await CreateParentDepartment("company", "company", [locationId]);

        PositionId? positionId = await CreatePosition("position", null, [company.Id]);

        var command = new RestoreDepartmentCommand(company.Id.Value);

        // soft delete
        Result<Guid, Errors> softDeleteCompanyResult =
            await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(company.Id.Value));

        Assert.True(softDeleteCompanyResult.IsSuccess);
        Assert.Equal(softDeleteCompanyResult.Value, company.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            Position? softDeletePosition = await dbContext.Positions.SingleAsync(
                l => l.Id == positionId, CancellationToken.None);

            Assert.False(softDeletePosition.IsActive);
            Assert.NotNull(softDeletePosition.DeletedAt);

            Assert.Equal(DeletionReason.NO_ACTIVE_DEPARTMENTS, softDeletePosition.DeletionReason);
        });

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, company.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            Position? restoredPosition = await dbContext.Positions.SingleAsync(
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
        LocationId? locationId = await CreateLocation();

        Department? company =
            await CreateParentDepartment("company", "company", [locationId]);

        var command = new RestoreDepartmentCommand(company.Id.Value);

        // soft delete
        Result<Guid, Errors> softDeleteCompanyResult =
            await ExecuteSoftDelete(new SoftDeleteDepartmentCommand(company.Id.Value));

        Assert.True(softDeleteCompanyResult.IsSuccess);
        Assert.Equal(softDeleteCompanyResult.Value, company.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            Location? softDeleteLocation = await dbContext.Locations.SingleAsync(
                l => l.Id == locationId, CancellationToken.None);

            Assert.False(softDeleteLocation.IsActive);
            Assert.NotNull(softDeleteLocation.DeletedAt);

            Assert.Equal(DeletionReason.NO_ACTIVE_DEPARTMENTS, softDeleteLocation.DeletionReason);
        });

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, company.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            Location? restoredLocation = await dbContext.Locations.SingleAsync(
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
        Result<Guid, Errors> result = await Execute(command);

        Assert.True(result.IsFailure);

        Assert.Contains(result.Error, e => e is { Code: "value.not.found", Type: ErrorType.NOT_FOUND });
    }

    [Fact]
    public async Task RestoreDepartment_WhenParentIsArchived_ShouldFail()
    {
        // arrange
        LocationId? locationId = await CreateLocation();

        Department? company =
            await CreateParentDepartment("company", "company", [locationId]);

        Department? backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        var parentCommand = new SoftDeleteDepartmentCommand(company.Id.Value);

        var childCommand = new SoftDeleteDepartmentCommand(backend.Id.Value);

        var command = new RestoreDepartmentCommand(backend.Id.Value);

        // act
        Result<Guid, Errors> deleteParentResult = await ExecuteSoftDelete(parentCommand);
        Result<Guid, Errors> deleteChildResult = await ExecuteSoftDelete(childCommand);

        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(deleteParentResult.IsSuccess);
        Assert.True(deleteChildResult.IsSuccess);
        Assert.True(result.IsFailure);

        await ExecuteInDb(async dbContext =>
        {
            Department? archivedChildDepartment = await dbContext.Departments.SingleAsync(
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
        LocationId? locationId = await CreateLocation();

        Department? company =
            await CreateParentDepartment("company", "company", [locationId]);

        Department? backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        Department? team =
            await CreateChildDepartment("team", "team", backend, [locationId]);

        var backendCommand = new SoftDeleteDepartmentCommand(backend.Id.Value);

        var companyCommand = new SoftDeleteDepartmentCommand(company.Id.Value);

        var command = new RestoreDepartmentCommand(company.Id.Value);

        // act
        Result<Guid, Errors> backendResult = await ExecuteSoftDelete(backendCommand);
        Result<Guid, Errors> commandResult = await ExecuteSoftDelete(companyCommand);

        await ExecuteInDb(async dbContext =>
        {
            Department? archivedDepartment = await dbContext.Departments.SingleAsync(
                d => d.Id == team.Id, CancellationToken.None);

            Assert.Equal("delete-company.delete-backend.team", archivedDepartment.Path.Value);
            Assert.True(archivedDepartment.IsActive);
            Assert.Null(archivedDepartment.DeletedAt);
        });
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(backendResult.IsSuccess);
        Assert.True(commandResult.IsSuccess);
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            Department? restoredCompany = await dbContext.Departments
                .SingleAsync(d => d.Id == company.Id);

            Assert.Equal("company", restoredCompany.Path.Value);
            Assert.True(restoredCompany.IsActive);
            Assert.Null(restoredCompany.DeletedAt);

            Department? archivedBackend = await dbContext.Departments
                .SingleAsync(d => d.Id == backend.Id);

            Assert.Equal("company.delete-backend", archivedBackend.Path.Value);
            Assert.False(archivedBackend.IsActive);
            Assert.NotNull(archivedBackend.DeletedAt);

            Department? activeTeam = await dbContext.Departments
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
        LocationId? locationId = await CreateLocation();

        Department? company = await CreateParentDepartment("company", "company", [locationId]);

        Department? backend = await CreateChildDepartment("backend", "backend", company, [locationId]);

        var companyCommand = new SoftDeleteDepartmentCommand(company.Id.Value);

        var command = new RestoreDepartmentCommand(company.Id.Value);

        // act
        Result<Guid, Errors> companyResult = await ExecuteSoftDelete(companyCommand);
        Result<Guid, Errors> result = await Execute(command);

        Assert.True(companyResult.IsSuccess);
        Assert.True(result.IsSuccess);

        Assert.Equal(company.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            Department? restoredCompany = await dbContext.Departments
                .SingleAsync(d => d.Id == DepartmentId.Create(result.Value), CancellationToken.None);

            Assert.Equal("company", restoredCompany.Path.Value);
            Assert.True(restoredCompany.IsActive);
            Assert.Null(restoredCompany.DeletedAt);

            Department? activeBackend = await dbContext.Departments.SingleAsync(
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
        LocationId? locationId = await CreateLocation();

        Department? company = await CreateParentDepartment("company", "company", [locationId]);

        Department? backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        DateTime updatedAtBeforeRestore = backend.UpdatedAt;

        var command = new RestoreDepartmentCommand(backend.Id.Value);

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, backend.Id.Value);

        await ExecuteInDb(async dbContext =>
        {
            Department? restoredBackend = await dbContext.Departments.SingleAsync(
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
        LocationId? locationId = await CreateLocation();

        Department? company =
            await CreateParentDepartment("company", "company", [locationId]);

        Department? backend = await CreateChildDepartment("backend", "backend", company, [locationId]);

        var childCommand = new SoftDeleteDepartmentCommand(backend.Id.Value);

        var command = new RestoreDepartmentCommand(backend.Id.Value);

        // act
        Result<Guid, Errors> childResult = await ExecuteSoftDelete(childCommand);
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(childResult.IsSuccess);
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            Department? restoredDepartment = await dbContext.Departments.SingleAsync(
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
        LocationId? locationId = await CreateLocation();

        Department? department =
            await CreateParentDepartment("department restore department", "department", [locationId]);

        var softCommand = new SoftDeleteDepartmentCommand(department.Id.Value);

        var command = new RestoreDepartmentCommand(department.Id.Value);

        // act
        Result<Guid, Errors> delete = await ExecuteSoftDelete(softCommand);
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(delete.IsSuccess);
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            Department? restoredDepartment =
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