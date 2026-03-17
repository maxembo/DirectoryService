using DirectoryService.Contracts.Locations.CreateLocations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using SharedService.Core.Validation;

namespace DirectoryService.Application.Locations.Commands.CreateLocations;

public class CreateLocationValidator : AbstractValidator<CreateLocationRequest>
{
    public CreateLocationValidator()
    {
        RuleFor(c => c.Name)
            .MustBeValueObject(LocationName.Create);

        RuleFor(c => c.Timezone)
            .MustBeValueObject(Timezone.Create);

        RuleFor(c => c.Address)
            .MustBeValueObject(
                a
                    => Address.Create(a.City, a.Country, a.Street, a.House));
    }
}