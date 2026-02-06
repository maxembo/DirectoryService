using FluentValidation;
using Shared;
using Shared.Validation;

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