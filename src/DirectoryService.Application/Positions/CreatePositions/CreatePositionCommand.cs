using DirectoryService.Contracts.Positions.CreatePositions;
using Shared.Abstractions;

namespace DirectoryService.Application.Positions.CreatePositions;

public record CreatePositionCommand(CreatePositionRequest Request) : ICommand;