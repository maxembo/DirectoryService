using CSharpFunctionalExtensions;
using DirectoryService.Application.Locations.Commands.CreateLocations;
using DirectoryService.Contracts.Locations.CreateLocations;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Shared;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SharedService.SharedKernel;

namespace DirectoryService.IntegrationTests.Locations.Commands;

public class CreateLocationTests(DirectoryTestWebFactory factory) : DirectoryBaseTests(factory)
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateLocation_WhenNameIsEmptyAfterNormalization_ShouldFail(string name)
    {
        // arrange
        var command = CreateCommand(name: name);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(
            result.Error, e
                => e is { Code: "value.is.required", Type: ErrorType.VALIDATION, InvalidField: "location.name" });
    }

    [Theory]
    [InlineData(Constants.MIN_TEXT_LENGTH - 1)]
    [InlineData(Constants.MAX_LOCATION_NAME_LENGTH + 1)]
    public async Task CreateLocation_WhenNameLengthIsOutOfRange_ShouldFail(int count)
    {
        // arrange
        var command = CreateCommand(name: new string('l', count));

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(
            result.Error, e =>
                e is { Code: "value.length.out.of.range", Type: ErrorType.VALIDATION, InvalidField: "location.name" });
    }

    [Theory]
    [InlineData("TEST NAME")]
    [InlineData("test name")]
    [InlineData("  Test Name  ")]
    public async Task CreateLocation_WhenNormalizedNameAlreadyExists_ShouldFail(string name)
    {
        // arrange
        const string conflictingName = "test name";

        var locationId = await CreateLocation(name: conflictingName);

        var command = CreateCommand(name: name);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        await ExecuteInDb(
            async dbContext =>
            {
                var location = await dbContext.Locations
                    .SingleAsync(l => l.Id == locationId, CancellationToken.None);

                Assert.Equal(conflictingName, location.Name.Value);
            });

        Assert.Contains(
            result.Error, e
                => e is { Code: "value.already.exist", Type: ErrorType.CONFLICT, InvalidField: "name" });
    }

    [Theory]
    [InlineData("test city", "test country", "test street", "10 test house")]
    [InlineData("TEST CITY", "TEST COUNTRY", "TEST STREET", "10 TEST HOUSE")]
    [InlineData("  Test City  ", "  Test Country  ", "  Test Street  ", "  10 Test House  ")]
    public async Task CreateLocation_WhenNormalizedAddressAlreadyExists_ShouldFail(
        string city, string country, string street, string house)
    {
        // arrange
        const string existingName = "test name 1";
        var conflictingAddress = new AddressDto(
            "test city",
            "test country",
            "test street",
            "10 test house");

        var existingLocationId = await CreateLocation(
            name: existingName,
            city: conflictingAddress.City,
            country: conflictingAddress.Country,
            street: conflictingAddress.Street,
            house: conflictingAddress.House);

        var command = CreateCommand(
            name: "test name",
            city: city,
            country: country,
            street: street,
            house: house);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        await ExecuteInDb(
            async dbContext =>
            {
                var location = await dbContext.Locations
                    .SingleAsync(l => l.Id == existingLocationId, CancellationToken.None);

                Assert.Equal(existingName, location.Name.Value);
                Assert.Equal(conflictingAddress.City, location.Address.City);
                Assert.Equal(conflictingAddress.Country, location.Address.Country);
                Assert.Equal(conflictingAddress.Street, location.Address.Street);
                Assert.Equal(conflictingAddress.House, location.Address.House);
            });

        Assert.Contains(
            result.Error, e
                => e is { Code: "value.already.exist", Type: ErrorType.CONFLICT, InvalidField: "address" });
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateLocation_WhenAddressEmptyAfterNormalization_ShouldFail(string value)
    {
        // arrange
        var command = CreateCommand(city: value, country: value, street: value, house: value);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(
            result.Error, e
                => e is { Code: "value.is.required", InvalidField: "location.address.city" });
        Assert.Contains(
            result.Error, e
                => e is { Code: "value.is.required", InvalidField: "location.address.country" });
        Assert.Contains(
            result.Error, e
                => e is { Code: "value.is.required", InvalidField: "location.address.street" });
        Assert.Contains(
            result.Error, e
                => e is { Code: "value.is.required", InvalidField: "location.address.house" });
    }

    [Fact]
    public async Task CreateLocation_WhenAddressIsTooLong_ShouldFail()
    {
        // arrange
        string tooLongValue = new('l', Constants.MAX_LOCATION_ADDRESS_LENGTH + 1);

        var command = CreateCommand(
            city: tooLongValue,
            country: tooLongValue,
            street: tooLongValue,
            house: tooLongValue);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(
            result.Error, e =>
                e is { Code: "value.length.out.of.range", InvalidField: "location.address.city" });
        Assert.Contains(
            result.Error, e =>
                e is { Code: "value.length.out.of.range", InvalidField: "location.address.country" });
        Assert.Contains(
            result.Error, e =>
                e is { Code: "value.length.out.of.range", InvalidField: "location.address.street" });
        Assert.Contains(
            result.Error, e =>
                e is { Code: "value.length.out.of.range", InvalidField: "location.address.house" });
    }

    [Fact]
    public async Task CreateLocation_WhenAddressHouseIsInvalid_ShouldFail()
    {
        // arrange
        var command = CreateCommand(house: "test house");

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(
            result.Error, e => e is { Code: "value.mismatch.regex", InvalidField: "location.address.house" });
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateLocation_WhenTimezoneIsEmptyAfterNormalization_ShouldFail(string timezone)
    {
        // arrange
        var command = CreateCommand(timezone: timezone);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(result.Error, e => e is { Code: "value.is.required", InvalidField: "location.timezone" });
    }

    [Fact]
    public async Task CreateLocation_WhenTimezoneIsInvalid_ShouldFail()
    {
        // arrange
        var command = CreateCommand(timezone: "test timezone");

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(result.Error, e => e is { Code: "value.is.invalid", InvalidField: "location.timezone" });
    }

    [Fact]
    public async Task CreateLocation_WhenTimezoneIsTooLong_ShouldFail()
    {
        // arrange
        var command = CreateCommand(timezone: new string('c', Constants.MAX_LOCATION_TIMEZONE_LENGTH + 1));

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsFailure);

        Assert.Contains(
            result.Error, e
                => e is { Code: "value.length.out.of.range", InvalidField: "location.timezone" });
    }

    [Theory]
    [InlineData(
        "Test Name", "Test City", "Test Country",
        "Test Street", "10 test house", "Europe/Moscow")]
    [InlineData(
        "  Test Name ", "  Test City  ", "  Test Country  ",
        "  Test Street  ", "  10 test house  ", "  Europe/Moscow  ")]
    public async Task CreateLocation_WithValidData_ShouldSucceed(
        string name, string city, string country, string street, string house, string timezone)
    {
        // arrange
        var command = CreateCommand(
            name: name,
            city: city,
            country: country,
            street: street,
            house: house,
            timezone: timezone);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(result.Value, Guid.Empty);

        await ExecuteInDb(
            async dbContext =>
            {
                var location = await dbContext.Locations.SingleAsync(
                    l => l.Id == LocationId.Create(result.Value), CancellationToken.None);

                Assert.Equal(result.Value, location.Id.Value);

                Assert.Equal(name.Trim(), location.Name.Value);
                Assert.Equal(city.Trim(), location.Address.City);
                Assert.Equal(country.Trim(), location.Address.Country);
                Assert.Equal(street.Trim(), location.Address.Street);
                Assert.Equal(house.Trim(), location.Address.House);
                Assert.Equal(timezone.Trim(), location.Timezone.Value);
            });
    }

    [Theory]
    [InlineData(Constants.MIN_TEXT_LENGTH)]
    [InlineData(Constants.MAX_LOCATION_NAME_LENGTH)]
    public async Task CreateLocation_WhenNameLengthIsWithinRange_ShouldSucceed(int count)
    {
        // arrange
        string inRangeLengthName = new('l', count);

        var command = CreateCommand(name: inRangeLengthName);

        // act
        var result = await Execute(command);

        // assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await ExecuteInDb(
            async dbContext =>
            {
                var location = await dbContext.Locations.SingleAsync(
                    l => l.Id == LocationId.Create(result.Value), CancellationToken.None);

                Assert.Equal(result.Value, location.Id.Value);

                Assert.Equal(inRangeLengthName, location.Name.Value);
            });
    }

    private static CreateLocationCommand CreateCommand(
        string name = "Test Name 1",
        string city = "Test City 1",
        string country = "Test Country 1",
        string street = "Test Street 1",
        string house = "1 test house",
        string timezone = "Europe/Moscow")
    {
        return new CreateLocationCommand(
            new CreateLocationRequest(
                name,
                new AddressDto(city, country, street, house),
                timezone));
    }

    private Task<Result<Guid, Errors>> Execute(CreateLocationCommand command)
        => Execute<Result<Guid, Errors>, CreateLocationHandler>(
            handler
                => handler.Handle(command, CancellationToken.None));
}