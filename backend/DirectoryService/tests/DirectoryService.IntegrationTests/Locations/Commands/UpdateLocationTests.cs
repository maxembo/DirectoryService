using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations.Commands.UpdateLocations;
using DirectoryService.Contracts.Locations.CreateLocations;
using DirectoryService.Contracts.Locations.UpdateLocations;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Locations.Commands;

public class UpdateLocationTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Fact]
    public async Task UpdateLocation_WhenLocationDoesNotExist_ShouldFail()
    {
        // arrange
        var command = CreateCommand(
            Guid.NewGuid(),
            CreateRequest(name: "updated name"));

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(result.Error, e => e is { Code: "value.not.found", Type: ErrorType.NOT_FOUND });
    }

    [Fact]
    public async Task UpdateLocation_WhenNameAlreadyExists_ShouldFail()
    {
        // arrange
        const string conflictingName = "update name";
        const string existingName = "test name 2";

        await CreateLocation(name: conflictingName);

        var locationToUpdateId = await CreateLocation(
            name: existingName,
            city: "test city 2",
            country: "test country 2",
            street: "test street 2",
            house: "2 test house");

        var command = CreateCommand(
            locationToUpdateId.Value,
            CreateRequest(name: conflictingName));

        var result = await Execute(command);

        Assert.True(result.IsFailure);

        await ExecuteInDb(
            async dbContext =>
            {
                var location = await dbContext.Locations
                    .SingleAsync(l => l.Id == locationToUpdateId, CancellationToken.None);

                Assert.Equal(existingName, location.Name.Value);
            });

        Assert.Contains(
            result.Error, e => e is { Code: "value.already.exist", Type: ErrorType.CONFLICT, InvalidField: "name" });
    }

    [Fact]
    public async Task UpdateLocation_WhenAddressAlreadyExists_ShouldFail()
    {
        // arrange
        var conflictingAddress = new AddressDto(
            "test city",
            "test country",
            "test street",
            "1 test house");

        var existingAddress = new AddressDto(
            "another city",
            "another country",
            "another street",
            "1 another house");

        await CreateLocation(
            name: "existing Name",
            city: conflictingAddress.City,
            country: conflictingAddress.Country,
            street: conflictingAddress.Street,
            house: conflictingAddress.House);

        var locationToUpdateId = await CreateLocation(
            name: "location to update",
            city: existingAddress.City,
            country: existingAddress.Country,
            street: existingAddress.Street,
            house: existingAddress.House);

        var command = CreateCommand(
            locationToUpdateId.Value,
            CreateRequest(
                name: "updated name",
                city: conflictingAddress.City,
                country: conflictingAddress.Country,
                street: conflictingAddress.Street,
                house: conflictingAddress.House));

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        await ExecuteInDb(
            async dbContext =>
            {
                var location = await dbContext.Locations
                    .SingleAsync(l => l.Id == locationToUpdateId, CancellationToken.None);

                Assert.Equal(existingAddress.City, location.Address.City);
                Assert.Equal(existingAddress.Country, location.Address.Country);
                Assert.Equal(existingAddress.Street, location.Address.Street);
                Assert.Equal(existingAddress.House, location.Address.House);
            });

        Assert.Contains(
            result.Error,
            e => e is { Code: "value.already.exist", Type: ErrorType.CONFLICT, InvalidField: "address" });
    }

    [Fact]
    public async Task UpdateLocation_WhenAddressHouseIsInvalid_ShouldFail()
    {
        // arrange
        const string existingHouse = "10 test house";

        var locationToUpdateId = await CreateLocation(house: existingHouse);

        var command = CreateCommand(
            locationToUpdateId.Value,
            CreateRequest(house: "test house"));

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        await ExecuteInDb(
            async dbContext =>
            {
                var location = await dbContext.Locations
                    .SingleAsync(l => l.Id == locationToUpdateId, CancellationToken.None);

                Assert.Equal(existingHouse, location.Address.House);
            });

        Assert.Contains(
            result.Error,
            e => e is
            {
                Code: "value.mismatch.regex", Type: ErrorType.VALIDATION, InvalidField: "location.address.house"
            });
    }

    [Fact]
    public async Task UpdateLocation_WhenTimezoneIsInvalid_ShouldFail()
    {
        // arrange
        const string existingTimezone = "Europe/Moscow";

        var locationToUpdateId = await CreateLocation(name: "test name 1", timezone: existingTimezone);

        var command = CreateCommand(
            locationToUpdateId.Value,
            CreateRequest(timezone: "test timezone"));

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        await ExecuteInDb(
            async dbContext =>
            {
                var location = await dbContext.Locations.SingleAsync(
                    l => l.Id == locationToUpdateId, CancellationToken.None);

                Assert.Equal(existingTimezone, location.Timezone.Value);
            });

        Assert.Contains(
            result.Error,
            e => e is { Code: "value.is.invalid", Type: ErrorType.VALIDATION, InvalidField: "location.timezone" });
    }

    [Theory]
    [InlineData(
        "Test Name 2", "Test City 2", "Test Country 2",
        "Test Street 2", "2 test house", "Europe/London")]
    [InlineData(
        "  Test Name 2 ", "  Test City 2 ", "  Test Country 2  ",
        "  Test Street 2  ", "  2 test house  ", "  Europe/London  ")]
    public async Task UpdateLocation_WithValidData_ShouldSucceed(
        string name, string city, string country, string street, string house, string timezone)
    {
        // arrange
        var locationToUpdateId = await CreateLocation(name: "test name 1");

        var command = CreateCommand(
            locationToUpdateId.Value,
            CreateRequest(
                name: name,
                city: city,
                country: country,
                street: street,
                house: house,
                timezone: timezone));

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(result.Value, Guid.Empty);

        await ExecuteInDb(
            async dbContext =>
            {
                var location = await dbContext.Locations.SingleAsync(
                    l => l.Id == locationToUpdateId, CancellationToken.None);

                Assert.Equal(name.Trim(), location.Name.Value);
                Assert.Equal(city.Trim(), location.Address.City);
                Assert.Equal(country.Trim(), location.Address.Country);
                Assert.Equal(street.Trim(), location.Address.Street);
                Assert.Equal(house.Trim(), location.Address.House);
                Assert.Equal(timezone.Trim(), location.Timezone.Value);
            });
    }

    private static UpdateLocationCommand CreateCommand(Guid id, UpdateLocationRequest? request = null)
    {
        return new UpdateLocationCommand(id, request ?? CreateRequest());
    }

    private static UpdateLocationRequest CreateRequest(
        string name = "Test Name 1",
        string city = "Test City 1",
        string country = "Test Country 1",
        string street = "Test Street 1",
        string house = "1 test house",
        string timezone = "Europe/Moscow")
    {
        return new UpdateLocationRequest(
            name,
            new AddressDto(city, country, street, house),
            timezone);
    }

    private Task<Result<Guid, Errors>> Execute(UpdateLocationCommand command) =>
        Execute<Result<Guid, Errors>, UpdateLocationHandler>(
            handler => handler.Handle(command, CancellationToken.None));
}