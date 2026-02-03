using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments;
using DirectoryService.Contracts.Positions.CreatePositions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Abstractions;
using Shared.Validation;

namespace DirectoryService.Application.Positions.CreatePositions;

public class CreatePositionHandler : ICommandHandler<Guid, CreatePositionCommand>
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IValidator<CreatePositionRequest> _validator;
    private readonly ILogger<CreatePositionHandler> _logger;

    public CreatePositionHandler(
        IPositionsRepository positionsRepository,
        IDepartmentsRepository departmentsRepository,
        IValidator<CreatePositionRequest> validator,
        ILogger<CreatePositionHandler> logger)
    {
        _positionsRepository = positionsRepository;
        _departmentsRepository = departmentsRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        CreatePositionCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToErrors();

            _logger.LogWarning("Validation failed for PositionRequest: {Errors}", errors);
            return errors;
        }

        var positionId = PositionId.CreateNew();
        var name = PositionName.Create(command.Request.Name).Value;
        var description = Description.Create(command.Request.Description).Value;

        var checkExistingAndActiveResult =
            await _departmentsRepository.CheckExistingAndActiveAsync(command.Request.DepartmentIds, cancellationToken);
        if (checkExistingAndActiveResult.IsFailure)
            return checkExistingAndActiveResult.Error;

        var departmentIds = command.Request.DepartmentIds.Select(
                di => new DepartmentPosition(DepartmentPositionId.CreateNew(), DepartmentId.Create(di), positionId))
            .ToList();

        var position = new Position(positionId, name, description, departmentIds);

        var addPositionResult = await _positionsRepository.AddAsync(position, cancellationToken);
        if (addPositionResult.IsFailure)
        {
            return Error.Failure(null, addPositionResult.Error.Message)
                .ToErrors();
        }

        _logger.LogInformation("Location {Position.Id} has been created.", position.Id);

        return positionId.Value;
    }
}