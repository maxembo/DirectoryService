using DirectoryService.Contracts.Locations.CreateLocations;
using Shared.Core.Abstractions;

namespace DirectoryService.Application.Locations.Commands.CreateLocations;

public record CreateLocationCommand(CreateLocationRequest LocationRequest) : ICommand;