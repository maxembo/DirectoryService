using DirectoryService.Contracts.Departments.UpdateDepartments;
using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.Commands.UpdateDepartments;

public class UpdateDepartmentLocationIdsValidator : AbstractValidator<UpdateDepartmentLocationIdsRequest>
{
    public UpdateDepartmentLocationIdsValidator()
    {
        RuleFor(request => request.LocationIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithError(GeneralErrors.Required("department.locationIds"))
            .Must(locationIds => locationIds.Distinct().Count() == locationIds.Length)
            .WithError(GeneralErrors.ArrayContainsDuplicates("department.locationIds"));
    }
}