using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Locations.CreateLocations;

namespace DirectoryService.Application.Locations.CreateLocation;

public record CreateLocationCommand(CreateLocationRequest LocationRequest) : ICommand;