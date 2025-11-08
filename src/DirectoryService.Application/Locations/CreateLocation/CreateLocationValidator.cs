using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Locations.CreateLocations;
using DirectoryService.Domain.Shared;
using FluentValidation;

namespace DirectoryService.Application.Locations.CreateLocation;

public class CreateLocationValidator : AbstractValidator<CreateLocationDto>
{
    public CreateLocationValidator()
    {
        RuleFor(cl => cl.Name)
            .NotEmpty()
            .NotNull()
            .WithMessage("Name не должен быть пустым.")
            .MaximumLength(Constants.MAX_LOCATION_NAME_LENGTH)
            .WithMessage($"Имя не должен быть больше {Constants.MAX_LOCATION_NAME_LENGTH} символов!");

        RuleFor(cl => cl.Timezone)
            .NotEmpty()
            .NotNull()
            .WithMessage("Timezone не должен быть пустым.")
            .Must(t => TimeZoneInfo.TryFindSystemTimeZoneById(t, out _))
            .WithMessage("Timezone не соответствует iana-коду.")
            .MaximumLength(Constants.MAX_LOCATION_TIMEZONE_LENGTH)
            .WithMessage($"Timezone не должен быть больше {Constants.MAX_LOCATION_TIMEZONE_LENGTH} символов!");

        RuleFor(cl => cl.Address.City)
            .NotEmpty()
            .WithMessage("City не должен быть пустым.")
            .NotNull()
            .MaximumLength(Constants.MAX_LOCATION_ADDRESS_LENGTH)
            .WithMessage($"City не должен быть больше {Constants.MAX_LOCATION_ADDRESS_LENGTH} символов!");

        RuleFor(cl => cl.Address.Country)
            .NotEmpty()
            .WithMessage("Country не должен быть пустым.")
            .NotNull()
            .MaximumLength(Constants.MAX_LOCATION_ADDRESS_LENGTH)
            .WithMessage($"Country не должен быть больше {Constants.MAX_LOCATION_ADDRESS_LENGTH} символов!");

        RuleFor(cl => cl.Address.Street)
            .NotEmpty()
            .WithMessage("Street не должен быть пустым.")
            .NotNull()
            .MaximumLength(Constants.MAX_LOCATION_ADDRESS_LENGTH)
            .WithMessage($"Street не должен быть больше {Constants.MAX_LOCATION_ADDRESS_LENGTH} символов!");

        RuleFor(cl => cl.Address.House)
            .NotEmpty()
            .WithMessage("House не должен быть пустым.")
            .NotNull()
            .MaximumLength(Constants.MAX_LOCATION_ADDRESS_LENGTH)
            .WithMessage($"House не должен быть больше {Constants.MAX_LOCATION_ADDRESS_LENGTH} символов!");
    }
}