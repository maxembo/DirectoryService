using DirectoryService.Contracts.Positions.CreatePositions;
using DirectoryService.Domain.Positions;
using FluentValidation;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Positions.Commands.CreatePositions;

public class CreatePositionValidator : AbstractValidator<CreatePositionRequest>
{
    public CreatePositionValidator()
    {
        RuleFor(c => c.Name)
            .MustBeValueObject(PositionName.Create);

        RuleFor(c => c.Description)
            .MustBeValueObject(Description.Create);

        RuleFor(c => c.DepartmentIds)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithError(GeneralErrors.Required("position.departmentIds"))
            .Must(departmentIds =>
                departmentIds.Distinct().Count() == departmentIds.Length)
            .WithError(GeneralErrors.ArrayContainsDuplicates("position.departmentIds"));
    }
}