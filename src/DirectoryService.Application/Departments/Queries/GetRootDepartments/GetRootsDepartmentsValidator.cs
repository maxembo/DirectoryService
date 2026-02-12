using DirectoryService.Contracts.Departments.GetDepartments.Requests;
using FluentValidation;
using Shared;
using Shared.Validation;

namespace DirectoryService.Application.Departments.Queries.GetRootDepartments;

public class GetRootsDepartmentsValidator : AbstractValidator<GetRootDepartmentsRequest>
{
    public GetRootsDepartmentsValidator()
    {
        RuleFor(g => g.Page)
            .GreaterThanOrEqualTo(1)
            .WithError(GeneralErrors.Invalid("page"));

        RuleFor(g => g.PageSize)
            .InclusiveBetween(1, 100)
            .WithError(GeneralErrors.Invalid("pageSize"));

        RuleFor(g => g.Prefetch)
            .InclusiveBetween(1, 100)
            .WithError(GeneralErrors.Invalid("prefetch"));
    }
}