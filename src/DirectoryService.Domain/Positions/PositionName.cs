using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Positions;

public record PositionName
{
    private PositionName(string value) => Value = value;

    public string Value { get; }

    public static Result<PositionName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > Constants.MAX_POSITION_NAME_LENGTH || value.Length < Constants.MIN_TEXT_LENGTH)
        {
            Result.Failure<DepartmentName>(
                "PositionName must be at least 3 characters long and no more than 100 characters long.");
        }

        var positionName = new PositionName(value);

        return Result.Success(positionName);
    }
}