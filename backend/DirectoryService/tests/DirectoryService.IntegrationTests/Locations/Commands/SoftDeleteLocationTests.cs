using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations.Commands.SoftDeleteLocations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Locations.Commands;

public class SoftDeleteLocationTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task SoftDeleteLocation_WhenLocationDoesNotExist_ShouldFail()
    {
        // arrange
        var command = new SoftDeleteLocationCommand(Guid.NewGuid());

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(result.Error, e => e is { Code: "value.not.found", Type: ErrorType.NOT_FOUND });
    }

    [Fact]
    public async Task SoftDeleteLocation_WhenLocationAlreadyInactive_ShouldFail()
    {
        // arrange
        const string alreadyInactiveName = "soft delete name";

        var locationToDeleteId = await CreateLocation(name: alreadyInactiveName);

        await MarkLocationAsDeleted(locationToDeleteId);

        var command = new SoftDeleteLocationCommand(locationToDeleteId.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        await ExecuteInDb(
            async dbContext =>
            {
                var location = await dbContext.Locations
                    .SingleAsync(l => l.Id == locationToDeleteId, CancellationToken.None);

                Assert.False(location.IsActive);
                Assert.NotNull(location.DeletedAt);

                Assert.Equal(alreadyInactiveName, location.Name.Value);
            });

        Assert.Contains(result.Error, e => e is { Code: "value.not.found", Type: ErrorType.NOT_FOUND });
    }

    [Fact]
    public async Task SoftDeleteLocation_WithValidId_ShouldSucceed()
    {
        // arrange
        const string name = "soft delete name";

        var locationToDeleteId = await CreateLocation(name: name);

        var command = new SoftDeleteLocationCommand(locationToDeleteId.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(locationToDeleteId.Value, result.Value);

        await ExecuteInDb(
            async dbContext =>
            {
                var location = await dbContext.Locations
                    .SingleAsync(l => l.Id == locationToDeleteId, CancellationToken.None);

                Assert.False(location.IsActive);
                Assert.NotNull(location.DeletedAt);

                Assert.Equal(name, location.Name.Value);
            });
    }

    private Task<Result<Guid, Errors>> Execute(SoftDeleteLocationCommand command)
        =>
            Execute<Result<Guid, Errors>, SoftDeleteLocationHandler>(
                handler
                    => handler.Handle(command, CancellationToken.None));
}