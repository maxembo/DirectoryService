using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Locations;

public record Address
{
    private Address(string city, string country, string street, string house)
    {
        City = city;
        Country = country;
        Street = street;
        House = house;
    }

    public string City { get; }

    public string Country { get; }

    public string Street { get; }

    public string House { get; }

    public static Result<Address> Create(string city, string country, string street, string house)
    {
        if (string.IsNullOrWhiteSpace(city) || city.Length > Constants.MAX_LOCATION_ADDRESS_LENGTH)
            return Result.Failure<Address>($"City cannot be empty or more than {Constants.MAX_LOCATION_ADDRESS_LENGTH} characters.");

        if (string.IsNullOrWhiteSpace(country) || country.Length > Constants.MAX_LOCATION_ADDRESS_LENGTH)
            return Result.Failure<Address>($"Country cannot be empty or more than {Constants.MAX_LOCATION_ADDRESS_LENGTH} characters.");

        if (string.IsNullOrWhiteSpace(street) || street.Length > Constants.MAX_LOCATION_ADDRESS_LENGTH)
            return Result.Failure<Address>($"Street cannot be empty or more than {Constants.MAX_LOCATION_ADDRESS_LENGTH} characters.");

        if (string.IsNullOrWhiteSpace(house) || house.Length > Constants.MAX_LOCATION_ADDRESS_LENGTH)
            return Result.Failure<Address>($"House cannot be empty or more than {Constants.MAX_LOCATION_ADDRESS_LENGTH} characters.");

        var address = new Address(city, country, street, house);

        return Result.Success(address);
    }
}