using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Locations.Commands.RestoreLocations;

public record RestoreLocationCommand(Guid LocationId) : ICommand;