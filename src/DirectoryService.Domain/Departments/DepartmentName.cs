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
            return GeneralErrors.ValueIsRequired("department name");
        }

        if (value.Length is > Constants.MAX_DEPARTMENT_NAME_LENGTH or < Constants.MIN_TEXT_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid("department name");
        }

        return new DepartmentName(value);
    }
}