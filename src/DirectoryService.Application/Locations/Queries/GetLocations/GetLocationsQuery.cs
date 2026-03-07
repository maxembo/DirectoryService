using DirectoryService.Contracts.Locations.GetLocations.Requests;
using Shared.Core.Abstractions;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public record GetLocationsQuery(GetLocationsRequest Request) : IQuery;