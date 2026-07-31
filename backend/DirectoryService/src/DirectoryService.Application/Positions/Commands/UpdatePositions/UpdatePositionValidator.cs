using DirectoryService.Contracts.Positions.UpdatePositions;
using DirectoryService.Domain.Positions;
using FluentValidation;
using SharedService.Core.Validation;

namespace DirectoryService.Application.Positions.Commands.UpdatePositions;

public class UpdatePositionValidator : AbstractValidator<UpdatePositionRequest>
{
    public UpdatePositionValidator()
    {
        RuleFor(u => u.Name)
            .MustBeValueObject(PositionName.Create);

        RuleFor(u => u.Description)
            .MustBeValueObject(Description.Create);
    }
}