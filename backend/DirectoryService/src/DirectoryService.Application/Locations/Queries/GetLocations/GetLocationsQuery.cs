using DirectoryService.Contracts.Locations.GetLocations.Requests;
using SharedService.Core.Abstractions;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public record GetLocationsQuery(GetLocationsRequest Request) : IQuery;