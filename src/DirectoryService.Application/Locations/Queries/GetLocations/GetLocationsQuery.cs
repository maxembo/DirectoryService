using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.Contracts.Locations.GetLocations.Requests;
using Shared.Abstractions;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public record GetLocationsQuery(GetLocationsRequest Request) : IQuery;