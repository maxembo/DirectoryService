using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Locations;

public record Timezone
{
    private Timezone(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Timezone> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > Constants.MAX_LOCATION_TIMEZONE_LENGTH)
            return Result.Failure<Timezone>("Timezone cannot be empty.");

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(value, out _))
            return Result.Failure<Timezone>("Timezone cannot be found.");

        var timezone = new Timezone(value);

        return Result.Success(timezone);
    }
}