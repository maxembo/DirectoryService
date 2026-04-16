using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.Locations;

public sealed partial record Address
{
    private Address(string city, string country, string street, string house)
    {
        City = city;
        Country = country;
        Street = street;
        House = house;
    }

    private static readonly Regex _houseRegex =
        HouseRegex();

    public string City { get; }

    public string Country { get; }

    public string Street { get; }

    public string House { get; }

    public static Result<Address, Errors> Create(string city, string country, string street, string house)
    {
        var errors = new List<Error>();

        var cityResult = ValidateAndTrim(city, "location.address.city");
        if (cityResult.IsFailure)
        {
            errors.Add(cityResult.Error);
        }

        var countryResult = ValidateAndTrim(country, "location.address.country");
        if (countryResult.IsFailure)
        {
            errors.Add(countryResult.Error);
        }

        var streetResult = ValidateAndTrim(street, "location.address.street");
        if (streetResult.IsFailure)
        {
            errors.Add(streetResult.Error);
        }

        var houseResult = ValidateHouse(house);
        if (houseResult.IsFailure)
        {
            errors.Add(houseResult.Error);
        }

        if (errors.Count != 0)
        {
            return new Errors(errors);
        }

        return new Address(cityResult.Value, countryResult.Value, streetResult.Value, houseResult.Value);
    }

    private static Result<string, Error> ValidateAndTrim(string? value, string fieldName)
    {
        string trimmed = value?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmed))
            return GeneralErrors.Required(fieldName);

        if (trimmed.Length > Constants.MAX_LOCATION_ADDRESS_LENGTH)
            return GeneralErrors.LengthOutOfRange(fieldName, 0, Constants.MAX_LOCATION_ADDRESS_LENGTH);

        return trimmed;
    }

    private static Result<string, Error> ValidateHouse(string? value)
    {
        const string fieldName = "location.address.house";

        var baseValidationResult = ValidateAndTrim(value, fieldName);
        if (baseValidationResult.IsFailure)
            return baseValidationResult.Error;

        if (!_houseRegex.IsMatch(baseValidationResult.Value))
            return GeneralErrors.MismatchRegex(fieldName);

        return baseValidationResult.Value;
    }

    [GeneratedRegex(@"^[0-9]+([a-zA-Zа-яА-Я\/\-\s]*[0-9a-zA-Zа-яА-Я]*)?$", RegexOptions.Compiled)]
    private static partial Regex HouseRegex();
}