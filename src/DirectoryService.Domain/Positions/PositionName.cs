using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Positions;

public sealed record PositionName
{
    private PositionName(string value) => Value = value;

    public string Value { get; }

    public static Result<PositionName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.Required("position name");
        }

        if (value.Length is > Constants.MAX_POSITION_NAME_LENGTH or < Constants.MIN_TEXT_LENGTH)
        {
            return GeneralErrors.LengthOutOfRange(
                "position name", Constants.MAX_POSITION_NAME_LENGTH, Constants.MIN_TEXT_LENGTH);
        }

        return new PositionName(value);
    }
}