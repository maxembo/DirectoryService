using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Locations;

public sealed record Timezone
{
    private Timezone(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Timezone, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.Required("timezone");
        }

        if (value.Length > Constants.MAX_LOCATION_TIMEZONE_LENGTH)
        {
            return GeneralErrors.LengthOutOfRange("timezone", Constants.MAX_LOCATION_TIMEZONE_LENGTH);
        }

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(value, out _))
        {
            return GeneralErrors.Invalid("timezone");
        }

        return new Timezone(value);
    }
}