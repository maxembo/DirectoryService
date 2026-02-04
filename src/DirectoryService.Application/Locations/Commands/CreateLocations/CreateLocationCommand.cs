using DirectoryService.Contracts.Locations.CreateLocations;
using Shared.Abstractions;

namespace DirectoryService.Application.Locations.Commands.CreateLocations;

public record CreateLocationCommand(CreateLocationRequest LocationRequest) : ICommand;