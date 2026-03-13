using DirectoryService.Contracts.Departments.UpdateDepartment;
using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartments;

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