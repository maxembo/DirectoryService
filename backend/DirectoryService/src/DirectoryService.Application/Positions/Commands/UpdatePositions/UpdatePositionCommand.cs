using DirectoryService.Contracts.Positions.UpdatePositions;
using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Positions.Commands.UpdatePositions;

public record UpdatePositionCommand(Guid PositionId, UpdatePositionRequest Request) : ICommand;