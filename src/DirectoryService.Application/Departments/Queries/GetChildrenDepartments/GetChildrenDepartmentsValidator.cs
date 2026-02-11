using FluentValidation;
using Shared;
using Shared.Validation;

namespace DirectoryService.Application.Departments.Queries.GetChildrenDepartments;

public class GetChildrenDepartmentsValidator : AbstractValidator<GetChildrenDepartmentsQuery>
{
    public GetChildrenDepartmentsValidator()
    {
        RuleFor(g => g.ParentId)
            .NotNull()
            .WithError(GeneralErrors.Required("parentId"))
            .Must(parentId => parentId != Guid.Empty)
            .WithError(GeneralErrors.Invalid("parentId"));

        RuleFor(g => g.Request.Pagination.Page)
            .GreaterThanOrEqualTo(1)
            .WithError(GeneralErrors.Invalid("page"));

        RuleFor(g => g.Request.Pagination.PageSize)
            .InclusiveBetween(1, 100)
            .WithError(GeneralErrors.Invalid("pageSize"));
    }
}