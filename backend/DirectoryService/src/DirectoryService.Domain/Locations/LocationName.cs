using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.Locations;

public sealed record LocationName
{
    private LocationName(string value) => Value = value;

    public string Value { get; }

    public static Result<LocationName, Error> Create(string value)
    {
        string valueTrim = value.Trim();

        if (string.IsNullOrWhiteSpace(valueTrim))
        {
            return GeneralErrors.Required("location.name");
        }

        if (valueTrim.Length is > Constants.MAX_LOCATION_NAME_LENGTH or < Constants.MIN_TEXT_LENGTH)
        {
            return GeneralErrors.LengthOutOfRange(
                "location.name", Constants.MIN_TEXT_LENGTH, Constants.MAX_LOCATION_NAME_LENGTH);
        }

        return new LocationName(valueTrim);
    }
}