using CSharpFunctionalExtensions;
using DirectoryService.Application.Positions.Commands.SoftDeletePositions;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Positions.Commands;

public class SoftDeletePositionTests(DirectoryTestWebFactory factory) : PositionBaseTests(factory)
{
    [Fact]
    public async Task SoftDeletePosition_WhenPositionDoesNotExist_ShouldFail()
    {
        // arrange
        var notExistPositionId = Guid.NewGuid();

        var command = new SoftDeletePositionCommand(notExistPositionId);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(result.Error, e => e is { Code: "value.not.found", Type: ErrorType.NOT_FOUND });
    }

    [Fact]
    public async Task SoftDeletePosition_WhenPositionAlreadyInactive_ShouldFail()
    {
        // arrange
        const string alreadyInactiveName = "soft delete name name";

        var department = await SeedActiveDepartment();

        var positionToDeleteId = await CreatePosition(
            alreadyInactiveName, "description test position", [department.Id]);

        await MarkPositionAsDeleted(positionToDeleteId);

        var command = new SoftDeletePositionCommand(positionToDeleteId.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .SingleAsync(l => l.Id == positionToDeleteId);

            Assert.False(position.IsActive);
            Assert.NotNull(position.DeletedAt);

            Assert.Equal(alreadyInactiveName, position.Name.Value);
        });

        Assert.Contains(result.Error, e => e is { Code: "value.not.found", Type: ErrorType.NOT_FOUND });
    }

    [Fact]
    public async Task SoftDeletePosition_WithValidId_ShouldSucceed()
    {
        // arrange
        const string name = "soft delete name position";

        var department = await SeedActiveDepartment();

        var positionToDeleteId =
            await CreatePosition(name, "description test position", [department.Id]);

        var command = new SoftDeletePositionCommand(positionToDeleteId.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(positionToDeleteId.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .SingleAsync(p => p.Id == positionToDeleteId);

            Assert.False(position.IsActive);
            Assert.NotNull(position.DeletedAt);

            Assert.Equal(name, position.Name.Value);
        });
    }

    private Task<Result<Guid, Errors>> Execute(SoftDeletePositionCommand command) =>
        Execute<Result<Guid, Errors>, SoftDeletePositionHandler>(
            handler => handler.Handle(command));
}