using DirectoryService.Contracts.Commands;

namespace DirectoryService.Contracts.Locations.CreateLocations;

public record CreateLocationRequest(CreateLocationDto LocationDto) : ICommand;