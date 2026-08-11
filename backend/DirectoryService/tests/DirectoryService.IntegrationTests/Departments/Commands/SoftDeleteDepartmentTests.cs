using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;
using DirectoryService.Domain;
using DirectoryService.Domain.Departments;
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
        var locationId = await CreateLocation();

        var firstCompany =
            await CreateParentDepartment("firstCompany", "first-company", [locationId]);

        var secondCompany =
            await CreateParentDepartment("secondCompany", "second-company", [locationId]);

        var positionId =
            await CreatePosition("name", null, [firstCompany.Id, secondCompany.Id]);

        var command = new SoftDeleteDepartmentCommand(firstCompany.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(firstCompany.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
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
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("firstCompany", "first-company", [locationId]);

        await CreateParentDepartment("secondCompany", "second-company", [locationId]);

        var command = new SoftDeleteDepartmentCommand(company.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(company.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
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
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var positionId = await CreatePosition("position", null, [company.Id]);

        var command = new SoftDeleteDepartmentCommand(company.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(company.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
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
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var command = new SoftDeleteDepartmentCommand(company.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(company.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
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
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var backend =
            await CreateChildDepartment("backend", "backend", company, [locationId]);

        var team = await CreateChildDepartment("team", "team", backend, [locationId]);

        var command = new SoftDeleteDepartmentCommand(backend.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(backend.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var companyDepartment = await dbContext.Departments
                .SingleAsync(d => d.Id == company.Id);

            Assert.Equal("company", companyDepartment.Path.Value);
            Assert.True(companyDepartment.IsActive);
            Assert.Null(companyDepartment.DeletedAt);

            var archivedDepartment =
                await dbContext.Departments.SingleAsync(
                    l => l.Id == DepartmentId.Create(result.Value), CancellationToken.None);

            Assert.Equal("company.delete-backend", archivedDepartment.Path.Value);
            Assert.False(archivedDepartment.IsActive);
            Assert.NotNull(archivedDepartment.DeletedAt);

            var teamDepartment = await dbContext.Departments
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
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(result.Error, e => e is { Code: "value.not.found", Type: ErrorType.NOT_FOUND });
    }

    [Fact]
    public async Task SoftDeleteDepartment_WhenDepartmentIsAlreadyInactive_ShouldFail()
    {
        // arrange
        var locationId = await CreateLocation();

        var company =
            await CreateParentDepartment("company", "company", [locationId]);

        var command = new SoftDeleteDepartmentCommand(company.Id.Value);

        // act 1
        var firstResult = await Execute(command);

        // assert 1
        Assert.True(firstResult.IsSuccess);

        DateTime deletedAt = default;

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .SingleAsync(l => l.Id == DepartmentId.Create(firstResult.Value));

            Assert.False(department.IsActive);
            Assert.NotNull(department.DeletedAt);

            Assert.Equal("delete-company", department.Path.Value);

            deletedAt = department.DeletedAt.Value;
        });

        // act 2
        var secondResult = await Execute(command);

        // assert 2
        Assert.True(secondResult.IsFailure);

        Assert.Contains(secondResult.Error, e => e is { Code: "value.not.found", Type: ErrorType.NOT_FOUND });

        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
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
        var locationId = await CreateLocation();

        var company = await CreateParentDepartment("company", "company", [locationId]);

        var command = new SoftDeleteDepartmentCommand(company.Id.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(company.Id.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var deleteDepartment = await dbContext.Departments
                .SingleAsync(p => p.Id == DepartmentId.Create(result.Value));

            Assert.Equal("delete-company", deleteDepartment.Path.Value);
            Assert.False(deleteDepartment.IsActive);
            Assert.NotNull(deleteDepartment.DeletedAt);
        });
    }

    private Task<Result<Guid, Errors>> Execute(SoftDeleteDepartmentCommand command) =>
        Execute<Result<Guid, Errors>, SoftDeleteDepartmentHandler>(handler => handler.Handle(command));
}