using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

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

    public static Result<Address, Error> Create(string city, string country, string street, string house)
    {
        if (string.IsNullOrWhiteSpace(city) || city.Length > Constants.MAX_LOCATION_ADDRESS_LENGTH)
            return GeneralErrors.ValueIsInvalid("address city");

        if (string.IsNullOrWhiteSpace(country) || country.Length > Constants.MAX_LOCATION_ADDRESS_LENGTH)
            return GeneralErrors.ValueIsInvalid("address country");

        if (string.IsNullOrWhiteSpace(street) || street.Length > Constants.MAX_LOCATION_ADDRESS_LENGTH)
            return GeneralErrors.ValueIsInvalid("address street");

        if (string.IsNullOrWhiteSpace(house) || house.Length > Constants.MAX_LOCATION_ADDRESS_LENGTH)
            return GeneralErrors.ValueIsInvalid("address house");

        var address = new Address(city, country, street, house);

        return address;
    }
}