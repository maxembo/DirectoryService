using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations.Commands.UpdateLocations;
using DirectoryService.Contracts.Locations.CreateLocations;
using DirectoryService.Contracts.Locations.UpdateLocations;
using DirectoryService.Domain.Locations;
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
        UpdateLocationCommand? command = CreateCommand(
            Guid.NewGuid(),
            CreateRequest("updated name"));

        // act
        Result<Guid, Errors> result = await Execute(command);

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

        await CreateLocation(conflictingName);

        LocationId? locationToUpdateId = await CreateLocation(
            existingName,
            "test city 2",
            "test country 2",
            "test street 2",
            "2 test house");

        UpdateLocationCommand? command = CreateCommand(
            locationToUpdateId.Value,
            CreateRequest(conflictingName));

        Result<Guid, Errors> result = await Execute(command);

        Assert.True(result.IsFailure);

        await ExecuteInDb(async dbContext =>
        {
            Location? location = await dbContext.Locations
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
            "existing Name",
            conflictingAddress.City,
            conflictingAddress.Country,
            conflictingAddress.Street,
            conflictingAddress.House);

        LocationId? locationToUpdateId = await CreateLocation(
            "location to update",
            existingAddress.City,
            existingAddress.Country,
            existingAddress.Street,
            existingAddress.House);

        UpdateLocationCommand? command = CreateCommand(
            locationToUpdateId.Value,
            CreateRequest(
                "updated name",
                conflictingAddress.City,
                conflictingAddress.Country,
                conflictingAddress.Street,
                conflictingAddress.House));

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        await ExecuteInDb(async dbContext =>
        {
            Location? location = await dbContext.Locations
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

        LocationId? locationToUpdateId = await CreateLocation(house: existingHouse);

        UpdateLocationCommand? command = CreateCommand(
            locationToUpdateId.Value,
            CreateRequest(house: "test house"));

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        await ExecuteInDb(async dbContext =>
        {
            Location? location = await dbContext.Locations
                .SingleAsync(l => l.Id == locationToUpdateId, CancellationToken.None);

            Assert.Equal(existingHouse, location.Address.House);
        });

        Assert.Contains(
            result.Error,
            e => e is
            {
                Code: "value.mismatch.regex", Type: ErrorType.VALIDATION, InvalidField: "location.address.house",
            });
    }

    [Fact]
    public async Task UpdateLocation_WhenTimezoneIsInvalid_ShouldFail()
    {
        // arrange
        const string existingTimezone = "Europe/Moscow";

        LocationId? locationToUpdateId = await CreateLocation("test name 1", timezone: existingTimezone);

        UpdateLocationCommand? command = CreateCommand(
            locationToUpdateId.Value,
            CreateRequest(timezone: "test timezone"));

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        await ExecuteInDb(async dbContext =>
        {
            Location? location = await dbContext.Locations.SingleAsync(
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
        LocationId? locationToUpdateId = await CreateLocation("test name 1");

        UpdateLocationCommand? command = CreateCommand(
            locationToUpdateId.Value,
            CreateRequest(
                name,
                city,
                country,
                street,
                house,
                timezone));

        // act
        Result<Guid, Errors> result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(result.Value, Guid.Empty);

        await ExecuteInDb(async dbContext =>
        {
            Location? location = await dbContext.Locations.SingleAsync(
                l => l.Id == locationToUpdateId, CancellationToken.None);

            Assert.Equal(name.Trim(), location.Name.Value);
            Assert.Equal(city.Trim(), location.Address.City);
            Assert.Equal(country.Trim(), location.Address.Country);
            Assert.Equal(street.Trim(), location.Address.Street);
            Assert.Equal(house.Trim(), location.Address.House);
            Assert.Equal(timezone.Trim(), location.Timezone.Value);
        });
    }

    private static UpdateLocationCommand CreateCommand(Guid id, UpdateLocationRequest? request = null) =>
        new(id, request ?? CreateRequest());

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
        Execute<Result<Guid, Errors>, UpdateLocationHandler>(handler => handler.Handle(
            command, CancellationToken.None));
}