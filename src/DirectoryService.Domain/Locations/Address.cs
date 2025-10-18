using CSharpFunctionalExtensions;

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
        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<Address>("City cannot be empty.");

        if (string.IsNullOrWhiteSpace(country))
            return Result.Failure<Address>("Country cannot be empty.");

        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure<Address>("Street cannot be empty.");

        var address = new Address(city, country, street, house);

        return Result.Success(address);
    }
}