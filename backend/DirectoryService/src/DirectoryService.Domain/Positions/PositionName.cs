using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.Positions;

public sealed record PositionName
{
    private PositionName(string value) => Value = value;

    public string Value { get; }

    public static Result<PositionName, Error> Create(string value)
    {
        string? normalizedValue = value?.Trim();

        if (string.IsNullOrEmpty(normalizedValue))
        {
            return GeneralErrors.Required("position.name");
        }

        if (normalizedValue.Length is > Constants.MAX_POSITION_NAME_LENGTH or < Constants.MIN_TEXT_LENGTH)
        {
            return GeneralErrors.LengthOutOfRange(
                "position.name", Constants.MIN_TEXT_LENGTH, Constants.MAX_POSITION_NAME_LENGTH);
        }

        return new PositionName(normalizedValue);
    }
}