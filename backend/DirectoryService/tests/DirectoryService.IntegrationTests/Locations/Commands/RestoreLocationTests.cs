using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations.Commands.RestoreLocations;
using DirectoryService.Domain.Locations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Locations.Commands;

public class RestoreLocationTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task RestoreLocation_WhenLocationIsAlreadyActive_ShouldSucceedWithoutChanges()
    {
        // arrange
        var activeLocationId = await CreateLocation(name: "active location");

        var updatedAtBeforeRestore = await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .SingleAsync(l => l.Id == activeLocationId);

            return location.UpdatedAt;
        });

        var command = new RestoreLocationCommand(activeLocationId.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(activeLocationId.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .SingleAsync(l => l.Id == LocationId.Create(result.Value));

            Assert.True(location.IsActive);
            Assert.Null(location.DeletedAt);
            Assert.Equal(updatedAtBeforeRestore, location.UpdatedAt);
        });
    }

    [Fact]
    public async Task RestoreLocation_WhenLocationDoesNotExist_ShouldFail()
    {
        // arrange
        var notExistingLocationId = Guid.NewGuid();

        var command = new RestoreLocationCommand(notExistingLocationId);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        AssertSingleError(result, GeneralErrors.NotFound("location", notExistingLocationId));
    }

    [Fact]
    public async Task RestoreLocation_WhenLocationIsInactive_ShouldSucceed()
    {
        // arrange
        const string name = "restore location";

        var locationId = await CreateLocation(name: name);

        await MarkLocationAsDeleted(locationId);

        var updatedAtBeforeRestore = await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .SingleAsync(l => l.Id == locationId);

            Assert.False(location.IsActive);
            Assert.NotNull(location.DeletedAt);

            return location.UpdatedAt;
        });

        var command = new RestoreLocationCommand(locationId.Value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(locationId.Value, result.Value);

        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .SingleAsync(l => l.Id == LocationId.Create(result.Value), CancellationToken.None);

            Assert.True(location.IsActive);
            Assert.Null(location.DeletedAt);
            Assert.True(location.UpdatedAt > updatedAtBeforeRestore);
            Assert.Equal(name, location.Name.Value);
        });
    }

    private static void AssertSingleError(Result<Guid, Errors> result, Error expectedError)
    {
        Assert.True(result.IsFailure);

        var actualError = Assert.Single(result.Error);

        Assert.Equal(expectedError.Code, actualError.Code);
        Assert.Equal(expectedError.Type, actualError.Type);
        Assert.Equal(expectedError.InvalidField, actualError.InvalidField);
    }

    private Task<Result<Guid, Errors>> Execute(RestoreLocationCommand command)
        =>
            Execute<Result<Guid, Errors>, RestoreLocationHandler>(handler => handler.Handle(
                command, CancellationToken.None));
}