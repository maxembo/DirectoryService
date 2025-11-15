using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using Shared;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    private DepartmentName(string value) => Value = value;

    public string Value { get; }

    public static Result<DepartmentName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > Constants.MAX_DEPARTMENT_NAME_LENGTH ||
            value.Length < Constants.MIN_TEXT_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid("department name");
        }

        var departmentName = new DepartmentName(value);

        return departmentName;
    }
}