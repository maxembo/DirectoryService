using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using SharedService.SharedKernel;

namespace DirectoryService.Domain.Departments;

public sealed record DepartmentName
{
    private DepartmentName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<DepartmentName, Error> Create(string value)
    {
        string valueTrim = value.Trim();

        if (string.IsNullOrWhiteSpace(valueTrim))
        {
            return GeneralErrors.Required("department.name");
        }

        if (valueTrim.Length is > Constants.MAX_DEPARTMENT_NAME_LENGTH or < Constants.MIN_TEXT_LENGTH)
        {
            return GeneralErrors.LengthOutOfRange(
                "department.name", Constants.MIN_TEXT_LENGTH, Constants.MAX_DEPARTMENT_NAME_LENGTH);
        }

        return new DepartmentName(valueTrim);
    }
}