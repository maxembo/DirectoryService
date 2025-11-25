using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Departments;

public sealed record DepartmentName
{
    private DepartmentName(string value) => Value = value;

    public string Value { get; }

    public static Result<DepartmentName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.Required("department name");
        }

        if (value.Length is > Constants.MAX_DEPARTMENT_NAME_LENGTH or < Constants.MIN_TEXT_LENGTH)
        {
            return GeneralErrors.LengthOutOfRange(
                "department name", Constants.MAX_DEPARTMENT_NAME_LENGTH, Constants.MIN_TEXT_LENGTH);
        }

        return new DepartmentName(value);
    }
}