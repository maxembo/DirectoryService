using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;

namespace DirectoryService.Domain.Departments;

public record DepartmentName
{
    private DepartmentName(string value) => Value = value;

    public string Value { get; }

    public static Result<DepartmentName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > Constants.MAX_DEPARTMENT_NAME_LENGTH ||
            value.Length < Constants.MIN_TEXT_LENGTH)
        {
            Result.Failure<DepartmentName>(
                "DepartmentName must be at least 3 characters long and no more than 150 characters long.");
        }

        var departmentName = new DepartmentName(value);

        return Result.Success(departmentName);
    }
}