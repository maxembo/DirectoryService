using CSharpFunctionalExtensions;
using DirectoryService.Contracts.Positions.UpdatePositions;
using DirectoryService.Domain.Positions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.Core.Validation;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Positions.Commands.UpdatePositions;

public class UpdatePositionHandler : ICommandHandler<Guid, UpdatePositionCommand>
{
    private readonly IPositionsRepository _positionsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IValidator<UpdatePositionRequest> _validator;
    private readonly ILogger<UpdatePositionHandler> _logger;

    public UpdatePositionHandler(
        IPositionsRepository positionsRepository,
        ITransactionManager transactionManager,
        IValidator<UpdatePositionRequest> validator,
        ILogger<UpdatePositionHandler> logger)
    {
        _positionsRepository = positionsRepository;
        _transactionManager = transactionManager;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        UpdatePositionCommand command, CancellationToken cancellationToken = default)
    {
        var positionId = PositionId.Create(command.PositionId);
        var request = command.Request;

        var validationResult = await _validator.ValidateAsync(command.Request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToErrors();
        }

        var positionResult = await _positionsRepository.GetByIdAsync(positionId, cancellationToken);
        if (positionResult.IsFailure)
        {
            return positionResult.Error.ToErrors();
        }

        var position = positionResult.Value;

        position.Update(PositionName.Create(request.Name).Value, Description.Create(request.Description).Value);

        var transactionResult = await _transactionManager.SaveChangeAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error.ToErrors();
        }

        _logger.LogInformation("Position with id: {Position.Id} has been updated.", position.Id.Value);

        return position.Id.Value;
    }
}