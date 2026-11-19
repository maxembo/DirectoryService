using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Search.Queries.GlobalSearch;

public class GlobalSearchValidator : AbstractValidator<GlobalSearchQuery>
{
    public GlobalSearchValidator()
    {
        RuleFor(g => g.Q)
            .MinimumLength(2)
            .MaximumLength(100)
            .WithError(GeneralErrors.Invalid("global.search"));
    }
}