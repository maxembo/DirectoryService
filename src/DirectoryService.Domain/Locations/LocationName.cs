using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Locations;

public record LocationName
{
    private LocationName(string value) => Value = value;

    public string Value { get; }

    public static Result<LocationName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > Constants.MAX_LOCATION_NAME_LENGTH || value.Length < Constants.MIN_TEXT_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid("location name");
        }

        var locationName = new LocationName(value);

        return locationName;
    }
}