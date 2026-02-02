using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.CreateLocations;

namespace DirectoryService.Application.Locations.CreateLocations;

public record CreateLocationCommand(CreateLocationRequest LocationRequest) : ICommand;