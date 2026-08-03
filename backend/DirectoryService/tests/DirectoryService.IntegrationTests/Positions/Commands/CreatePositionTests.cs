using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions.Commands.CreatePositions;
using DirectoryService.Contracts.Positions.CreatePositions;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Positions.Commands;

public class CreatePositionTests(DirectoryTestWebFactory factory) : PositionBaseTests(factory)
{
    [Fact]
    public async Task CreatePosition_WhenOneDepartmentIsInactive_ShouldFail()
    {
        // arrange
        var activeDepartment = await SeedActiveDepartment("activeDepartment", "department-active");

        var inactiveDepartment = await SeedActiveDepartment("inactiveDepartment", "department-inactive");

        await MarkDepartmentAsDeleted(inactiveDepartment.Id);

        var command = CreateCommand(
            "name test position",
            "description test position",
            [activeDepartment.Id.Value, inactiveDepartment.Id.Value]);

        // act
        var result = await Execute(command);

        AssertSingleError(result, GeneralErrors.NotFound("position.departmentIds"));
    }

    [Theory]
    [InlineData("Test Position")]
    [InlineData("test position")]
    [InlineData("TEST POSITION")]
    [InlineData("  Test Position  ")]
    [InlineData("  TEST POSITION  ")]
    public async Task CreatePosition_WhenNormalizedNameAlreadyExists_ShouldFail(string requestedName)
    {
        // arrange
        const string existingName = "Test Position";

        var department = await SeedActiveDepartment();

        var existingPositionId = await CreatePosition(
            existingName,
            "existing description",
            [department.Id]);

        var command = CreateCommand(
            requestedName,
            "description test position",
            [department.Id.Value]);

        // act
        var result = await Execute(command);

        // assert
        AssertSingleError(
            result,
            GeneralErrors.AlreadyExist("name"));

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .SingleAsync(p => p.Id == existingPositionId);

            Assert.Equal(existingName, position.Name.Value);
            Assert.Equal(1, await dbContext.Positions.CountAsync());
            Assert.Equal(1, await dbContext.DepartmentPositions.CountAsync());
        });
    }

    [Fact]
    public async Task CreatePosition_WhenOneDepartmentDoesNotExist_ShouldFail()
    {
        // arrange
        var existingDepartment = await SeedActiveDepartment();
        var missingDepartmentId = Guid.NewGuid();

        var command = CreateCommand(
            "name test position",
            "description test position",
            [existingDepartment.Id.Value, missingDepartmentId]);

        // act
        var result = await Execute(command);

        // assert
        AssertSingleError(result, GeneralErrors.NotFound("department", missingDepartmentId));

        await AssertPositionTableCounts(0, 0);
    }

    [Fact]
    public async Task CreatePosition_WhenDepartmentIsInactive_ShouldFail()
    {
        // arrange
        var inactiveDepartment = await SeedActiveDepartment();

        await MarkDepartmentAsDeleted(inactiveDepartment.Id);

        var command = CreateCommand(
            "name test position",
            "description test position",
            [inactiveDepartment.Id.Value]);

        // act
        var result = await Execute(command);

        // assert
        AssertSingleError(result, GeneralErrors.NotFound("department", inactiveDepartment.Id.Value));

        await AssertPositionTableCounts(0, 0);
    }

    [Fact]
    public async Task CreatePosition_WhenDepartmentDoesNotExist_ShouldFail()
    {
        // arrange
        var missingDepartmentId = Guid.NewGuid();

        var command = CreateCommand(
            "name test position",
            "description test position",
            [missingDepartmentId]);

        // act
        var result = await Execute(command);

        // assert
        AssertSingleError(result, GeneralErrors.NotFound("position.departmentIds"));

        await AssertPositionTableCounts(0, 0);
    }

    [Fact]
    public async Task CreatePosition_WhenDepartmentIdsContainDuplicates_ShouldFail()
    {
        // arrange
        var department = await SeedActiveDepartment();

        var command = CreateCommand(
            "name test position", "description test position", [department.Id.Value, department.Id.Value]);

        // act
        var result = await Execute(command);

        // assert
        AssertSingleError(result, GeneralErrors.ArrayContainsDuplicates("position.departmentIds"));

        await AssertPositionTableCounts(0, 0);
    }

    [Fact]
    public async Task CreatePosition_WhenDepartmentIdsAreEmpty_ShouldFail()
    {
        // arrange
        var emptyDepartmentIds = Array.Empty<Guid>();

        var command = CreateCommand("name test position", "description test position", emptyDepartmentIds);

        // act
        var result = await Execute(command);

        // assert
        AssertSingleError(result, GeneralErrors.Required("position.departmentIds"));

        await AssertPositionTableCounts(0, 0);
    }

    [Fact]
    public async Task CreatePosition_WhenDescriptionExceedsMaxLength_ShouldFail()
    {
        // arrange
        string description = new string('c', Constants.MAX_POSITION_DESCRIPTION_LENGTH + 1);

        var department = await SeedActiveDepartment();

        var command = CreateCommand("test name position", description, [department.Id.Value]);

        // act
        var result = await Execute(command);

        // assert
        AssertSingleError(
            result,
            GeneralErrors
                .LengthOutOfRange("position.description", 0, Constants.MAX_POSITION_DESCRIPTION_LENGTH));

        await AssertPositionTableCounts(0, 0);
    }

    [Theory]
    [InlineData(Constants.MIN_TEXT_LENGTH - 1)]
    [InlineData(Constants.MAX_POSITION_NAME_LENGTH + 1)]
    public async Task CreatePosition_WhenNameLengthIsOutOfRange_ShouldFail(int nameLength)
    {
        // arrange
        string name = new('p', nameLength);
        const string description = "description test position";

        var department = await SeedActiveDepartment();

        var command = CreateCommand(name, description, [department.Id.Value]);

        // act
        var result = await Execute(command);

        // assert
        AssertSingleError(
            result,
            GeneralErrors.LengthOutOfRange(
                "position.name", Constants.MIN_TEXT_LENGTH, Constants.MAX_POSITION_NAME_LENGTH));

        await AssertPositionTableCounts(0, 0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task CreatePosition_WhenNameIsNullOrWhiteSpace_ShouldFail(string? name)
    {
        // arrange
        const string description = "description test position";

        var department = await SeedActiveDepartment();

        var command = CreateCommand(name!, description, [department.Id.Value]);

        // act
        var result = await Execute(command);

        // assert
        AssertSingleError(result, GeneralErrors.Required("position.name"));

        await AssertPositionTableCounts(0, 0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(Constants.MAX_POSITION_DESCRIPTION_LENGTH)]
    public async Task CreatePosition_WhenDescriptionLengthIsWithinRange_ShouldSucceed(int descriptionLength)
    {
        // arrange
        string description = new('d', descriptionLength);

        var department = await SeedActiveDepartment();

        var command = CreateCommand(
            "name test position",
            description,
            [department.Id.Value]);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .SingleAsync(p => p.Id == PositionId.Create(result.Value));

            Assert.Equal(description, position.Description?.Value);
        });
    }

    [Fact]
    public async Task CreatePosition_WithMultipleDepartments_ShouldSucceed()
    {
        // arrange
        var firstDepartment = await SeedActiveDepartment("first department", "first-department");

        var secondDepartment = await SeedActiveDepartment("second department", "second-department");

        var command = CreateCommand(
            "name test position", "description test position",
            [firstDepartment.Id.Value, secondDepartment.Id.Value]);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .Include(p => p.Departments)
                .SingleAsync(p => p.Id == PositionId.Create(result.Value), CancellationToken.None);

            Assert.Contains(firstDepartment.Id.Value, position.Departments.Select(dp => dp.DepartmentId.Value));
            Assert.Contains(secondDepartment.Id.Value, position.Departments.Select(dp => dp.DepartmentId.Value));

            Assert.Equal(2, position.Departments.Count);
        });
    }

    [Fact]
    public async Task CreatePosition_WhenNameHasSurroundingSpaces_ShouldSucceed()
    {
        // arrange
        const string nameWithSurroundingSpaces = "   name test position  ";
        const string description = "description test position";

        var department = await SeedActiveDepartment();

        var command = CreateCommand(nameWithSurroundingSpaces, description, [department.Id.Value]);

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

    [Fact]
    public async Task CreatePosition_DescriptionIsNull_ShouldSucceed()
    {
        // arrange
        const string? nullDescription = null;
        const string name = "name test position";

        var department = await SeedActiveDepartment();

        var command = CreateCommand(name, nullDescription, [department.Id.Value]);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .Include(p => p.Departments)
                .SingleAsync(p => p.Id == PositionId.Create(result.Value));

            Assert.Equal(result.Value, position.Id.Value);
            Assert.Equal(name, position.Name.Value);
            Assert.Null(position.Description);

            Assert.True(position.IsActive);
            Assert.Null(position.DeletedAt);
        });
    }

    [Theory]
    [InlineData(Constants.MIN_TEXT_LENGTH)]
    [InlineData(Constants.MAX_POSITION_NAME_LENGTH)]
    public async Task CreatePosition_WhenNameLengthIsWithinRange_ShouldSucceed(int nameLength)
    {
        // arrange
        string name = new('p', nameLength);
        const string description = "description test position";

        var department = await SeedActiveDepartment();

        var command = CreateCommand(name, description, [department.Id.Value]);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .SingleAsync(p => p.Id == PositionId.Create(result.Value));

            Assert.Equal(name, position.Name.Value);
        });
    }

    [Fact]
    public async Task CreatePosition_WithValidData_ShouldSucceed()
    {
        // arrange
        const string name = "name test position";
        const string description = "description test position";

        var department = await SeedActiveDepartment();

        var command = CreateCommand(name, description, [department.Id.Value]);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .Include(p => p.Departments)
                .SingleAsync(p => p.Id == PositionId.Create(result.Value));

            Assert.Equal(result.Value, position.Id.Value);
            Assert.Equal(name, position.Name.Value);
            Assert.Equal(description, position.Description?.Value);

            Assert.True(position.IsActive);
            Assert.Null(position.DeletedAt);

            var departmentPosition = Assert.Single(position.Departments);

            Assert.Equal(department.Id, departmentPosition.DepartmentId);
            Assert.Equal(position.Id, departmentPosition.PositionId);
        });
    }

    private static CreatePositionCommand CreateCommand(string name, string? description, Guid[] departmentIds)
        => new(new CreatePositionRequest(name, description, departmentIds));

    private Task<Result<Guid, Errors>> Execute(CreatePositionCommand command) =>
        Execute<Result<Guid, Errors>, CreatePositionHandler>(
            handler => handler.Handle(command));
}
