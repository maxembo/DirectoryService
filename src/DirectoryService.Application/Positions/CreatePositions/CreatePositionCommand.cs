using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions.CreatePositions;

namespace DirectoryService.Application.Positions.CreatePositions;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;