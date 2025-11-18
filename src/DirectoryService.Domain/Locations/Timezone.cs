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
            return GeneralErrors.ValueIsRequired("timezone");
        }

        if (string.IsNullOrWhiteSpace(value) || value.Length > Constants.MAX_LOCATION_TIMEZONE_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid("timezone");
        }

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(value, out _))
        {
            return GeneralErrors.ValueIsInvalid("timezone");
        }

        return new Timezone(value);
    }
}