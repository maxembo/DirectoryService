using DirectoryService.Contracts.Locations.UpdateLocations;
using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Locations.Commands.UpdateLocations;

public record UpdateLocationCommand(Guid LocationId, UpdateLocationRequest Request) : ICommand;