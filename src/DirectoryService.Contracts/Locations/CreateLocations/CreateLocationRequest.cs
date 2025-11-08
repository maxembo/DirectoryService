using DirectoryService.Contracts.Abstractions;

namespace DirectoryService.Contracts.Locations.CreateLocations;

public record CreateLocationRequest(CreateLocationDto LocationDto) : ICommand;