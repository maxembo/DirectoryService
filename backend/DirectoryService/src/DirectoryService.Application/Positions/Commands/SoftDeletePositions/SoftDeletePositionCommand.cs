using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Positions.Commands.SoftDeletePositions;

public record SoftDeletePositionCommand(Guid PositionId) : ICommand;