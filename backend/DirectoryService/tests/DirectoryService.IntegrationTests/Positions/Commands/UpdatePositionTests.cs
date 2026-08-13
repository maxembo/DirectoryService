using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions.Commands.UpdatePositions;
using DirectoryService.Contracts.Positions.UpdatePositions;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Positions.Commands;

public class UpdatePositionTests(DirectoryTestWebFactory factory) : PositionBaseTests(factory)
{
    [Fact]
    public async Task UpdatePosition_WhenDescriptionExceedsMaxLength_ShouldFail()
    {
        // arrange
        string description = new('c', Constants.MAX_POSITION_DESCRIPTION_LENGTH + 1);
        const string name = "name test position";

        var department = await SeedActiveDepartment();

        var positionToUpdatedId = await CreatePosition(name, "description test position", [department.Id]);

        var command = CreateCommand(positionToUpdatedId.Value, name, description);

        // act
        var result = await Execute(command);

        // assert
        AssertSingleError(
            result,
            GeneralErrors
                .LengthOutOfRange("position.description", 0, Constants.MAX_POSITION_DESCRIPTION_LENGTH));

        await AssertPositionTableCounts(1, 1);
    }

    [Fact]
    public async Task UpdatePosition_WhenNameHasSurroundingSpaces_ShouldTrimName()
    {
        // arrange
        const string nameWithSurroundingSpaces = "   name test position  ";
        const string description = "description test position";

        var department = await SeedActiveDepartment();

        var positionToUpdatedId =
            await CreatePosition(nameWithSurroundingSpaces, description, [department.Id]);

        var command =
            CreateCommand(positionToUpdatedId.Value, nameWithSurroundingSpaces, description);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .SingleAsync(p => p.Id == PositionId.Create(result.Value));

            Assert.Equal(nameWithSurroundingSpaces.Trim(), position.Name.Value);
        });
    }

    [Theory]
    [InlineData("Test Position")]
    [InlineData("test position")]
    [InlineData("TEST POSITION")]
    [InlineData("  Test Position  ")]
    public async Task UpdatePosition_WhenNormalizedNameAlreadyExists_ShouldFail(string requestedName)
    {
        // arrange
        var department = await SeedActiveDepartment();

        await CreatePosition(
            "Test Position",
            "test description",
            [department.Id]);

        var positionToUpdateId = await CreatePosition(
            "Update Test Position",
            "Update test description",
            [department.Id]);

        var positionBeforeUpdate = await ExecuteInDb(dbContext =>
            dbContext.Positions
                .AsNoTracking()
                .SingleAsync(p => p.Id == positionToUpdateId));

        var command = CreateCommand(
            positionToUpdateId.Value,
            requestedName,
            "new description");

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .SingleAsync(p => p.Id == positionToUpdateId);

            Assert.Equal(
                positionBeforeUpdate.Name.Value,
                position.Name.Value);

            Assert.Equal(
                positionBeforeUpdate.Description?.Value,
                position.Description?.Value);

            Assert.Equal(
                positionBeforeUpdate.UpdatedAt,
                position.UpdatedAt);

            Assert.Equal(2, await dbContext.Positions.CountAsync());
            Assert.Equal(2, await dbContext.DepartmentPositions.CountAsync());
        });

        Assert.Contains(result.Error, e => e.Code == "value.already.exist");
    }

    [Fact]
    public async Task UpdatePosition_WhenConflictName_ShouldFail()
    {
        // arrange
        const string conflictingName = "conflict name test position";
        const string updateDescription = "description test position";

        var locationId = await CreateLocation("name test location");

        var department =
            await CreateParentDepartment("name test department", "department", [locationId]);

        var positionId =
            await CreatePosition("conflict name test position", "description test position", [department.Id]);

        var positionToUpdateId =
            await CreatePosition("name test position", "description test position", [department.Id]);

        var command = CreateCommand(positionToUpdateId.Value, conflictingName, updateDescription);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(
            result.Error,
            e => e is
                { Code: "value.already.exist", Type: ErrorType.CONFLICT });
    }

    [Fact]
    public async Task UpdatePosition_WhenNameIsEmpty_ShouldFail()
    {
        // arrange
        const string updateEmptyName = "";
        const string updateEmptyDescription = "";

        var locationId = await CreateLocation("name test location");

        var department =
            await CreateParentDepartment("name test department", "department", [locationId]);

        var positionId =
            await CreatePosition("name test position", "description test position", [department.Id]);

        var command = CreateCommand(positionId.Value, updateEmptyName, updateEmptyDescription);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(
            result.Error,
            e => e is
                { Code: "value.is.required", Type: ErrorType.VALIDATION, InvalidField: "position.name" });
    }

    [Fact]
    public async Task UpdatePosition_InactivePosition_ShouldFail()
    {
        // arrange
        const string newName = "update name test position";
        const string newDescription = "update description test position";

        var locationId = await CreateLocation("name test location");

        var department =
            await CreateParentDepartment("name test department", "department", [locationId]);

        var positionId =
            await CreatePosition("name test position", "description test position", [department.Id]);

        await MarkPositionAsDeleted(positionId);

        var command = CreateCommand(positionId.Value, newName, newDescription);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Error, e => e is { Code: "value.not.found", Type: ErrorType.NOT_FOUND });

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions.SingleAsync(
                p => p.Id == PositionId.Create(positionId.Value), CancellationToken.None);

            Assert.False(position.IsActive);
            Assert.NotNull(position.DeletedAt);
        });
    }

    [Fact]
    public async Task UpdatePosition_WhenPositionDoesNotExist_ShouldFail()
    {
        // arrange
        const string newName = "update name test position";
        const string newDescription = "update description test position";

        var locationId = await CreateLocation("name test location");

        var department =
            await CreateParentDepartment("name test department", "department", [locationId]);

        var positionId = Guid.NewGuid();

        var command = CreateCommand(positionId, newName, newDescription);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(result.Error, e => e is { Code: "value.not.found", Type: ErrorType.NOT_FOUND });
    }

    [Fact]
    public async Task UpdatePosition_WithValidData_ShouldSucceed()
    {
        // arrange
        const string originalName = "original position";
        const string originalDescription = "original description";
        const string newName = "updated position";
        const string newDescription = "updated description";

        var department = await SeedActiveDepartment();

        var positionId = await CreatePosition(
            originalName,
            originalDescription,
            [department.Id]);

        var positionBeforeUpdate = await ExecuteInDb(dbContext =>
            dbContext.Positions
                .AsNoTracking()
                .SingleAsync(p => p.Id == positionId));

        var command = CreateCommand(
            positionId.Value,
            newName,
            newDescription);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(positionId.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .Include(p => p.Departments)
                .SingleAsync(p => p.Id == positionId);

            Assert.Equal(newName, position.Name.Value);
            Assert.Equal(newDescription, position.Description?.Value);

            Assert.Equal(positionBeforeUpdate.CreatedAt, position.CreatedAt);
            Assert.True(position.UpdatedAt > positionBeforeUpdate.UpdatedAt);

            var relation = Assert.Single(position.Departments);

            Assert.Equal(department.Id, relation.DepartmentId);
            Assert.Equal(positionId, relation.PositionId);
        });
    }

    [Fact]
    public async Task UpdatePosition_WhenUsingOwnNormalizedName_ShouldSucceed()
    {
        // arrange
        const string name = "name test position";
        const string description = "description test position";

        var department = await SeedActiveDepartment();

        var positionToUpdatedId = await CreatePosition(name, description, [department.Id]);

        var command = CreateCommand(positionToUpdatedId.Value, name, description);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(positionToUpdatedId.Value, result.Value);
    }

    private static UpdatePositionCommand CreateCommand(Guid positionId, string name, string description) =>
        new(positionId, new UpdatePositionRequest(name, description));

    private Task<Result<Guid, Errors>> Execute(UpdatePositionCommand command) =>
        Execute<Result<Guid, Errors>, UpdatePositionHandler>(handle => handle.Handle(command));
}