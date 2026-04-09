using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Locations.Commands.SoftDeleteLocations;

public record SoftDeleteLocationCommand(Guid LocationId) : ICommand;