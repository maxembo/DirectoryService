using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.Commands.SoftDeleteDepartments;

public class SoftDeleteDepartmentValidator : AbstractValidator<SoftDeleteDepartmentCommand>
{
    public SoftDeleteDepartmentValidator()
    {
        RuleFor(s => s.DepartmentId)
            .NotNull()
            .WithError(GeneralErrors.Required("departmentId"))
            .Must(departmentId => departmentId != Guid.Empty)
            .WithError(GeneralErrors.Invalid("departmentId"));
    }
}