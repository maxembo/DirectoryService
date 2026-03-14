using DirectoryService.Contracts.Locations.GetLocations.Requests;
using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsValidator : AbstractValidator<GetLocationsRequest>
{
    public GetLocationsValidator()
    {
        RuleFor(g => g.Search)
            .MaximumLength(1000)
            .WithError(GeneralErrors.Invalid("search"));

        RuleFor(g => g.Page)
            .GreaterThanOrEqualTo(1)
            .WithError(GeneralErrors.Invalid("page"));

        RuleFor(g => g.PageSize)
            .InclusiveBetween(1, 100)
            .WithError(GeneralErrors.Invalid("pageSize"));
    }
}