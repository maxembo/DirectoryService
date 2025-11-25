using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Locations;

public sealed record LocationName
{
    private LocationName(string value) => Value = value;

    public string Value { get; }

    public static Result<LocationName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.Required("location name");
        }

        if (value.Length is > Constants.MAX_LOCATION_NAME_LENGTH or < Constants.MIN_TEXT_LENGTH)
        {
            return GeneralErrors.LengthOutOfRange(
                "location name", Constants.MAX_LOCATION_NAME_LENGTH, Constants.MIN_TEXT_LENGTH);
        }

        return new LocationName(value);
    }
}