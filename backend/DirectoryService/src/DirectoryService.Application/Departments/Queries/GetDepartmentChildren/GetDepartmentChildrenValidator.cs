using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentChildren;

public class GetDepartmentChildrenValidator : AbstractValidator<GetDepartmentChildrenQuery>
{
    public GetDepartmentChildrenValidator()
    {
        RuleFor(g => g.ParentId)
            .NotEmpty()
            .WithError(GeneralErrors.Required("parentId"))
            .Must(parentId => parentId != Guid.Empty)
            .WithError(GeneralErrors.Invalid("parentId"));

        RuleFor(g => g.Request.Page)
            .GreaterThanOrEqualTo(1)
            .WithError(GeneralErrors.Invalid("page"));

        RuleFor(g => g.Request.PageSize)
            .InclusiveBetween(1, 100)
            .WithError(GeneralErrors.Invalid("pageSize"));
    }
}