using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using SharedService.SharedKernel;

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
        string trimmed = value.Trim();

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return GeneralErrors.Required("location.timezone");
        }

        if (trimmed.Length > Constants.MAX_LOCATION_TIMEZONE_LENGTH)
        {
            return GeneralErrors.LengthOutOfRange("location.timezone", 0, Constants.MAX_LOCATION_TIMEZONE_LENGTH);
        }

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(trimmed, out _))
        {
            return GeneralErrors.Invalid("location.timezone");
        }

        return new Timezone(trimmed);
    }
}