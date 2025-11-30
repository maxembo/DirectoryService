using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions.CreatePositions;

namespace DirectoryService.Application.Positions.CreatePosition;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;