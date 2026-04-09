using DirectoryService.Contracts.Locations.UpdateLocations;
using DirectoryService.Domain.Locations;
using FluentValidation;
using SharedService.Core.Validation;

namespace DirectoryService.Application.Locations.Commands.UpdateLocations;

public class UpdateLocationValidator : AbstractValidator<UpdateLocationRequest>
{
    public UpdateLocationValidator()
    {
        RuleFor(u => u.Name)
            .MustBeValueObject(LocationName.Create);

        RuleFor(u => u.Address)
            .MustBeValueObject(
                a
                    => Address.Create(a.Country, a.City, a.Street, a.House));

        RuleFor(u => u.Timezone)
            .MustBeValueObject(Timezone.Create);
    }
}