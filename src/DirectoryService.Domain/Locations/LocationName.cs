using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Locations;

public record LocationName
{
    private LocationName(string value) => Value = value;

    public string Value { get; }

    public static Result<LocationName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > Constants.MAX_LOCATION_NAME_LENGTH || value.Length < Constants.MIN_TEXT_LENGTH)
        {
            Result.Failure<DepartmentName>("LocationName must be at least 3 characters long and no more than 120 characters long.");
        }

        var locationName = new LocationName(value);

        return Result.Success(locationName);
    }
}