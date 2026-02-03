using DirectoryService.Contracts.Locations.GetLocations;
using Shared.Abstractions;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public record GetLocationsQuery(GetLocationsRequest Request) : IQuery;