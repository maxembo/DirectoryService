using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.Commands.MoveDepartments;

public class MoveDepartmentValidator : AbstractValidator<MoveDepartmentCommand>
{
    public MoveDepartmentValidator()
    {
        RuleFor(m => m.DepartmentId)
            .NotEmpty()
            .WithError(GeneralErrors.Required("DepartmentId"));

        RuleFor(m => m.Request.ParentId)
            .Must(parentId => parentId == null || parentId != Guid.Empty)
            .WithError(GeneralErrors.Required("department.parentId"))
            .Must((m, parentId) => m.DepartmentId != parentId)
            .WithError(GeneralErrors.Invalid("department.parentId"));
    }
}