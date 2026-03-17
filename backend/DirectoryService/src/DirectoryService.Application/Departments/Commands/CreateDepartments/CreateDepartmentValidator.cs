using DirectoryService.Contracts.Departments.CreateDepartment;
using DirectoryService.Domain.Departments;
using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.Commands.CreateDepartments;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentValidator()
    {
        RuleFor(c => c.Name)
            .MustBeValueObject(DepartmentName.Create);

        RuleFor(c => c.Identifier)
            .MustBeValueObject(Identifier.Create);

        RuleFor(c => c.ParentId)
            .Must(parentId => parentId != Guid.Empty || parentId == null)
            .WithError(GeneralErrors.Required("department parentId"));

        RuleFor(c => c.LocationsIds)
            .Must(locationIds => locationIds.Distinct().Count() == locationIds.Length)
            .WithError(GeneralErrors.ArrayContainsDuplicates("department.locationIds"))
            .NotEmpty()
            .WithError(GeneralErrors.Required("locationsIds"));
    }
}