using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations.CreateLocations;
using DirectoryService.Application.Locations.Queries.GetLocations;
using DirectoryService.Contracts.Locations.CreateLocations;
using DirectoryService.Contracts.Locations.GetLocations;
using DirectoryService.Presentation.EndpointResults;
using DirectoryService.Presentation.Response;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions;

namespace DirectoryService.Presentation.Locations;

[ApiController]
[Route("/api/locations")]
public class LocationsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Envelope<Guid>>(200)]
    [ProducesResponseType<Envelope>(404)]
    [ProducesResponseType<Envelope>(500)]
    [ProducesResponseType<Envelope>(401)]
    public async Task<EndpointResult<Guid>> Create(
        [FromServices] ICommandHandler<Guid, CreateLocationCommand> handler,
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLocationCommand(request);

        return await handler.Handle(command, cancellationToken);
    }

    [HttpGet]
    public async Task<EndpointResult<PaginationResponse>> GetLocations(
        [FromServices] IQueryHandler<PaginationResponse, GetLocationsQuery> handler,
        [FromQuery] GetLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationsQuery(request);

        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet("dapper")]
    public async Task<EndpointResult<PaginationResponse>> GetLocationsDapper(
        [FromServices] IQueryHandler<PaginationResponse, GetLocationsQuery> handler,
        [FromQuery] GetLocationsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetLocationsQuery(request);

        return await handler.Handle(query, cancellationToken);
    }
}