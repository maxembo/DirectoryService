using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Positions;

public record PositionName
{
    private PositionName(string value) => Value = value;

    public string Value { get; }

    public static Result<PositionName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > Constants.MAX_POSITION_NAME_LENGTH || value.Length < Constants.MIN_TEXT_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid("position name");
        }

        var positionName = new PositionName(value);

        return positionName;
    }
}