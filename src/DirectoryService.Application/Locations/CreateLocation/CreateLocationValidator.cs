using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Locations.CreateLocations;
using DirectoryService.Domain.Locations;
using FluentValidation;

namespace DirectoryService.Application.Locations.CreateLocation;

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

        // RuleFor(c => c.LocationDto.Name)
        //     .NotEmpty()
        //     .NotNull()
        //     .WithMessage("Name не должен быть пустым.")
        //     .MaximumLength(Constants.MAX_LOCATION_NAME_LENGTH)
        //     .WithMessage($"Имя не должен быть больше {Constants.MAX_LOCATION_NAME_LENGTH} символов!");
        //
        // RuleFor(c => c.LocationDto.Timezone)
        //     .NotEmpty()
        //     .NotNull()
        //     .WithMessage("Timezone не должен быть пустым.")
        //     .Must(t => TimeZoneInfo.TryFindSystemTimeZoneById(t, out _))
        //     .WithMessage("Timezone не соответствует iana-коду.")
        //     .MaximumLength(Constants.MAX_LOCATION_TIMEZONE_LENGTH)
        //     .WithMessage($"Timezone не должен быть больше {Constants.MAX_LOCATION_TIMEZONE_LENGTH} символов!");
        //
        // RuleFor(c => c.LocationDto.Address.City)
        //     .NotEmpty()
        //     .WithMessage("City не должен быть пустым.")
        //     .NotNull()
        //     .MaximumLength(Constants.MAX_LOCATION_ADDRESS_LENGTH)
        //     .WithMessage($"City не должен быть больше {Constants.MAX_LOCATION_ADDRESS_LENGTH} символов!");
        //
        // RuleFor(c => c.LocationDto.Address.Country)
        //     .NotEmpty()
        //     .WithMessage("Country не должен быть пустым.")
        //     .NotNull()
        //     .MaximumLength(Constants.MAX_LOCATION_ADDRESS_LENGTH)
        //     .WithMessage($"Country не должен быть больше {Constants.MAX_LOCATION_ADDRESS_LENGTH} символов!");
        //
        // RuleFor(c => c.LocationDto.Address.Street)
        //     .NotEmpty()
        //     .WithMessage("Street не должен быть пустым.")
        //     .NotNull()
        //     .MaximumLength(Constants.MAX_LOCATION_ADDRESS_LENGTH)
        //     .WithMessage($"Street не должен быть больше {Constants.MAX_LOCATION_ADDRESS_LENGTH} символов!");
        //
        // RuleFor(c => c.LocationDto.Address.House)
        //     .NotEmpty()
        //     .WithMessage("House не должен быть пустым.")
        //     .NotNull()
        //     .MaximumLength(Constants.MAX_LOCATION_ADDRESS_LENGTH)
        //     .WithMessage($"House не должен быть больше {Constants.MAX_LOCATION_ADDRESS_LENGTH} символов!");
    }
}