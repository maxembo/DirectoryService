using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Departments.Commands;

public class SoftDeleteDepartmentTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task SoftDeleteDepartment_WhenPositionHasAnotherActiveDepartment_ShouldKeepPositionActive()
    {
        // arrange
        LocationId? locationId = await CreateLocation();

        Department? firstCompany =
            await CreateParentDepartment("firstCompany", "first-company", [locationId]);

        Department? secondCompany =
            await CreateParentDepartment("secondCompany", "second-company", [locationId]);

        PositionId? positionId =
            await CreatePosition("name", null, [firstCompany.Id, secondCompany.Id]);

        var command = new SoftDeleteDepartmentCommand(firstCompany.Id.Value);

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(firstCompany.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            Position? position = await dbContext.Positions
                .SingleAsync(l => l.Id == positionId, CancellationToken.None);

            Assert.True(position.IsActive);
            Assert.Null(position.DeletedAt);

            Assert.Null(position.DeletionReason);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_WhenLocationHasAnotherActiveDepartment_ShouldKeepLocationActive()
    {
        // arrange
        LocationId? locationId = await CreateLocation();

        Department? company =
            await CreateParentDepartment("firstCompany", "first-company", [locationId]);

        await CreateParentDepartment("secondCompany", "second-company", [locationId]);

        var command = new SoftDeleteDepartmentCommand(company.Id.Value);

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(company.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            Location? location = await dbContext.Locations
                .SingleAsync(l => l.Id == locationId, CancellationToken.None);

            Assert.True(location.IsActive);
            Assert.Null(location.DeletedAt);

            Assert.Null(location.DeletionReason);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_WhenPositionHasNoOtherActiveDepartments_ShouldArchivePositionAutomatically()
    {
        // arrange
        LocationId? locationId = await CreateLocation();

        Department? company =
            await CreateParentDepartment("company", "company", [locationId]);

        PositionId? positionId = await CreatePosition("position", null, [company.Id]);

        var command = new SoftDeleteDepartmentCommand(company.Id.Value);

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(company.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            Position? position = await dbContext.Positions
                .SingleAsync(l => l.Id == positionId, CancellationToken.None);

            Assert.False(position.IsActive);
            Assert.NotNull(position.DeletedAt);

            Assert.Equal(DeletionReason.NO_ACTIVE_DEPARTMENTS, position.DeletionReason);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_WhenLocationHasNoOtherActiveDepartments_ShouldArchiveLocationAutomatically()
    {
        // arrange
        LocationId? locationId = await CreateLocation();

        Department? company =
            await CreateParentDepartment("company", "company", [locationId]);

        var command = new SoftDeleteDepartmentCommand(company.Id.Value);

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(company.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            Location? location = await dbContext.Locations
                .SingleAsync(l => l.Id == locationId, CancellationToken.None);

            Assert.False(location.IsActive);
            Assert.NotNull(location.DeletedAt);

            Assert.Equal(DeletionReason.NO_ACTIVE_DEPARTMENTS, location.DeletionReason);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_WhenDepartmentHasDescendants_ShouldUpdateSubtreePaths()
    {
        // arrange
        LocationId? locationId = await CreateLocation();

        Department? company =
            await CreateParentDepartment("company", "company", [locationId]);

        Department? backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        Department? team = await CreateChildDepartment("team", "team", backend, [locationId]);

        var command = new SoftDeleteDepartmentCommand(backend.Id.Value);

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(backend.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            Department? companyDepartment = await dbContext.Departments
                .SingleAsync(d => d.Id == company.Id);

            Assert.Equal("company", companyDepartment.Path.Value);
            Assert.True(companyDepartment.IsActive);
            Assert.Null(companyDepartment.DeletedAt);

            Department? archivedDepartment =
                await dbContext.Departments.SingleAsync(
                    l => l.Id == DepartmentId.Create(result.Value), CancellationToken.None);

            Assert.Equal("company.delete-backend", archivedDepartment.Path.Value);
            Assert.False(archivedDepartment.IsActive);
            Assert.NotNull(archivedDepartment.DeletedAt);

            Department? teamDepartment = await dbContext.Departments
                .SingleAsync(l => l.Id == team.Id, CancellationToken.None);

            Assert.Equal("company.delete-backend.team", teamDepartment.Path.Value);
            Assert.True(teamDepartment.IsActive);
            Assert.Null(teamDepartment.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_WhenDepartmentDoesNotExist_ShouldFail()
    {
        // arrange
        var notExistDepartmentId = Guid.NewGuid();

        var command = new SoftDeleteDepartmentCommand(notExistDepartmentId);

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(result.Error, e => e is { Code: "value.not.found", Type: ErrorType.NOT_FOUND });
    }

    [Fact]
    public async Task SoftDeleteDepartment_WhenDepartmentIsAlreadyInactive_ShouldFail()
    {
        // arrange
        LocationId? locationId = await CreateLocation();

        Department? company =
            await CreateParentDepartment("company", "company", [locationId]);

        var command = new SoftDeleteDepartmentCommand(company.Id.Value);

        // act 1
        Result<Guid, Errors> firstResult = await Execute(command);

        // assert 1
        Assert.True(firstResult.IsSuccess);

        DateTime deletedAt = default;

        await ExecuteInDb(async dbContext =>
        {
            Department? department = await dbContext.Departments
                .SingleAsync(l => l.Id == DepartmentId.Create(firstResult.Value));

            Assert.False(department.IsActive);
            Assert.NotNull(department.DeletedAt);

            Assert.Equal("delete-company", department.Path.Value);

            deletedAt = department.DeletedAt.Value;
        });

        // act 2
        Result<Guid, Errors> secondResult = await Execute(command);

        // assert 2
        Assert.True(secondResult.IsFailure);

        Assert.Contains(secondResult.Error, e => e is { Code: "value.not.found", Type: ErrorType.NOT_FOUND });

        await ExecuteInDb(async dbContext =>
        {
            Department? department = await dbContext.Departments
                .SingleAsync(l => l.Id == company.Id);

            Assert.False(department.IsActive);
            Assert.Equal(deletedAt, department.DeletedAt);

            Assert.Equal("delete-company", department.Path.Value);
        });
    }

    [Fact]
    public async Task SoftDeleteDepartment_WhenDepartmentIsActive_ShouldSucceed()
    {
        // arrange
        LocationId? locationId = await CreateLocation();

        Department? company = await CreateParentDepartment("company", "company", [locationId]);

        var command = new SoftDeleteDepartmentCommand(company.Id.Value);

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(company.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            Department? deleteDepartment = await dbContext.Departments
                .SingleAsync(p => p.Id == DepartmentId.Create(result.Value));

            Assert.Equal("delete-company", deleteDepartment.Path.Value);
            Assert.False(deleteDepartment.IsActive);
            Assert.NotNull(deleteDepartment.DeletedAt);
        });
    }

    private Task<Result<Guid, Errors>> Execute(SoftDeleteDepartmentCommand command) =>
        Execute<Result<Guid, Errors>, SoftDeleteDepartmentHandler>(handler => handler.Handle(command));
}