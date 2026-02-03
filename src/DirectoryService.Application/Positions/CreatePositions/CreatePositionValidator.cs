using DirectoryService.Contracts.Positions.CreatePositions;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Shared;
using Shared.Validation;

namespace DirectoryService.Application.Positions.CreatePositions;

public class CreatePositionValidator : AbstractValidator<CreatePositionRequest>
{
    public CreatePositionValidator()
    {
        RuleFor(c => c.Name)
            .MustBeValueObject(PositionName.Create);

        RuleFor(c => c.Description)
            .MustBeValueObject(Description.Create);

        RuleFor(c => c.DepartmentIds)
            .Must(departmentIds => departmentIds.Distinct().Count() == departmentIds.Length)
            .WithError(GeneralErrors.ArrayContainsDuplicates("position.departmentIds"))
            .NotEmpty()
            .WithError(GeneralErrors.Required("departmentIds"));
    }
}