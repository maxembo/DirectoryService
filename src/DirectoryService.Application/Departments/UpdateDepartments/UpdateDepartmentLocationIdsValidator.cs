using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments.UpdateDepartment;
using FluentValidation;
using Shared;

namespace DirectoryService.Application.Departments.UpdateDepartments;

public class UpdateDepartmentLocationIdsValidator : AbstractValidator<UpdateDepartmentLocationIdsRequest>
{
    public UpdateDepartmentLocationIdsValidator()
    {
        RuleFor(u => u.LocationIds)
            .NotEmpty()
            .WithError(GeneralErrors.Required("locationIds"))
            .Must(locationIds => locationIds.Distinct().Count() == locationIds.Length)
            .WithError(GeneralErrors.ArrayContainsDuplicates("department.locationIds"));
    }
}