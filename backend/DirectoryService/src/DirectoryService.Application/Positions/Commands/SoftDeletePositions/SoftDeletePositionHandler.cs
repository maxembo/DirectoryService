using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using Microsoft.Extensions.Logging;
using SharedService.Core.Abstractions;
using SharedService.Core.Database;
using SharedService.SharedKernel;

namespace DirectoryService.Application.Positions.Commands.SoftDeletePositions;

public class SoftDeletePositionHandler : ICommandHandler<Guid, SoftDeletePositionCommand>
{
    private readonly ILogger<SoftDeletePositionHandler> _logger;
    private readonly IPositionsRepository _positionRepository;
    private readonly ITransactionManager _transactionManager;

    public SoftDeletePositionHandler(
        IPositionsRepository positionRepository,
        ITransactionManager transactionManager,
        ILogger<SoftDeletePositionHandler> logger)
    {
        _positionRepository = positionRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(
        SoftDeletePositionCommand command, CancellationToken cancellationToken = default)
    {
        var positionId = PositionId.Create(command.PositionId);

        Result<Position, Error> positionResult = await _positionRepository.GetByIdAsync(positionId, cancellationToken);
        if (positionResult.IsFailure)
        {
            return positionResult.Error.ToErrors();
        }

        Position? position = positionResult.Value;

        position.MarkAsDelete();

        UnitResult<Error> transactionResult = await _transactionManager.SaveChangeAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error.ToErrors();
        }

        _logger.LogInformation("Position {Position.id} soft deleted successfully.", position.Id.Value);

        return position.Id.Value;
    }
}