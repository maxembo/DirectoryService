using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Locations;

public sealed record Address
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
        if (string.IsNullOrWhiteSpace(city))
            return GeneralErrors.Required("Address city");

        if (city.Length > Constants.MAX_LOCATION_ADDRESS_LENGTH)
            return GeneralErrors.LengthOutOfRange("address city", Constants.MAX_LOCATION_ADDRESS_LENGTH);

        if (string.IsNullOrWhiteSpace(country))
            return GeneralErrors.Required("Address country");

        if (country.Length > Constants.MAX_LOCATION_ADDRESS_LENGTH)
            return GeneralErrors.LengthOutOfRange("address country", Constants.MAX_LOCATION_ADDRESS_LENGTH);

        if (string.IsNullOrWhiteSpace(street))
            return GeneralErrors.Required("Address street");

        if (street.Length > Constants.MAX_LOCATION_ADDRESS_LENGTH)
            return GeneralErrors.LengthOutOfRange("address street", Constants.MAX_LOCATION_ADDRESS_LENGTH);

        if (string.IsNullOrWhiteSpace(house))
            return GeneralErrors.Required("Address house");

        if (house.Length > Constants.MAX_LOCATION_ADDRESS_LENGTH)
            return GeneralErrors.LengthOutOfRange("address street", Constants.MAX_LOCATION_ADDRESS_LENGTH);

        return new Address(city, country, street, house);
    }
}