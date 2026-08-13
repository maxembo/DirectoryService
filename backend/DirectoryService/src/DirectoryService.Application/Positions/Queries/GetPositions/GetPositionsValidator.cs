using DirectoryService.Contracts.Positions.GetPositions;
using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Positions.Queries.GetPositions;

public class GetPositionsValidator : AbstractValidator<GetPositionsRequest>
{
    public GetPositionsValidator()
    {
        RuleFor(request => request.Search)
            .MaximumLength(1000)
            .WithError(GeneralErrors.Invalid("search"));

        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1)
            .WithError(GeneralErrors.Invalid("page"));

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 100)
            .WithError(GeneralErrors.Invalid("pageSize"));
    }
}