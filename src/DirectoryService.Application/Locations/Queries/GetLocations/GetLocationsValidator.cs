using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.Contracts.Locations.GetLocations.Requests;
using FluentValidation;
using Shared;
using Shared.Validation;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public class GetLocationsValidator : AbstractValidator<GetLocationsRequest>
{
    public GetLocationsValidator()
    {
        RuleFor(g => g.Search)
            .MaximumLength(1000)
            .WithError(GeneralErrors.Invalid("search"));

        RuleFor(g => g.Pagination.Page)
            .GreaterThanOrEqualTo(1)
            .WithError(GeneralErrors.Invalid("page"));

        RuleFor(g => g.Pagination.PageSize)
            .InclusiveBetween(1, 100)
            .WithError(GeneralErrors.Invalid("pageSize"));
    }
}