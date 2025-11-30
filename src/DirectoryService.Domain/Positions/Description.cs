using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Positions;

public record Description
{
    private Description(string? value) => Value = value;

    public string? Value { get; }

    public static Result<Description, Error> Create(string? value)
    {
        if (value is { Length: > Constants.MAX_POSITION_DESCRIPTION_LENGTH })
        {
            return GeneralErrors.LengthOutOfRange("description", Constants.MAX_POSITION_DESCRIPTION_LENGTH);
        }

        return new Description(value);
    }
}